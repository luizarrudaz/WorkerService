public interface ISftpService
{
    void Connect();
    void Disconnect();
    bool UploadFile(string localFilePath, string remoteFilePath);
}