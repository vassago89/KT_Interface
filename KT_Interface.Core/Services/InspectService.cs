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
using System.Threading;

namespace KT_Interface.Core.Services
{
    public enum EInspectorCommand
    {
        Get_stat, Start       
    }

    public enum EJudgement
    {
        Pass, Fail, SKIP, TIMEOUT
    }

    public enum ESubJudgement
    {
        Pass, Fail
    }


    public class InspectResult
    {
        public EJudgement Judgement { get; set; }
        public string WaferID { get; set; }
        //Modify.ach.20210601.Camera 결과값 매개변수 추가.Start...
        public string Resultmessage { get; set; }
        public string FolderPath { get; set; }
        //Modify.ach.20210601.Camera 결과값 매개변수 추가.end...
        public Rect? Rect { get; set; }

        public IEnumerable<SubResult> SubResults { get; }

        public InspectResult(EJudgement judgement, IEnumerable<SubResult> subResults, string waferID = null, Rect? rect = null, string resultmessage ="", string folderPath = "")
        {
            Judgement = judgement;
            SubResults = subResults;
            WaferID = waferID;
            Rect = rect;
            //Modify.ach.20210601.Camera 결과값 매개변수 추가.Start...
            Resultmessage = resultmessage;
            FolderPath = folderPath;
            //Modify.ach.20210601.Camera 결과값 매개변수 추가.End...
        }
    }



    public class SubResult
    {
        public string Message { get; }
        public ESubJudgement SubJudgement { get; }
        public string FilePath { get; }
        
        public SubResult(string message)
        {
            Message = message;

            var messages = message.Split(',');

            /////////////////////////////////////
            //Result 형식 체크 필요합니다
            /////////////////////
            SubJudgement = (ESubJudgement)Enum.Parse(typeof(ESubJudgement), messages[0]);
            FilePath = messages[1];
        }

        public GrabInfo GetGrabinfo()
        {
            var mat = Cv2.ImRead(FilePath);

            var length = mat.Width * mat.Height * mat.Channels();
            var buffer = new byte[length];
            Marshal.Copy(mat.Data, buffer, 0, length);

            return new GrabInfo(EGrabResult.Success, mat.Width, mat.Height, mat.Channels(), buffer);
        }

    }

    public class InspectService : TcpServerService
    {
        public Action<InspectResult> Inspected { get; set; }

        CoreConfig _coreConfig;
        Mat _mat;
        EStatCommand _stat;

        /// <summary>
        /// readyflag = 결과값 완료Flag
        /// </summary>
        private int readyflag = 0;
        /// <summary>
        /// 결과메세지
        /// </summary>
        private string Resultmessages = "";
  
        public InspectService(
            CoreConfig coreConfig,
            CancellationToken token) : base(coreConfig.InspectorPort, token)
       {
            _stat = EStatCommand.None;
            _mat = null;

            _coreConfig = coreConfig;
            _logger = LogManager.GetCurrentClassLogger();
        }

        private void Save(GrabInfo grabInfo, string imagePath)
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
            _mat.ImWrite(string.Format("{0}.{1}", imagePath, _coreConfig.ImageFormat));
        }

        public InspectResult Inspect(IEnumerable<GrabInfo> grabInfos, string waferID, CancellationToken token)
        {
            if (grabInfos.Count() == 0)
                return null;

            if (Directory.Exists(_coreConfig.GetTempPath()) == false)
                Directory.CreateDirectory(_coreConfig.GetTempPath());

            var directoryPath = _coreConfig.GetTempPath();

            if (waferID != null)
                directoryPath = Path.Combine(directoryPath, waferID);

            if (Directory.Exists(directoryPath) == false)
                Directory.CreateDirectory(directoryPath);

            int count = 1;
            foreach (var grabInfo in grabInfos)
            {
                //Modify.ach.20210601.Camera 예외처리.Start...
                if (grabInfo.Data != null) 
                { 
                    Save(grabInfo, Path.Combine(directoryPath, count.ToString()));
                    count++;
                }
                //Modify.ach.20210601.Camera 예외처리.End
            }

            var command = GetStat(token);
            switch (command)
            {
                case EStatCommand.Ready:
                    break;
                case EStatCommand.Busy:
                case EStatCommand.Error:
                case EStatCommand.None:
                    break;
            }

            Inspect(waferID, directoryPath, token);
            //결과값에 따른 result 값 변경해야함.
            var inspectResult = new InspectResult(EJudgement.Fail, ParseMessages());
            inspectResult.Resultmessage = Resultmessages;
            inspectResult.FolderPath = directoryPath;
            inspectResult.WaferID = waferID;

            //?
            if (Inspected != null)
                Inspected(inspectResult);

            //Send(Path.GetFullPath(directoryPath));

            return inspectResult;
        }

        

        private EStatCommand GetStat(CancellationToken token)
        {
            if (_client == null)
                return EStatCommand.None;
            
            _stat = EStatCommand.None;

            byte[] buff = Encoding.ASCII.GetBytes("Get_stat");
            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);

            while (token.IsCancellationRequested == false)
            {
                if (_stat != EStatCommand.None)
                    break;
            }
            return _stat;
        }
        

        private bool Inspect(string waferID, string imagePath, CancellationToken token2)
        {
            if (_client == null)
                return false;
            //Modify.ach.20210601.Inspect 결과값 완료대기.Start...
            readyflag = 1;
            var message = string.Format("Start,{0},{1}", waferID, imagePath);

            byte[] buff = Encoding.ASCII.GetBytes(message);
            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);
            //While 예외처리 필요
            while (token2.IsCancellationRequested == false)
            {
                if (readyflag == 0)
                    break;
            }
            return true;
            //Modify.ach.20210601.Inspect 결과값 완료대기.End...
        }
     
        
        protected override async Task DataRecived(string data)
        {
            var messages = data.Split(',');
            if (messages.Length == 0)
                return;
            //Get_Stat
            if (messages.First() == "Stat" && messages.Length == 3)
            {
                var command = (EStatCommand)Enum.Parse(typeof(EStatCommand), messages[1]);
                _stat = command;
                return;
            }
            //결과값 조건 수정해야함.
            if (messages.Length > 5)
            {
                Resultmessages = data;
                readyflag = 0;
                return;            
            }
            //throw new NotImplementedException();
        }

        private IEnumerable<SubResult> ParseMessages()
        {
            //Resultmessages to SubResultList
            var messages = Resultmessages.Split('\n');
            var subResults = new List<SubResult>();

            foreach (var message in messages)
                subResults.Add(new SubResult(message));

            return subResults;
        }
    }
}