﻿using System;
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

            Console.WriteLine("-----Host-----\n\n");

            while (true)
            {
                TcpClient client = new TcpClient();

                client.Connect("localhost", 5000);

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
                else if (key.Key == ConsoleKey.D3)
                {
                    message = "Get_stat\r";
                }

                byte[] buff = Encoding.ASCII.GetBytes(message);

                var stream = client.GetStream();
                stream.Write(buff, 0, buff.Length);

                byte[] outbuf = new byte[1024];
                int nbytes = stream.Read(outbuf, 0, outbuf.Length);
                string output = Encoding.ASCII.GetString(outbuf, 0, nbytes);
                Console.WriteLine(output);

                if (output == "Ack")
                {
                    
                }

                stream.Close();
                client.Close();
            }
        }
    }
}
