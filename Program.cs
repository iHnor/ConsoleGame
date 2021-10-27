using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Collections.Generic;

namespace Tcp
{
    class newGame
    {
        List<newClient> clients = new List<newClient>();
        public newGame(List<newClient> clients)
        {
            this.clients = clients;
        }
        private Game StartGame = new Game();
        private PrintGameField PrintField = new PrintGameField();
        private int whosStep = 0;
        private bool isFirstPlayer;
        private bool isPlaying = true;
        public bool gatIsPlaying() { return isPlaying; }
        public newClient changePlayer(List<newClient> clients)
        {
            isFirstPlayer = !isFirstPlayer;
            return clients[isFirstPlayer ? 0 : 1];
        }
        public bool Start(int[] steps, newClient client)
        {
            bool resultOfStep;

            if (whosStep % 2 == 0)
            {
                resultOfStep = StartGame.DoStep(steps[0], steps[1], 'x', client);
                if (resultOfStep) whosStep++;
            }
            else
            {
                resultOfStep = StartGame.DoStep(steps[0], steps[1], 'o', client);
                if (resultOfStep) whosStep++;
            }

            char testwin = StartGame.isWin();
            if (testwin == ' ')
            {
                PrintField.ShowTheField(StartGame.field, clients);
                return resultOfStep;
            }
            else
            {
                PrintField.ShowTheField(StartGame.field, clients);
                PrintField.PrintWiner(testwin);
                isPlaying = false;
                client.setIsWinValue();
                return false;
            }

        }


    }

    class Game
    {
        public char[,] field = { { ' ', ' ', ' ' }, { ' ', ' ', ' ' }, { ' ', ' ', ' ' } };
        static private string isX = "xxx";
        static private string isO = "ooo";
        public bool DoStep(int row, int col, char XorO, newClient client)
        {
            if (XorO == 'x' || XorO == 'o')
            {
                if (checkCell(row, col))
                {
                    field[row, col] = XorO;
                    return true;
                }
                else
                {
                    client.writer.WriteLine("Cell is not free");
                    client.writer.Flush();
                    return false;
                }
            }
            else
            {
                client.writer.WriteLine("Enter the right value");
                client.writer.Flush();
                return false;
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
        public void ShowTheField(char[,] field, List<newClient> clients)
        {
            string show = transformField(field);
            foreach (var client in clients)
            {
                client.writer.WriteLine(show);
                client.writer.Flush();
            }
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
        private bool isWin;
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
        public void setIsWinValue()
        {
            isWin = !isWin;
        }
        public bool getIsWin()
        {
            return isWin;
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

                    newClient inputClient = new newClient(server.AcceptTcpClient());
                    clients.Add(inputClient);
                    Console.WriteLine("Connected!");
                
                    data = null;
                    if (clients.Count == 2)
                    {
                        newGame game = new newGame(clients);
                        newClient client = game.changePlayer(clients);
                        clients[0].writer.WriteLine("You is X");
                        clients[0].writer.Flush();
                        clients[1].writer.WriteLine("You is Y");
                        clients[1].writer.Flush();

                        while (true)
                        {
                            data = client.reader.ReadLine();
                            int[] response = Array.ConvertAll(data.Split(' '), Convert.ToInt32);
                            bool resultStep = game.Start(response, client);

                            if (game.gatIsPlaying())
                            {
                                if (resultStep)
                                    client = game.changePlayer(clients);
                            }
                            else
                                foreach (var c in clients)
                                {
                                    if (c.getIsWin())
                                        c.writer.WriteLine("You Win!");
                                    else
                                        c.writer.WriteLine("You Lose");
                                    c.writer.WriteLine("Game over");
                                    c.writer.Flush();
                                    c.client.Close();
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
