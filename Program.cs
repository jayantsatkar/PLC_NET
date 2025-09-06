
using log4net;
using log4net.Config;
using Microsoft.Extensions.Configuration;
using PLCConnection;
using System.Reflection;
internal class Program
{
    private static readonly ILog Logger = LogManager.GetLogger(typeof(Program));

    static void Main(string[] args)
    {
        try
        {
            var config = new ConfigurationBuilder()
                    .SetBasePath(AppContext.BaseDirectory)
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                    .Build();

            BasicConfigurator.Configure();
            var logRepository = LogManager.GetRepository(Assembly.GetEntryAssembly());
            XmlConfigurator.Configure(logRepository, new FileInfo("log4net.config"));

            Logger.Info("Application Started .NET 9");
            string FileOkSourcePath = config.GetSection("PLC")["FileOkSourcePath"];//  "10.168.158.230"; // PLC IP address

            if (System.IO.Directory.Exists(FileOkSourcePath))
            {
                FileSystemWatcher watcher = new FileSystemWatcher(FileOkSourcePath);
                watcher.IncludeSubdirectories = true;
                watcher.Created += FileSystemWatcher_Created_Ok;
                watcher.EnableRaisingEvents = true;
            }

            PLC.GetDMCNumber();

            Console.WriteLine("Press ENTER to exit...");
            Console.ReadLine();
        }

        catch (Exception ex) 
        { 
            Logger.Error(ex);
        }
    }

    private static void FileSystemWatcher_Created_Ok(object sender, FileSystemEventArgs e)
    {
        try
        {
            Thread.Sleep(2000);
            //PLC plc = new PLC();
            string DMC = PLC.GetDMCNumber();

            string folder = Path.GetDirectoryName(e.FullPath)!;
            string oldName = Path.GetFileName(e.FullPath);
            string newName = DMC + "_" + oldName;
            string newPath = Path.Combine(folder, newName);

            File.Move(e.FullPath, newPath);
        }

        catch (Exception ex)
        {
            Logger.Error(ex);
        }
        
    }
}

