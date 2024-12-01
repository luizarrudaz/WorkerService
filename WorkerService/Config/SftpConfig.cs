namespace WorkerService.Config
{
    public class SftpConfigs
    {
        public SftpConfig SftpConfig { get; set; }

    }
    public class SftpConfig
    {
        public string Protocol { get; set; }
        public string Server { get; set; }
        public int Port { get; set; }
        public string User { get; set; }
        public string Password { get; set; }
        public string SshHostKeyFingerprint { get; set; }
    }
}