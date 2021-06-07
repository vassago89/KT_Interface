using KT_Interface.Core.Comm;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public enum EHostCommand
    {
        Get_stat,
        Start1,
        Start2,
        Stop,
        Req_result,
    }

    public enum EMachineCommand
    {
        Stat,
        Result,
        Ack,
        Nak
    }
    public enum EStatCode
    {
        None = 0,
        Light = 1,
        Camera = 2,
        Inspector = 3,
    }

    public enum EMachineMassage
    {
        Busy,
        Invalid,
        Light,
        Camera,
        Comm
    }

    public class HostCommService : TcpServerService
    {
        private GrabService _grabService;
        private LightControlService _lightControlService;
        private InspectService _inspectionCommService;
        
        private CoreConfig _coreConfig;
        //private ILogger _logger;

        //private TcpListener _listener;
        //private TcpClient _client;
        //private static CancellationToken _token;
        private AppState _appState;

        
        private List<GrabInfo> _grabInfos;
        private EStatCommand _stat;
        private string _wafer;

        //Modify.ach.20210601.Camera 결과값 변수 추가.Start...
        private string resultbufferMessage;
        private string resultbufferJudge;
        private string resultbufferWaferID;
        private string resultbufferFolderPath;
        private string resultbufferCommand;
        //Modify.ach.20210601.Camera 결과값 변수 추가.end...


        public HostCommService(
            InspectService inspectionCommService,
            GrabService grabService,
            LightControlService lightControlService,
            AppState appState,
            CoreConfig coreConfig,
            CancellationToken token) : base(coreConfig.HostPort, token)
        {
            _stat = EStatCommand.Ready;

            _inspectionCommService = inspectionCommService;
            _grabService = grabService;
            _lightControlService = lightControlService;

            _appState = appState;

            _coreConfig = coreConfig;
            
            _grabInfos = new List<GrabInfo>();
            //_token = token;

            //_logger = LogManager.GetCurrentClassLogger();
        }

        //public bool IsConnected()
        //{
        //    return _client != null && _client.Connected;
        //}

        //public async Task Connect()
        //{
        //    try
        //    {
        //        _listener = new TcpListener(IPAddress.Any, _coreConfig.HostPort);
        //        _listener.Start();

        //        while (_token.IsCancellationRequested == false)
        //        {
        //            _client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
        //            await Task.Factory.StartNew(AsyncTcpProcess, this);
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        _logger.Error(e);
        //    }
        //}

        //public bool Disconnect()
        //{
        //    if (_listener == null)
        //        return false;

        //    _listener.Stop();
        //    _listener = null;
        //    return true;
        //}

        //private async static void AsyncTcpProcess(object o)
        //{
            
        //    var service = (HostCommService)o;
        //    var client = service._client;
        //    NetworkStream stream = client.GetStream();

        //    try
        //    {
        //        while (_token.IsCancellationRequested == false)
        //        {
        //            var buff = new byte[1024];
        //            var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);

        //            if (nbytes > 0)
        //            {
        //                string message = Encoding.ASCII.GetString(buff, 0, nbytes);
        //                await service.DataRecived(message).ConfigureAwait(false);
        //            }
        //        }
        //    }
        //    catch (Exception e)
        //    {
        //        service._logger.Error(e);   
        //    }
        //    finally
        //    {
        //        stream.Close();
        //        client.Close();
        //    }
        //}

        protected override async Task DataRecived(string data)
        {
            //if (_appState.IsAutoEnabled == false
            //    || _coreConfig.UseHost == false)
            //    return;

            //if (_appState.IsManualEnabled == false
            //    && _appState.IsGrabEnabled == true)
            //{
            //    Send(EMachineCommand.Error, EMachineMassage.Comm);
            //    return;
            //}

            var index = data.IndexOf('\r');
            if (index < 0 || index > data.Length)
            {
                //Send(EMachineCommand.D:\Project\KT_Interface\KT_Interface.Core\Services\HostCommService.csError, EMachineMassage.Invalid);
                _logger.Error("Invalid Command");
                return;
            }

            var message = new string(data.Take(index).ToArray());
            var messages = message.Split(',');

            EHostCommand command;

            if (Enum.TryParse(messages.First(), out command) == false)
            {
                _logger.Error("Invalid Command");
                //Send(EMachineCommand.Error, EMachineMassage.Invalid);
                return;
            }
            
            switch (command)
            {
                case EHostCommand.Get_stat:
                    SendStat(_stat, EStatCode.None);
                    break;
                case EHostCommand.Start1:
                    Send(EMachineCommand.Ack, EMachineMassage.Comm, "Start1");
                    _stat = EStatCommand.Busy;
                    _wafer = messages[1];
                    _grabInfos.Clear();
                    //_lightControlService.LightOn();
                    //Modify.changhyun_an.2021.05.27.Insert Grab Delay Code.Start...
                    Thread.Sleep(300);
                    //Modify.changhyun_an.2021.05.27.Insert Grab Delay Code.End...

                    _grabService.ImageGrabbed += ImageGrabbed;
                    _grabService.GrabDone += GrabDone;
                    _grabService.StartGrab(_coreConfig.GrabCount);

                    break;
                case EHostCommand.Start2:
                    Send(EMachineCommand.Ack, EMachineMassage.Comm, "Start2");
                    SendResult(EMachineCommand.Result, new InspectResult(EJudgement.Fail, null, null, null));
                    break;
                    
                case EHostCommand.Stop:
                    _stat = EStatCommand.Ready;
                    //ach test
                    Thread.Sleep(1000);
                    Send(EMachineCommand.Ack, EMachineMassage.Comm, "Stop");
                    
                    //_stat = EStatCommand.Ready;
                    //_grabService.Stop();
                    //_lightControlService.LightOff();
                    //if (_lightControlService.LightOff() == false)
                    //{
                    //    //Send(EMachineCommand.Error, EMachineMassage.Light);
                    //    return;
                    //}
                    //Send(EMachineCommand.Ack);
                    break;
                case EHostCommand.Req_result:
                    _stat = EStatCommand.Ready;
                    SendReqResult();
                    break;
            }
        }



        private void ImageGrabbed(GrabInfo grabInfo)
        {
            _grabInfos.Add(grabInfo);
        }
        
        private void GrabDone()
        {
            _grabService.ImageGrabbed -= ImageGrabbed;
            _grabService.GrabDone -= GrabDone;

            _lightControlService.LightOff();
            if (_coreConfig.UseInspector)
            {
                var result = _inspectionCommService.Inspect(_grabInfos, _wafer, new CancellationToken());
                SendResult(EMachineCommand.Result, result);
            }
            else
            {
                SendResult(EMachineCommand.Result, new InspectResult(EJudgement.SKIP,null,null,null));
            }

            _stat = EStatCommand.Ready;
        }
       
        private void SendResult(EMachineCommand command, InspectResult result)
        {
            if (_client == null)
                return;

            string message = string.Empty;
           
            switch (command)
            {
                case EMachineCommand.Result:
                    //ach
                    message = string.Format("{0},{1},{2},{3}\r", command, result.Judgement, result.WaferID, result.FolderPath);
                    break;
            }
            byte[] buff = Encoding.ASCII.GetBytes(message);

            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);

            resultbufferCommand = command.ToString();
            resultbufferJudge = result.Judgement.ToString();
            resultbufferWaferID = result.WaferID;
            resultbufferMessage = result.Resultmessage;
            resultbufferFolderPath = result.FolderPath;
        }
        //Modify.changhyun_an.2021.05.27.result request add.Start...
        private void SendReqResult()
        {
            
            if (_client == null)
                return;

            string message = string.Empty;
            string[] ResultBuffer = resultbufferMessage.Split('\n');
        
            for (int i = 0; i < ResultBuffer.Length - 1; i++)
            {
                message = string.Format("{0}\r", ResultBuffer[i]);

                byte[] buff = Encoding.ASCII.GetBytes(message);

                var stream = _client.GetStream();
                stream.Write(buff, 0, buff.Length);
            }

            message = string.Format("{0},{1},{2},{3}\r", resultbufferCommand, resultbufferJudge, resultbufferWaferID, resultbufferFolderPath);
            
            byte[] buff2 = Encoding.ASCII.GetBytes(message);

            var stream2 = _client.GetStream();
            stream2.Write(buff2, 0, buff2.Length);
            
        }
        //Modify.changhyun_an.2021.05.27.result request add.end...
         

        private void SendStat(EStatCommand command, EStatCode code)
        {
            if (_client == null)
                return;

            string message = string.Format("{0},{1},{2}\r", EMachineCommand.Stat, command, (int)code);
            byte[] buff = Encoding.ASCII.GetBytes(message);

            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);
        }

        private void Send(EMachineCommand command, EMachineMassage machineMassage = EMachineMassage.Invalid, string Mcommand ="")
        {
            if (_client == null)
                return;

            string message = string.Empty;
            switch (command)
            {
                case EMachineCommand.Result:
                case EMachineCommand.Nak:
                    //Modify.changhyun_an.2021.05.27.ack modify.Start...
                    message = string.Format("{0},{1}\r", command, machineMassage);
                    break;
                case EMachineCommand.Ack:
                    message = string.Format("{0},{1}\r", command, Mcommand);
                    //Modify.changhyun_an.2021.05.27.ack modify.end...
                    break;
            }

            byte[] buff = Encoding.ASCII.GetBytes(message);

            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);
        }
    }
}