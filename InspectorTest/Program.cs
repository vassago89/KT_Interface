using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace InspectorTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("-----Inspector-----\n\n");

            var listener = new TcpListener(IPAddress.Any, 4444);
            listener.Start();

            while (true)
            {
                var task = listener.AcceptTcpClientAsync();
                task.ConfigureAwait(false);
                task.Wait();
                
                var client = task.Result;
                Task.Factory.StartNew(AsyncTcpProcess, client).Wait();
            }
        }

        private async static void AsyncTcpProcess(object o)
        {
            var client = (TcpClient)o;

            NetworkStream stream = client.GetStream();

            var buff = new byte[1024];
            var nbytes = await stream.ReadAsync(buff, 0, buff.Length).ConfigureAwait(false);

            if (nbytes > 0)
            {
                string message = Encoding.ASCII.GetString(buff, 0, nbytes);
                try
                {
                    Console.WriteLine(message);
                }
                catch (Exception e)
                {

                }
                finally
                {
                    stream.Close();
                    client.Close();
                }
            }
        }
        
    }
}
