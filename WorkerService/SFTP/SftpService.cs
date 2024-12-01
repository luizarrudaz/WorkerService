using WinSCP;
using WorkerService.Config;

public class SftpService : ISftpService
{
    private readonly Session _session;
    private readonly ILogger<SftpService> _logger;
    private readonly SftpConfigs _config;

    public SftpService(SftpConfigs config, ILogger<SftpService> logger)
    {
        _config = config ?? throw new ArgumentNullException(nameof(config), "Configurações SFTP não podem ser nulas.");
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        _session = new Session();
    }

    public void Connect()
    {
        try
        {
            SessionOptions sessionOptions = new SessionOptions
            {
                Protocol = Protocol.Sftp,
                HostName = _config.SftpConfig.Server,
                PortNumber = _config.SftpConfig.Port,
                UserName = _config.SftpConfig.User,
                Password = _config.SftpConfig.Password,
                SshHostKeyFingerprint = _config.SftpConfig.SshHostKeyFingerprint
            };

            _session.Open(sessionOptions);
            Console.WriteLine("Conexão estabelecida com o servidor.");
            _logger.LogInformation($"Conexão estabelecida com o servidor {_config.SftpConfig.Server}.");
        }
        catch (Exception ex)
        {
            Console.WriteLine("Erro ao conectar ao servidor SFTP.");
            _logger.LogError(ex, "Erro ao conectar ao servidor SFTP.");
            throw;
        }
    }

    public void Disconnect()
    {
        try
        {
            if (_session.Opened)
            {
                _session.Close();
                _logger.LogInformation("Sessão desconectada com sucesso.");
                Console.WriteLine("Conexão com o servidor encerrada.");
            }
            else
            {
                _logger.LogWarning("Tentativa de desconectar, mas a sessão não estava aberta.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Erro ao desconectar do servidor SFTP.");
        }
    }

    public bool UploadFile(string localFilePath, string remoteFilePath)
    {
        try
        {
            if (!_session.Opened)
            {
                _logger.LogError("A sessão SFTP não está aberta. Conecte antes de enviar arquivos.");
                throw new InvalidOperationException("Sessão SFTP não está aberta.");
            }

            _session.PutFiles(localFilePath, remoteFilePath).Check();
            _logger.LogInformation($"Arquivo enviado com sucesso: {localFilePath} -> {remoteFilePath}");
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Erro ao enviar arquivo {localFilePath} para {remoteFilePath}");
            return false;
        }
    }
}