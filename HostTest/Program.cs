using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace HostTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Thread.Sleep(2000);

            while (true)
            {
                TcpClient client = new TcpClient();

                client.Connect("localhost", 5555);

                var key = Console.ReadKey();
                
                string message = "";
                
                if (key.Key == ConsoleKey.D1)
                {
                    message = "Start1,2345\n";
                }
                else if (key.Key == ConsoleKey.D2)
                {
                    message = "Start2,1234\n";
                }

                byte[] buff = Encoding.ASCII.GetBytes(message);

                var stream = client.GetStream();
                stream.Write(buff, 0, buff.Length);

                byte[] outbuf = new byte[1024];
                int nbytes = stream.Read(outbuf, 0, outbuf.Length);
                string output = Encoding.ASCII.GetString(outbuf, 0, nbytes);
                Console.WriteLine(output);

                stream.Close();
                client.Close();
            }
        }
    }
}
