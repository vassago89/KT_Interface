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
    public enum EStatCommand
    {
        Ready,
        Busy,
        Error,
        None,
    }

    public abstract class TcpServerService
    {
        protected ILogger _logger;

        private TcpListener _listener;
        protected TcpClient _client;
        private int _port;

        private CancellationToken _token;

        public TcpServerService(int port, CancellationToken token)
        {
            _port = port;
            _token = token;
            _logger = LogManager.GetCurrentClassLogger();
        }

        public bool IsConnected()
        {
            return _client != null && _client.Connected;
        }

        public async Task Connect()
        {
            try
            {
                _listener = new TcpListener(IPAddress.Any, _port);
                _listener.Start();

                while (_token.IsCancellationRequested == false)
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

        public bool Disconnect()
        {
            if (_listener == null)
                return false;

            _listener.Stop();
            _listener = null;
            return true;
        }

        private async static void AsyncTcpProcess(object o)
        {

            var service = (TcpServerService)o;
            var client = service._client;
            NetworkStream stream = client.GetStream();

            try
            {
                while (service._token.IsCancellationRequested == false)
                {
                    var buff = new byte[1024];
                    var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);

                    if (nbytes > 0)
                    {
                        string message = Encoding.ASCII.GetString(buff, 0, nbytes);
                        await service.DataRecived(message).ConfigureAwait(false);
                    }
                }
            }
            catch (Exception e)
            {
                service._logger.Error(e);
            }
            finally
            {
                //stream.Close();
                //client.Close();
            }
        }

        protected abstract Task DataRecived(string data);
    }
}
