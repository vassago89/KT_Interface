using KT_Interface.Core.Comm;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenCvSharp;
using OpenCvSharp.Extensions;
using System.Runtime.InteropServices;
using System.IO;
using System.Drawing.Imaging;
using System.Net.Sockets;
using System.Net.NetworkInformation;

namespace KT_Interface.Core.Services
{
    public enum EInspectionCommand
    {
        
    }

    public enum EJudgement
    {
        OK, NG, SKIP, TIMEOUT
    }

    public class InspectResult
    {
        public EJudgement Judgement { get; set; }
        public string WaferID { get; set; }
        public Rect? Rect { get; set; }

        public InspectResult(EJudgement judgement, string _waferID = null, Rect? rect = null)
        {
            Judgement = judgement;
            WaferID = _waferID;
            Rect = rect;
        }
    }

    public class InspectService
    {
        public Action<InspectResult> Inspected { get; set; }

        CoreConfig _coreConfig;
        ILogger _logger;

        Mat _mat;

        public InspectService(
            CoreConfig coreConfig)
        {
            _mat = null;

            _coreConfig = coreConfig;
            _logger = LogManager.GetCurrentClassLogger();
        }
        
        public InspectResult Inspect(GrabInfo grabInfo, string waferID = null)
        {
            if (_mat == null || _mat.Cols != grabInfo.Width || _mat.Rows != grabInfo.Height || _mat.Channels() != grabInfo.Channels)
            {
                switch (grabInfo.Channels)
                {
                    case 1:
                        _mat = new Mat(grabInfo.Height, grabInfo.Width, MatType.CV_8UC1);
                        break;
                    case 3:
                        _mat = new Mat(grabInfo.Height, grabInfo.Width, MatType.CV_8UC3);
                        break;
                }
            }

            Marshal.Copy(grabInfo.Data, 0, _mat.Data, grabInfo.Data.Length);

            if (Directory.Exists(_coreConfig.TempPath) == false)
                Directory.CreateDirectory(_coreConfig.TempPath);
            
            var imagePath = string.Format("{0}.{1}", DateTime.Now.ToString("yyyyMMddHHmmss"), _coreConfig.ImageFormat);
            if (waferID != null)
                imagePath = string.Format("{0}_{1}", waferID, imagePath);

            imagePath = Path.Combine(_coreConfig.TempPath, imagePath);
            imagePath = Path.GetFullPath(imagePath);
            _mat.ImWrite(imagePath);

            var inspectResult = new InspectResult(EJudgement.SKIP);

            var response = Send(imagePath);
            if (string.IsNullOrEmpty(response))
                inspectResult = new InspectResult(EJudgement.TIMEOUT);
            
            if (Inspected != null)
                Inspected(new InspectResult(EJudgement.OK));

            return inspectResult;
        }

        public bool IsConnected()
        {
            try
            {
                var ipgp = IPGlobalProperties.GetIPGlobalProperties();
                var endpoints = ipgp.GetActiveTcpListeners();
                return endpoints.Any(ep => ep.Port == _coreConfig.InspectorPort);
            }
            catch (Exception e)
            {
                return false;
            }
        }

        private string Send(string imagePath)
        {
            try
            {
                lock (this)
                {
                    var client = new TcpClient();
                    client.Connect("localhost", _coreConfig.InspectorPort);
                    var stream = client.GetStream();

                    try
                    {
                        byte[] buff = Encoding.ASCII.GetBytes(string.Format("{0}", imagePath));
                        stream.Write(buff, 0, buff.Length);

                        stream.ReadTimeout = _coreConfig.ResultTimeout;

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
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return null;
            }
        }
    }
}