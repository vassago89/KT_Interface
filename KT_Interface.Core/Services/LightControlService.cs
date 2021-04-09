using KT_Interface.Core.Comm;
using NLog;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace KT_Interface.Core.Services
{
    public class LightControlService 
    {
        private int[] _storage;

        private SerialComm _serialComm;
        private CoreConfig _coreConfig;
        private ILogger _logger;
        ManualResetEvent _resetEvent;
        private char[] _buffer;

        public LightControlService(CoreConfig coreConfig)
        {
            _serialComm = new SerialComm();
            _coreConfig = coreConfig;

            _logger = LogManager.GetCurrentClassLogger();

            _serialComm.DataReceived += DataReceived;

            _resetEvent = new ManualResetEvent(false);
        }

        private void DataReceived(string message)
        {
            var index = Array.IndexOf(_buffer, (char)0);
            Array.Copy(message.ToCharArray(), 0, _buffer, index, message.Length);

            if (Array.IndexOf(_buffer, 0) == -1)
            {
                _resetEvent.Set();
                Array.Clear(_buffer, 0, _buffer.Length);
            }
        }

        public bool IsConnected()
        {
            lock (this)
            {
                _resetEvent.Reset();
                if (_serialComm.Write("$4100011"))
                {
                    if (_resetEvent.WaitOne(_coreConfig.ReponseTimeout))
                    {
                        Thread.Sleep(10);
                        return true;
                    }
                }

                Thread.Sleep(10);
                return false;
            }
        }

        public bool Connect()
        {
            var info = _coreConfig.LightSerialInfo;
            try
            {
                _logger.Info(string.Format("Connect - Port:{0}, BaudRate:{1}, Parity:{2}, DataBits:{3}, StopBits{4}", info.PortName, info.BaudRate, info.Parity, info.DataBits, info.StopBits));
                _buffer = new char[info.DataBits];
                return _serialComm.Connect(info.PortName, info.BaudRate, info.Parity, info.DataBits, info.StopBits);
            }
            catch (Exception e)
            {
                _logger.Error(e);
                return false;
            }
        }

        public bool Disconnect()
        {
            _logger.Info("Disconnect");
            return _serialComm.Disconnect();
        }

        public bool LightOn()
        {
            lock (this)
            {
                try
                {
                    _resetEvent.Reset();
                    string data = "$51000";
                    if (_serialComm.Write(data + GetCheckSum(data)))
                    {
                        if (_resetEvent.WaitOne(_coreConfig.ReponseTimeout))
                        {
                            Thread.Sleep(10);
                            return true;
                        }
                    }
                    
                    Thread.Sleep(10);
                    return false;
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return false;
                }
            }
        }

        public bool LightOff()
        {
            lock (this)
            {
                try
                {
                    _resetEvent.Reset();

                    string data = "$61000";
                    if (_serialComm.Write(data + GetCheckSum(data)))
                    {
                        if (_resetEvent.WaitOne(_coreConfig.ReponseTimeout))
                        {
                            Thread.Sleep(10);
                            return true;
                        }
                    }

                    Thread.Sleep(10);
                    return false;
                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return false;
                }
            }
        }

        public bool SetValue(params byte[] values)
        {
            lock (this)
            {
                try
                {

                    _resetEvent.Reset();
                    bool result = true;

                    for (int i = 0; i < values.Length; i++)
                    {
                        string data = string.Format("$3{0}0{1}", i + 1, BitConverter.ToString(new byte[] { values[i] }));
                        if (_serialComm.Write(data + GetCheckSum(data)))
                        {
                            if (_resetEvent.WaitOne(_coreConfig.ReponseTimeout) == false)
                            {
                                result = false;
                                break;
                            }

                            Thread.Sleep(10);
                            continue;
                        }

                        Thread.Sleep(10);
                        result = false;
                    }

                    return result;

                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return false;
                }
            }
        }

        public int[] GetValue(int channels)
        {
            lock (this)
            {
                try
                {
                    _resetEvent.Reset();
                    bool result = true;
                    _storage = new int[channels];
                    for (int i = 0; i < channels; i++)
                    {
                        string data = string.Format("$4{0}000", i + 1);
                        if (_serialComm.Write(data + GetCheckSum(data)))
                        {
                            if (_resetEvent.WaitOne(_coreConfig.ReponseTimeout) == false)
                            {
                                result = false;
                                break;
                            }
                        }
                    }

                    return result ? _storage : null;

                }
                catch (Exception e)
                {
                    _logger.Error(e);
                    return null;
                }
            }
        }

        private string GetCheckSum(string data)
        {
            var checksum = data.Aggregate(0, (p, v) => p ^ v);
            return Convert.ToString(checksum >> 4) + Convert.ToString(checksum & 0xF);
        }
    }
}
