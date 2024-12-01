using WorkerService;
using WorkerService.Config;
using WorkerService.FileManagement;
using NLog.Extensions.Logging;

var host = Host.CreateDefaultBuilder(args)
    .ConfigureServices(services =>
    {
        services.AddHostedService<Watcher>();
        services.AddHostedService<SeleniumWorker>();
        services.AddHostedService<SftpWorker>();

        var sftpConfig = LoaderConfig.LoadConfig<SftpConfigs>("config.json");
        var filePathsConfig = LoaderConfig.LoadConfig<FilePathsConfig>("config.json");

        services.AddSingleton(sftpConfig);
        services.AddSingleton(filePathsConfig);
        services.AddSingleton<ISftpService, SftpService>();
        services.AddSingleton<FileManager>();
        services.AddSingleton<ReprocessConfig>();
        services.AddSingleton<Watcher>();
    })
    .ConfigureLogging(logging =>
    {
        logging.ClearProviders();
        logging.AddNLog();
    })
    .Build();

await host.RunAsync();