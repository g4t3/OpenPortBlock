using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

namespace OpenPortBlock
{
    class BlockPort
    {
        public bool Activated = false;
        public bool GeneralActivation = true;
        private int Port { get; set; }
        private IPAddress Address { get; set; }

        public BlockPort(int port)
        {
            Port = port;

            Thread BackgroundThread = new Thread(Block);
            BackgroundThread.Start(new IPEndPoint(IPAddress.Any, port));
        }

        public BlockPort(IPAddress address, int port)
        {
            Port = port;
            Address = address;
            
            Thread BackgroundThread = new Thread(Block);
            BackgroundThread.Start(new IPEndPoint(address, port));
        }

        private void Block(object obj)
        {
            IPEndPoint endPoint = (IPEndPoint)obj;
            var date = DateTime.Now.ToString("yy-MM-dd-mm-ss-fff");
            var writer = new StreamWriter($"portlog/{endPoint.Port}.log");
            writer.WriteLine($"{date}: *Activated={Activated}*");
            writer.Close();


            Thread.Sleep(200);

            if (Activated)
            {
                Socket listener = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                listener.Bind(endPoint);
                listener.Listen(0);
                Console.ForegroundColor = ConsoleColor.Green;
                Console.WriteLine($"Port {endPoint.Port} is active.");
                Console.ForegroundColor = ConsoleColor.Gray;
                
                while (Activated)
                {
                    Socket handler = listener.Accept();
                    string data = null;
                    byte[] bytes;
                    while (handler.Connected && Activated && GeneralActivation)
                    {
                        bytes = new byte[1024];
                        int bytesRec;
                        try { bytesRec = handler.Receive(bytes); }
                        catch { break; }
                        data += Encoding.ASCII.GetString(bytes, 0, bytesRec);
                        if (data.IndexOf("<EOF>") > -1)
                        {
                            break;
                        }
                        date = DateTime.Now.ToString("yy-MM-dd-mm-ss-fff");
                        File.AppendAllText($"portlog/{endPoint.Port}.log", $"{date}: {data}\n");
                    }
                }
            }
        }
    }
}
