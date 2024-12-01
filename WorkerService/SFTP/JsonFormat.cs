using Newtonsoft.Json;
using NLog;
using WorkerService.Config;

namespace WorkerService.SFTP
{
    public class JsonFormat : IDataFormatter
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();
        private readonly string configFilePath = "config.json";

        public List<CarInfoSelectorsConfig> Items { get; set; }
        public bool Reprocess { get; set; }
        public string Date { get; set; }

        public JsonFormat(List<CarInfoSelectorsConfig> items, bool reprocess, string date)
        {
            this.Items = items;
            this.Reprocess = reprocess;
            this.Date = date;
        }

        public void FormatData(string path)
        {
            string fileName = "veiculos_" + (Reprocess ? Date : DateTime.Now.ToString("dd-MM-yyyy_HH'h'mm'm'")) +
                                            ".json";

            var config = LoaderConfig.LoadConfig<JsonFormat>(configFilePath);
            string outputFilePath = Path.Combine(path, fileName);

            try
            {
                logger.Info("Iniciando a serialização para JSON.");
                string jsonString = JsonConvert.SerializeObject(Items, Formatting.Indented);

                File.WriteAllText(outputFilePath, jsonString);
                logger.Info($"Arquivo formatado para .json com sucesso.");
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao salvar o arquivo JSON: {ex}");
                Console.WriteLine($"Erro ao salvar o arquivo JSON: {ex.Message}");
            }
        }
    }
}
