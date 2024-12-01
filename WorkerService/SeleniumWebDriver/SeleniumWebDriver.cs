using OpenQA.Selenium;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Edge;
using OpenQA.Selenium.Support.UI;
using NLog;
using WorkerService.Config;

namespace WorkerService.SeleniumWebDriver
{
    public class SeleniumWebDriver
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public IWebDriver Driver { get; set; }
        public string Url { get; set; }
        public string CarToBeSearched { get; set; }
        public string DriverName { get; set; }
        public string SearchFieldId { get; set; }
        public string ResultBlock { get; set; }
        public string ResultFieldSelector { get; set; }
        public CarInfoSelectorsConfig CarInfoSelectors { get; set; }
        private readonly ReprocessConfig _reprocessConfig;
        private readonly string _filePath = @"C:\Users\luiz.arruda\source\repos\WorkerService\WorkerService\reprocess.txt";

        public SeleniumWebDriver(string configFilePath, ReprocessConfig reprocessConfig)
        {
            _reprocessConfig = reprocessConfig;  // Recebe a instância de ReprocessConfig

            var config = LoaderConfig.LoadConfig<SeleniumConfig>(configFilePath);
            Url = config.Url;
            DriverName = config.Driver;
            SearchFieldId = config.SearchFieldId;
            ResultBlock = config.ResultBlock;
            ResultFieldSelector = config.ResultFieldSelector;
            CarInfoSelectors = config.CarInfoSelectors;

            // Usando o CarToBeSearched de ReprocessConfig, caso Reprocess seja true
            CarToBeSearched = _reprocessConfig.Reprocess ? _reprocessConfig.CarToBeSearched : config.CarToBeSearched;

            if (string.IsNullOrEmpty(Url))
            {
                logger.Error("URL não pode ser nulo ou vazio.");
                throw new ArgumentException("URL não pode ser nulo ou vazio.");
            }

            if (DriverName.ToLower() == "chrome")
            {
                ChromeOptions options = new ChromeOptions();
                options.AddArguments("--headless=new");

                Driver = new ChromeDriver(options);
                Console.WriteLine("Driver Chrome iniciado.");
                logger.Info("Driver Chrome iniciado.");
            }
            else if (DriverName.ToLower() == "edge")
            {
                Driver = new EdgeDriver();
                Console.WriteLine("Driver Edge iniciado.");
                logger.Info("Driver Edge iniciado.");
            }
            else
            {
                logger.Error($"Driver para {DriverName} não implementado.");
                throw new NotImplementedException($"Driver para {DriverName} não implementado.");
            }
        }

        public void Start()
        {
            try
            {
                Driver.Navigate().GoToUrl(Url);
                logger.Info($"Navegando para a URL: {Url}");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao iniciar a navegação para a URL {Url}: {ex}");
                throw;
            }
        }

        public void Stop()
        {
            try
            {
                Driver?.Quit();
                logger.Info("Navegador encerrado.");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao encerrar o navegador: {ex}");
            }
        }

        public List<CarInfoSelectorsConfig> PerformSearch(string searchTerm)
        {
            List<CarInfoSelectorsConfig> resultData = new List<CarInfoSelectorsConfig>();
            Console.WriteLine("Pesquisa iniciada.");
            logger.Info($"Iniciando pesquisa por: {searchTerm}");

            try
            {
                WebDriverWait wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(30));
                IWebElement searchField = wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementToBeClickable(By.Id(SearchFieldId)));
                searchField.SendKeys(searchTerm);

                Thread.Sleep(3500);
                searchField.SendKeys(Keys.Enter);

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.CssSelector(ResultBlock)));

                wait.Until(SeleniumExtras.WaitHelpers.ExpectedConditions.ElementExists(By.XPath(ResultFieldSelector)));
                IReadOnlyCollection<IWebElement> results = Driver.FindElements(By.XPath(ResultFieldSelector));

                Console.WriteLine("Carregando resultados.");
                foreach (var result in results)
                {
                    try
                    {
                        string name = result.FindElement(By.CssSelector(CarInfoSelectors.Name))?.Text ?? string.Empty;
                        name = name.Replace("\r\n", " ").Trim();
                        string description = result.FindElement(By.CssSelector(CarInfoSelectors.Description))?.Text ?? string.Empty;
                        string price = result.FindElement(By.CssSelector(CarInfoSelectors.Price))?.Text ?? string.Empty;
                        string year = result.FindElement(By.CssSelector(CarInfoSelectors.Year))?.Text ?? string.Empty;
                        string origin = result.FindElement(By.CssSelector(CarInfoSelectors.Origin))?.Text ?? string.Empty;

                        resultData.Add(new CarInfoSelectorsConfig
                        {
                            Name = name,
                            Description = description,
                            Price = price,
                            Year = year,
                            Origin = origin
                        });
                    }
                    catch (NoSuchElementException ex)
                    {
                        logger.Warn($"Elemento ausente em um resultado: {ex.Message}");
                    }
                }
                Console.WriteLine($"Resultados carregados.");
                logger.Info($"Foram encontrados {results.Count} resultados.");
            }
            catch (TimeoutException ex)
            {
                Console.WriteLine($"Tempo de espera excedido para encontrar elementos: {ex}");
                logger.Error($"Tempo de espera excedido para encontrar elementos: {ex}");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro durante a pesquisa por '{searchTerm}': {ex}");
            }
            return resultData;
        }
    }
}
