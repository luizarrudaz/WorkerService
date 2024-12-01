using System.Collections.Concurrent;
using WorkerService.Config;
using WorkerService.FileManagement;

public class SftpWorker : BackgroundService
{
    private readonly ILogger<SftpWorker> _logger;
    private readonly ISftpService _sftpService;
    private readonly FileManager _fileManager;
    private readonly ConcurrentQueue<string> _failedFilesQueue = new();
    private readonly string _localDirectory;
    private readonly string _remoteDirectory;

    // Construtor: Inicializa as dependências necessárias para o worker
    public SftpWorker(
        ISftpService sftpService,
        FileManager fileManager,
        ILogger<SftpWorker> logger,
        FilePathsConfig filePathsConfig)
    {
        _sftpService = sftpService;
        _fileManager = fileManager;
        _logger = logger;
        _localDirectory = filePathsConfig.FilePaths.LocalFilesPath;
        _remoteDirectory = filePathsConfig.FilePaths.RemoteFilesPath;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Iniciando o SftpWorker...");

        var config = LoaderConfig.LoadConfig<SeleniumConfig>("config.json");
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                _logger.LogInformation("SftpWorker iniciou.");
                await ProcessFailedFiles();  // Processa arquivos que falharam no envio anterior

                string[] files = _fileManager.GetFiles(_localDirectory);

                if (files.Length == 0)
                {
                    _logger.LogInformation("Nenhum arquivo encontrado no diretório local.");
                }
                else
                {
                    _sftpService.Connect();
                    var deletedFiles = new List<string>();

                    // Processa cada arquivo, enviando ao servidor SFTP
                    foreach (string file in files)
                    {
                        await ProcessFile(file, deletedFiles);
                    }

                    _sftpService.Disconnect();

                    // Deleta arquivos locais que foram enviados com sucesso
                    foreach (var file in deletedFiles)
                    {
                        _fileManager.DeleteFile(file);
                    }

                    if (deletedFiles.Count > 0)
                    {
                        Console.WriteLine("Arquivos locais deletados.");
                    }
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Erro no Worker. {ex}", ex);
            }

            int delay = config.DelayOfWorkers.SftpWorkerDelay;
            _logger.LogInformation("Delay configurado para o SftpWorker: {0}ms", delay);
            await Task.Delay(delay, stoppingToken);
        }
    }

    // Processa cada arquivo, tentando enviá-lo ao servidor SFTP
    private async Task ProcessFile(string localFilePath, List<string> deletedFiles)
    {
        try
        {
            string remoteFilePath = Path.Combine(_remoteDirectory, Path.GetFileName(localFilePath));
            bool uploadSuccess = _sftpService.UploadFile(localFilePath, remoteFilePath);

            if (uploadSuccess)
            {
                deletedFiles.Add(localFilePath);
            }
            else
            {
                EnqueueFileForRetry(localFilePath);
                Console.WriteLine("Arquivo(s) local(is) aguardando reenvio na fila.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao processar arquivo. {ex}", ex);
        }
        await Task.Delay(1500);
    }

    // Processa os arquivos que falharam no envio, tentando reenviá-los
    private async Task ProcessFailedFiles()
    {
        while (_failedFilesQueue.TryDequeue(out var filePath)) // Tenta obter o próximo arquivo da fila
        {
            try
            {
                await ProcessFile(filePath, new List<string>());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao reprocessar arquivo na fila.");
            }
            await Task.Delay(1500);
        }
    }

    // Coloca um arquivo na fila para tentativa de reenvio em outra execução
    private void EnqueueFileForRetry(string filePath)
    {
        _failedFilesQueue.Enqueue(filePath);
        _logger.LogInformation("Arquivo adicionado à fila para reenvio: {FilePath}", filePath);
    }
}