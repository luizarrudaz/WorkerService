using WorkerService.Config;
using WorkerService.FileManagement;
using System.Text.RegularExpressions;

public class Watcher : BackgroundService
{
    private readonly ILogger<Watcher> _logger;
    private readonly FileSystemWatcher _fileSystemWatcher;
    private readonly string _filePath = @"C:\Users\luiz.arruda\source\repos\WorkerService\WorkerService\reprocess.txt";
    private DateTime _lastReadTime;
    private readonly object _watcherLock = new object();
    public readonly ReprocessConfig _reprocessConfig;

    // Construtor: Inicializa o arquivo de configuração e o FileSystemWatcher
    public Watcher(ILogger<Watcher> logger)
    {
        _logger = logger;
        _lastReadTime = DateTime.MinValue;
        _reprocessConfig = new ReprocessConfig();

        _fileSystemWatcher = ConfigureFileSystemWatcher();
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogWarning("");
        _logger.LogWarning("--------------------------------------------------------------");
        _logger.LogInformation("Iniciando o monitoramento do arquivo de reprocessamento...");
        while (!stoppingToken.IsCancellationRequested)
        {
            await Task.Delay(1000, stoppingToken);
        }
    }

    // Configura o FileSystemWatcher para monitorar as alterações no reprocess.txt
    private FileSystemWatcher ConfigureFileSystemWatcher()
    {
        var watcher = new FileSystemWatcher(Path.GetDirectoryName(_filePath), Path.GetFileName(_filePath))
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.FileName | NotifyFilters.Size,
            EnableRaisingEvents = true // Ativa o monitoramento
        };

        watcher.Changed += OnChanged;
        watcher.Created += OnChanged;
        watcher.Renamed += OnChanged;

        return watcher;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (!ShouldProcessFileChange()) return; // Impede processamento muito rápido em alterações consecutivas

        lock (_watcherLock) // Bloqueia o acesso para evitar condições de corrida
        {
            try
            {
                if (!File.Exists(_filePath))
                {
                    _logger.LogWarning($"O arquivo monitorado '{e.Name}' não existe mais.");
                    return;
                }

                _logger.LogInformation($"Arquivo '{e.Name}' foi modificado. Reprocessamento ativado.");
                ReadFileProperties();
                LogProperties();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao processar alteração no arquivo: {ex.Message}");
            }
        }
    }

    // Verifica se a alteração do arquivo deve ser processada, garantindo que não seja processada de forma repetitiva em intervalos muito curtos
    private bool ShouldProcessFileChange()
    {
        var now = DateTime.Now;
        if ((now - _lastReadTime).TotalMilliseconds < 500) // Intervalo mínimo entre processamentos
            return false;

        _lastReadTime = now;
        return true;
    }

    public void CreateOrOverwriteFile()
    {
        int retryCount = 30;
        int retryDelay = 500;

        for (int attempt = 1; attempt <= retryCount; attempt++)
        {
            try
            {
                _fileSystemWatcher.EnableRaisingEvents = false; // Desativa o monitoramento temporariamente

                string defaultContent = GenerateDefaultFileContent();

                // Verifica se o arquivo já existe e se o conteúdo é o mesmo
                if (File.Exists(_filePath))
                {
                    string existingContent = File.ReadAllText(_filePath);
                    if (existingContent == defaultContent)
                    {
                        _logger.LogInformation("O arquivo já está no estado padrão. Nenhuma ação necessária.");
                        return;
                    }
                }

                FileHelper.CreateOrOverwriteFile(_filePath, defaultContent);
                break;
            }
            catch (IOException ex) when (attempt < retryCount)
            {
                _logger.LogWarning($"Tentativa {attempt} falhou. Retentando em {retryDelay}ms...");
                Thread.Sleep(retryDelay);
            }
            catch (Exception ex)
            {
                _logger.LogError($"Erro ao criar ou sobrescrever o arquivo reprocess.txt: {ex.Message}");
                break; 
            }
            finally
            {
                _fileSystemWatcher.EnableRaisingEvents = true; // Restaura o monitoramento
                ReadFileProperties(); 
            }
        }
    }

    private string GenerateDefaultFileContent()
    {
        return $"Reprocess (true or false): false{Environment.NewLine}" +
               $"CarToBeSearched (type the car): {Environment.NewLine}" +
               $"Date (format as shown in the example): {DateTime.Now:dd-MM-yyyy_HH'h'mm'm'}";
    }

    private void ReadFileProperties()
    {
        try
        {
            string fileContent;

            // Lê o conteúdo do arquivo
            using (var reader = new StreamReader(_filePath))
            {
                fileContent = reader.ReadToEnd();
            }

            // Analisa o conteúdo do arquivo
            var properties = ParseFileProperties(fileContent);

            // Verifica se as propriedades esperadas estão presentes
            if (string.IsNullOrEmpty(properties.reprocess) || string.IsNullOrEmpty(properties.date))
            {
                _logger.LogError("Arquivo reprocess.txt não está no formato correto.");
                return;
            }

            // Carrega as propriedades no objeto de configuração
            _reprocessConfig.Reprocess = ParseReprocessValue(properties.reprocess);
            _reprocessConfig.CarToBeSearched = properties.carToBeSearched;
            _reprocessConfig.Date = properties.date;

            // Verifica se o reprocessamento foi solicitado
            if (_reprocessConfig.Reprocess)
            {
                _logger.LogInformation("Reprocessamento solicitado.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Erro ao ler o arquivo reprocess.txt: {ex.Message}");
        }
    }

    private void LogProperties()
    {
        _logger.LogInformation($"Reprocess: {_reprocessConfig.Reprocess}");
        _logger.LogInformation($"CarToBeSearched: {_reprocessConfig.CarToBeSearched}");
        _logger.LogInformation($"Date: {_reprocessConfig.Date}");
    }

    // Analisa o conteúdo do arquivo reprocess.txt e extrai as propriedades
    private (string reprocess, string carToBeSearched, string date) ParseFileProperties(string fileContent)
    {
        var match = Regex.Match(fileContent,
            @"Reprocess \(.*\):\s*(?<reprocess>true|false)\s*\nCarToBeSearched \(.*\):\s*(?<car>[^\n]*)\s*\nDate \(.*\):\s*(?<date>[^\n]*)");

        if (!match.Success)
        {
            _logger.LogError("O arquivo reprocess.txt não contém todas as propriedades esperadas ou está no formato errado.");
            return (null, null, null);
        }

        return (
            match.Groups["reprocess"].Value.Trim().ToLower(),
            match.Groups["car"].Value.Trim(),
            match.Groups["date"].Value.Trim()
        );
    }

    // Converte o valor de reprocess para um booleano
    private bool ParseReprocessValue(string reprocessValue)
    {
        if (bool.TryParse(reprocessValue, out bool result))
        {
            return result; // Retorna o valor booleano
        }

        _logger.LogError($"O valor '{reprocessValue}' para 'Reprocess' não pôde ser convertido para booleano.");
        return false; // Retorna false se não for possível converter
    }
}