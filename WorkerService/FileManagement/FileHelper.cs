using System;
using System.IO;

namespace WorkerService.FileManagement
{
    public static class FileHelper
    {
        private static FileSystemWatcher _fileSystemWatcher;

        // Cria ou sobrescreve um arquivo com o conteúdo especificado.
        public static void CreateOrOverwriteFile(
            string filePath,
            string content,
            Action disableWatcherAction = null,
            Action enableWatcherAction = null)
        {
            try
            {
                // Garante que o FileSystemWatcher esteja inicializado
                EnsureFileSystemWatcherInitialized(filePath);

                // Desativa o watcher antes de alterar o arquivo, se necessário
                disableWatcherAction?.Invoke();
                _fileSystemWatcher.EnableRaisingEvents = false;

                // Cria ou sobrescreve o arquivo
                File.WriteAllText(filePath, content);

            }
            catch (Exception ex)
            {
                Console.WriteLine($"Erro ao criar ou sobrescrever o arquivo '{filePath}': {ex.Message}");
                throw;
            }
            finally
            {
                // Reativa o watcher após a alteração, se necessário
                enableWatcherAction?.Invoke();
                _fileSystemWatcher.EnableRaisingEvents = true;
            }
        }

        // Inicializa o FileSystemWatcher, caso ainda não tenha sido inicializado
        private static void EnsureFileSystemWatcherInitialized(string filePath)
        {
            if (_fileSystemWatcher == null)
            {
                string directoryPath = Path.GetDirectoryName(filePath);

                // Cria o diretório, se não existir
                if (!Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                }

                _fileSystemWatcher = new FileSystemWatcher(directoryPath)
                {
                    NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName,
                    Filter = Path.GetFileName(filePath),
                    EnableRaisingEvents = true
                };

                // Define o que fazer quando o arquivo for alterado
                _fileSystemWatcher.Changed += (sender, args) =>
                {
                    Console.WriteLine($"O arquivo '{args.FullPath}' foi alterado.");
                };
            }
        }
    }
}