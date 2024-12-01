using Newtonsoft.Json;
using NLog;

namespace WorkerService.Config
{
    public class LoaderConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public static T LoadConfig<T>(string configFilePath)
        {
            try
            {
                string jsonContent = File.ReadAllText(configFilePath);
                var config = JsonConvert.DeserializeObject<T>(jsonContent);

                return config;
            }
            catch (Exception ex)
            {
                logger.Error($"Erro ao carregar as configurações do arquivo {configFilePath}: {ex}");
                throw new InvalidOperationException($"Erro ao carregar as configurações: {ex}");
            }
        }
    }
}
