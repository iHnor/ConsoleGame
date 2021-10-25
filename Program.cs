using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Tcp
{
    class newClient
    {
        public TcpClient client;
        public NetworkStream stream;
        public StreamReader reader;
        public StreamWriter writer;

        public newClient(TcpClient server)
        {
            this.client = server;
            this.stream = client.GetStream();
            this.reader = new StreamReader(stream);
            this.writer = new StreamWriter(stream);
        }

    }
    class Program
    {
        public static void Main(string[] args)
        {
            TcpListener server = null;
            List<newClient> clients = new List<newClient>();
            try
            {
                IPAddress localAddr = IPAddress.Parse(args[0]);
                Int32 port = Int32.Parse(args[1]);
                server = new TcpListener(localAddr, port);
                server.Start();
                String data = null;

                while (true)
                {
                    Console.Write("Waiting for a connection... ");

                    newClient tmp = new newClient(server.AcceptTcpClient());
                    clients.Add(tmp);
                    Console.WriteLine("Connected!");
                    System.Console.WriteLine(clients.Count);

                    data = null;
                    if (clients.Count == 2)
                    {
                        while (true)
                        {
                            foreach (var client in clients)
                            {
                                data = client.reader.ReadLine();
                                Console.WriteLine("Received: {0}", data);
                                string response = data.ToUpper();
                                client.writer.WriteLine(response);
                                client.writer.Flush();
                                Console.WriteLine("Sent: {0}", response);
                            }

                        }
                    }
                }
            }
            catch (SocketException e)
            {
                Console.WriteLine("SocketException: {0}", e);
            }
            finally
            {
                server.Stop();
            }

            Console.WriteLine("\nHit enter to continue...");
            Console.Read();
        }
    }
}
