using NLog;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class StoringService
    {
        private CoreConfig _coreConfig;
        private CancellationToken _token;

        private ILogger _logger;

        public StoringService(
            CoreConfig coreConfig,
            CancellationToken token)
        {
            _coreConfig = coreConfig;
            _token = token;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public void Run()
        {
            Task.Run(async () =>
            {
                var names = Enum.GetNames(typeof(EJudgement)).ToList();
                names.Add("Temp");

                while (_token.IsCancellationRequested == false)
                {
                    if (Directory.Exists(_coreConfig.ResultPath))
                    {
                        var files = new List<string>();

                        var directoryInfo = new DirectoryInfo(_coreConfig.ResultPath);

                        foreach (var dir in directoryInfo.GetDirectories())
                        {
                            if (names.Any(n => n == dir.Name) == false)
                                continue;
                            
                            foreach (var file in dir.GetFiles())
                            {
                                if (file.Extension == ImageFormat.Bmp.ToString()
                                || file.Extension == ImageFormat.Png.ToString()
                                || file.Extension == ImageFormat.Jpeg.ToString())
                                {
                                    if (DateTime.Now - file.CreationTime > new TimeSpan(_coreConfig.ResultStoringDays, 0, 0, 0, 0))
                                    {
                                        files.Add(file.Name);
                                        file.Delete();
                                    }
                                }
                            }
                        }

                        if (files.Count > 0)
                            _logger.Info(string.Format("{0} result deleted", files.Count));
                    }

                    if (Directory.Exists(_coreConfig.LogPath))
                    {
                        var files = new List<string>();

                        var directoryInfo = new DirectoryInfo(_coreConfig.ResultPath);

                        foreach (var file in directoryInfo.GetFiles())
                        {
                            if (DateTime.Now - file.CreationTime > new TimeSpan(_coreConfig.LogStoringDays, 0, 0, 0, 0))
                            {
                                files.Add(file.Name);
                                file.Delete();
                            }
                        }

                        if (files.Count > 0)
                            _logger.Info(string.Format("{0} log deleted", files.Count));
                    }
                    
                    await Task.Delay(new TimeSpan(0, _coreConfig.StoringCheckDelay, 0), _token).ConfigureAwait(false);
                }
            });
        }
    }
}
