using WorkerService.SFTP;
using WorkerService.Config;

namespace WorkerService
{
    public class SeleniumWorker : BackgroundService
    {
        private readonly ILogger<SeleniumWorker> _logger;
        private const string ConfigFilePath = "config.json";
        private readonly string _localDirectory;
        private readonly ReprocessConfig _reprocessConfig;
        private readonly Watcher _watcher;

        // Construtor: Recebe depend�ncias necess�rias para o funcionamento do worker
        public SeleniumWorker(ILogger<SeleniumWorker> logger, FilePathsConfig filePathsConfig, ReprocessConfig reprocessConfig, Watcher watcher)
        {
            _logger = logger;
            _localDirectory = filePathsConfig.FilePaths.LocalFilesPath;
            _reprocessConfig = reprocessConfig;
            _watcher = watcher;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var config = LoaderConfig.LoadConfig<SeleniumConfig>(ConfigFilePath);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Verifica se o campo 'CarToBeSearched' est� configurado no arquivo JSON
                    if (string.IsNullOrEmpty(config?.CarToBeSearched))
                    {
                        _logger.LogError("Campo 'CarToBeSearched' n�o definido.");
                        throw new InvalidOperationException("Campo 'CarToBeSearched' n�o pode ser vazio.");
                    }

                    // L�gica de escolha do valor de 'CarToBeSearched':
                    // Se o reprocessamento estiver ativado, usa a configura��o de reprocessamento. Caso contr�rio, usa o valor no arquivo de configura��o.
                    string carToBeSearched = _watcher._reprocessConfig.Reprocess ? _watcher._reprocessConfig.CarToBeSearched : config.CarToBeSearched;

                    // Cria o SeleniumWebDriver, passando a configura��o e as op��es de reprocessamento
                    var webDriver = new SeleniumWebDriver.SeleniumWebDriver(ConfigFilePath, _reprocessConfig);
                    webDriver.Start();
                    var results = webDriver.PerformSearch(carToBeSearched); // Realiza a busca no site
                    webDriver.Stop();

                    // Serializa os resultados da pesquisa para um arquivo JSON
                    var jsonSerializer = new JsonFormat(results, _watcher._reprocessConfig.Reprocess, _watcher._reprocessConfig.Date);
                    jsonSerializer.FormatData(_localDirectory);
                    _logger.LogInformation("Resultados serializados para JSON.");

                    // Ap�s salvar o JSON, reinicia o arquivo 'reprocess.txt' para o pr�ximo ciclo
                    _watcher.CreateOrOverwriteFile();
                    _logger.LogInformation("Arquivo reprocess.txt resetado.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Erro no Worker.");
                }

                // Delay configurado no arquivo de configura��o antes de iniciar o pr�ximo ciclo de execu��o
                int delay = config.DelayOfWorkers.SeleniumWorkerDelay;
                _logger.LogInformation("Delay configurado: {0}ms", delay);
                await Task.Delay(delay, stoppingToken);
            }
        }
    }
}