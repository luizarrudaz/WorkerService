namespace WorkerService.FileManagement
{
    public class FileManager
    {
        private readonly ILogger<FileManager> _logger;

        public FileManager(ILogger<FileManager> logger)
        {
            _logger = logger;
        }

        public string[] GetFiles(string directory)
        {
            if (!Directory.Exists(directory))
            {
                _logger.LogError("Diretório não encontrado: {Directory}", directory);
                return Array.Empty<string>();
            }

            return Directory.GetFiles(directory);
        }

        public void DeleteFile(string filePath)
        {
            try
            {
                File.Delete(filePath);
                _logger.LogInformation("Arquivos locais deletados.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao deletar arquivos locais.");
            }
        }
    }

}