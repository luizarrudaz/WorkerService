using NLog;

namespace WorkerService.Config
{
    public class FilePathsConfig
    {
        private static readonly Logger logger = LogManager.GetCurrentClassLogger();

        public string LocalFilesPath { get; set; }
        public string RemoteFilesPath { get; set; }

        public FilePathsConfig FilePaths { get; set; }
    }
}