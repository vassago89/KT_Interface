using KT_Interface.Core.Comm;
using KT_Interface.Core.Infos;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public enum EHostCommand
    {
        Start1,
        Start2,
        Stop,
        Req_result,
    }

    public enum EMachineCommand
    {
        Result,
        Ack,
        Nak
    }

    public enum EMachineMassage
    {
        Busy,
        Invalid,
        Light,
        Camera,
        Comm
    }

    public class HostCommService
    {
        private string _last;
        private GrabService _grabService;
        private LightControlService _lightControlService;
        private InspectService _inspectionCommService;
        
        private CoreConfig _coreConfig;
        private ILogger _logger;

        private TcpListener _listener;
        private TcpClient _client;

        public HostCommService(
            InspectService inspectionCommService,
            GrabService grabService,
            LightControlService lightControlService,
            CoreConfig coreConfig, 
            LogFactory factory)
        {
            _inspectionCommService = inspectionCommService;
            _grabService = grabService;
            _lightControlService = lightControlService;

            _coreConfig = coreConfig;
            
            _logger = factory.GetCurrentClassLogger();
        }

        public async Task Connect()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _coreConfig.HostPort);
                _listener.Start();

                while (true)
                {
                    _client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                    await Task.Factory.StartNew(AsyncTcpProcess, this);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private async static void AsyncTcpProcess(object o)
        {
            
            var service = (HostCommService)o;
            var client = service._client;
            NetworkStream stream = client.GetStream();

            try
            {
                var buff = new byte[1024];
                var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);

                if (nbytes > 0)
                {
                    string message = Encoding.ASCII.GetString(buff, 0, nbytes);
                    await service.DataRecived(message).ConfigureAwait(false);
                }
            }
            catch (Exception e)
            {
                service._logger.Error(e);   
            }
            finally
            {
                stream.Close();
                client.Close();
            }
        }

        private async Task DataRecived(string data)
        {
            var index = data.IndexOf('\n');
            if (index < 0 || index > data.Length)
            {
                Send(EMachineCommand.Nak, EMachineMassage.Invalid);
                return;
            }

            var message = new string(data.Take(index).ToArray());
            var messages = message.Split(',');

            EHostCommand command;

            if (Enum.TryParse(messages.First(), out command) == false)
            {
                Send(EMachineCommand.Nak, EMachineMassage.Invalid);
                return;
            }
            
            switch (command)
            {
                case EHostCommand.Start1:
                case EHostCommand.Start2:
                    if (_lightControlService.LightOn() == false)
                    {
                        Send(EMachineCommand.Nak, EMachineMassage.Light);
                        return;
                    }

                    var grabInfo = await _grabService.Grab();
                    if (grabInfo == null)
                    {
                        Send(EMachineCommand.Nak, EMachineMassage.Camera);
                        return;
                    }

                    Send(EMachineCommand.Ack);

                    _inspectionCommService.Start(grabInfo.Value, messages[1]);

                    break;
                case EHostCommand.Stop:
                    if (_grabService.Stop() == false)
                    {
                        Send(EMachineCommand.Nak, EMachineMassage.Camera);
                        return;
                    }

                    if (_lightControlService.LightOff() == false)
                    {
                        Send(EMachineCommand.Nak, EMachineMassage.Light);
                        return;
                    }

                    Send(EMachineCommand.Ack);

                    break;
                case EHostCommand.Req_result:
                    break;
            }
        }

        private void Send(EMachineCommand command, object obj = null)
        {
            if (_client == null)
                return;

            string message = string.Empty;
            switch (command)
            {
                case EMachineCommand.Result:
                case EMachineCommand.Nak:
                    message = $"{command},{obj}";
                    break;
                case EMachineCommand.Ack:
                    message = $"{command}";
                    break;
            }

            byte[] buff = Encoding.ASCII.GetBytes(message);

            var stream = _client.GetStream();
            stream.Write(buff, 0, buff.Length);
        }
    }
}