using KT_Interface.Core.Comm;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Sockets;

namespace KT_Interface.Core.Services
{
    public enum EInspectionCommand
    {
        
    }

    public class InspectService
    {
        CoreConfig _coreConfig;
        ILogger _logger;

        Mat _mat;

        public InspectService(
            CoreConfig coreConfig, 
            LogFactory factory)
        {
            _mat = null;

            _coreConfig = coreConfig;
            _logger = factory.GetCurrentClassLogger();
        }
        
        public bool Start(GrabInfo grabInfo, string waferID = null)
        {
            if (_mat == null || _mat.Cols != grabInfo.Width || _mat.Rows != grabInfo.Height || _mat.Channels() != grabInfo.Channels)
            {
                switch (grabInfo.Channels)
                {
                    case 1:
                        _mat = new Mat(grabInfo.Height, grabInfo.Width, MatType.CV_8UC3);
                        break;
                    case 3:
                        _mat = new Mat(grabInfo.Height, grabInfo.Width, MatType.CV_8UC1);
                        break;
                }
            }
            _mat.SetArray(grabInfo.Data);

            if (Directory.Exists(_coreConfig.TempPath) == false)
                Directory.CreateDirectory(_coreConfig.TempPath);
            
            var imagePath = $"{DateTime.Now.ToString("yyyyMMddHHmmss")}.{_coreConfig.ImageFormat}";
            if (waferID != null)
                imagePath = $"{waferID}_{imagePath}";

            imagePath = Path.Combine(_coreConfig.TempPath, imagePath);
            imagePath = Path.GetFullPath(imagePath);
            _mat.ImWrite(imagePath);

            var result = Send(imagePath);

            return true;
        }

        private string Send(string imagePath)
        {
            try
            {
                var client = new TcpClient();
                client.Connect("localhost", _coreConfig.InspectorPort);
                var stream = client.GetStream();

                try
                {
                    byte[] buff = Encoding.ASCII.GetBytes($"{imagePath}");
                    stream.Write(buff, 0, buff.Length);

                    byte[] outbuf = new byte[1024];
                    int nbytes = stream.Read(outbuf, 0, outbuf.Length);
                    string output = Encoding.ASCII.GetString(outbuf, 0, nbytes);
                    return output;
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return null;
                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }
    }
}