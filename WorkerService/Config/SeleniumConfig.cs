public class SeleniumConfig
{
    public string LocalFilesPath { get; set; }
    public string Url { get; set; }
    public string Driver { get; set; }
    public string CarToBeSearched { get; set; }
    public string SearchFieldId { get; set; }
    public string ResultBlock { get; set; }
    public string ResultFieldSelector { get; set; }
    public CarInfoSelectorsConfig CarInfoSelectors { get; set; }
    public DelayOfWorkers DelayOfWorkers { get; set; }
}

public class DelayOfWorkers
{
    public int SeleniumWorkerDelay { get; set; }
    public int SftpWorkerDelay { get; set; }
}
