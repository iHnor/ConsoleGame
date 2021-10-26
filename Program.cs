using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Tcp
{
    class newGame {

        private Game StartGame = new Game();
        private PrintGameField PrintField = new PrintGameField();
        private int whosStep = 0;
        private  bool change;
        public newClient changePlayer(List<newClient> clients){
            if(change){
                change = !change;    
                return clients[0];
            }
            else{
                change = !change;    
                return clients[1];   
            } 
        }
        public void Start(int[] steps)
        {
            //int[,] steps = { { 0, 0 }, { 0, 1 }, { 1, 1 }, { 0, 2 }, { 2, 2 } };

            //int i = 0;    
            PrintField.ShowTheField(StartGame.field);
            //for (int i = 0; i < steps.GetUpperBound(0) + 2; i++)
            //{
                char testwin = StartGame.isWin();
                if (testwin == ' ')
                {
                    if (whosStep % 2 == 0)
                    {
                        StartGame.field = StartGame.DoStep(steps[0], steps[1], 'x');
                    }
                    else
                    {
                        StartGame.field = StartGame.DoStep(steps[0], steps[1], 'o');
                    }
                    PrintField.ShowTheField(StartGame.field);
                }
                else
                {
                    PrintField.PrintWiner(testwin);
                }
            //}
            whosStep++;
        }


    }

    class Game
    {
        public char[,] field = { { ' ', ' ', ' ' }, { ' ', ' ', ' ' }, { ' ', ' ', ' ' } };
        static private string isX = "xxx";
        static private string isO = "ooo";
        public char[,] DoStep(int row, int col, char XorO)
        {
            if (XorO == 'x' || XorO == 'o')
            {
                if (checkCell(row, col))
                {
                    field[row, col] = XorO;
                    return field;
                }
                else
                {
                    System.Console.WriteLine("Cell is not free");
                    return field;
                }
            }
            else
            {
                System.Console.WriteLine("Enter the right value");
                return field;
            }
        }

        private bool checkCell(int row, int col)
        {
            if (field[row, col] == ' ') return true;
            else return false;
        }
        public char isWin()
        {

            if (isDiagonal(isX)) return 'x';
            else if (isDiagonal(isO)) return 'o';
            else if (isHorisontal(isX)) return 'x';
            else if (isHorisontal(isO)) return 'o';
            else if (isVertical(isX)) return 'x';
            else if (isVertical(isO)) return 'o';
            else if (isСellsAreFree()) return ' ';
            else return 'e';
        }


        private bool isСellsAreFree()
        {
            for (int i = 0; i < 3; i++)
            {
                for (int j = 0; j < 3; j++)
                {
                    if (field[i, j] == ' ') return true;
                }
            }
            return false;
        }

        private bool isVertical(string XorO)
        {
            for (int i = 0; i < 3; i++)
            {
                string resultLine = null;
                for (int j = 0; j < 3; j++)
                {
                    resultLine = resultLine + field[j, i];
                    if (resultLine == XorO) return true;
                }
            }
            return false;
        }
        private bool isDiagonal(string XorO)
        {
            if ($"{field[0, 0]}{field[1, 1]}{field[2, 2]}" == XorO) return true;
            else if ($"{field[0, 2]}{field[1, 1]}{field[2, 0]}" == XorO) return true;
            else return false;
        }
        private bool isHorisontal(string XorO)
        {
            for (int i = 0; i < 3; i++)
            {
                string resultLine = null;
                for (int j = 0; j < 3; j++)
                {
                    resultLine = resultLine + field[i, j];
                    if (resultLine == XorO) return true;
                }
            }
            return false;
        }
    }

    class PrintGameField
    {
        public void ShowTheField(char[,] field)
        {
            string show = transformField(field);
            Console.WriteLine(show);
        }

        private string transformField(char[,] field)
        {
            string transform = null;

            for (int i = 0; i < field.GetUpperBound(0) + 1; i++)
                transform += $"{field[i, 0]}|{field[i, 1]}|{field[i, 2]} \n";
            return transform;
        }
        public void PrintWiner(char testwin)
        {
            if (testwin == 'e')
            {
                Console.WriteLine("No one win!");
            }
            else
            {
                Console.WriteLine($"Win: {testwin.ToString().ToUpper()}");

            }
        }
    }


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
                   // System.Console.WriteLine(clients.Count);

                    data = null;
                    if (clients.Count == 2)
                    {
                        newGame game = new newGame();
                        while (true)
                        {
                            //foreach (var client in clients)
                            //{
                                newClient client = game.changePlayer(clients); 
                                data = client.reader.ReadLine();
                                Console.WriteLine("Received: {0}", data);
                                System.Console.WriteLine(Int32.Parse(data[0].ToString()));
                                System.Console.WriteLine(Int32.Parse(data[2].ToString()));

                                int[] response = Array.ConvertAll(data.Split(' '), Convert.ToInt32);

                                game.Start(response);
                                //if ()
                                //string response = data.ToUpper();
                                //client.writer.WriteLine(response);
                                //client.writer.Flush();
                                //Console.WriteLine("Sent: {0}", response);
                            //}

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
