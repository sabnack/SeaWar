using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;

namespace SeaWarNew
{
    struct Point
    {
        public int X;
        public int Y;

        public Point(int x, int y)
        {
            X = x;
            Y = y;
        }
    }

    struct BlownDeckCount
    {
        public int CompCount;
        public int UserCount;
        
        public bool TestWin()
        {
            if (CompCount == 20)
            {
                Print("Computer Win");
                return true;
            }

            if (UserCount == 20)
            {
                Print("User Win");
                return true;
            }
            return false;
        }

        private void Print(string str)
        {
            Console.SetCursorPosition(0, 30);
            Console.ForegroundColor = ConsoleColor.Red;
            Console.WriteLine(str);
            Console.WriteLine("Game Over");
        }
    }

    struct Hit
    {
        private readonly int X;
        private readonly int Y;
        public bool Killed;
        public bool LastHit;
        private readonly int DeckNum;
        private int DeckCount;
        private List<Point> PossibleLoc;

        public Hit(int x, int y, int deckNum)
        {
            X = x;
            Y = y;
            Killed = false;
            LastHit = true;
            DeckNum = deckNum;
            DeckCount = 1;
            PossibleLoc = new List<Point>();
            if (x - 1 > 0) PossibleLoc.Add(new Point(x - 1, y));
            if (x + 1 < 10) PossibleLoc.Add(new Point(x + 1, y));
            if (y - 1 > 0) PossibleLoc.Add(new Point(x, y - 1));
            if (y + 1 < 10) PossibleLoc.Add(new Point(x, y + 1));
        }

        private int RemoveMissFromList(int[,] radarComp)
        {
            int tmpX;
            int tmpY;
            var xy = new Random().Next(PossibleLoc.Count);

            do
            {
                tmpX = PossibleLoc[xy].X;
                tmpY = PossibleLoc[xy].Y;

                if (radarComp[tmpX, tmpY] == -1)
                {
                    PossibleLoc.RemoveAt(xy);
                    xy = new Random().Next(PossibleLoc.Count);
                }
                else
                {
                    return xy;
                }
            } while (radarComp[tmpX, tmpY] != 0);
            return xy;
        }

        public void DoMove(int[,] fieldUser, int[,] radarComp, ref BlownDeckCount bdc)
        {
            var xy = RemoveMissFromList(radarComp);
            var tmpX = PossibleLoc[xy].X;
            var tmpY = PossibleLoc[xy].Y;

            if (fieldUser[tmpX, tmpY] > 0 && fieldUser[tmpX, tmpY] < 5)
            {
                fieldUser[tmpX, tmpY] += 5;
                radarComp[tmpX, tmpY] = fieldUser[tmpX, tmpY];
                bdc.CompCount++;
                DeckCount++;
                if (DeckCount == DeckNum)
                {
                    Killed = true;
                    return;
                }
                PossibleLoc = new List<Point>();
                LastHit = true;

                if (tmpY == Y)
                {
                    if ((X < tmpX ? X - 1 : X + 1) > -1 && (X < tmpX ? X - 1 : X + 1) < 10)
                    {
                        PossibleLoc.Add(new Point((X < tmpX ? X - 1 : X + 1), Y));
                    }
                    if ((X < tmpX ? tmpX + 1 : tmpX - 1) > -1 && (X < tmpX ? tmpX + 1 : tmpX - 1) < 10)
                    {
                        PossibleLoc.Add(new Point((X < tmpX ? tmpX + 1 : tmpX - 1), Y));
                    }
                }
                else
                {
                    if ((Y < tmpY ? Y - 1 : Y + 1) > -1 && (Y < tmpY ? Y - 1 : Y + 1) < 10)
                    {
                        PossibleLoc.Add(new Point(X, (Y < tmpY ? Y - 1 : Y + 1)));
                    }
                    if ((Y < tmpY ? tmpY + 1 : tmpY - 1) > -1 && (Y < tmpY ? tmpY + 1 : tmpY - 1) < 10)
                    {
                        PossibleLoc.Add(new Point(X, (Y < tmpY ? tmpY + 1 : tmpY - 1)));
                    }
                }
            }

            if (fieldUser[tmpX, tmpY] == 0)
            {
                fieldUser[tmpX, tmpY] = -1;
                radarComp[tmpX, tmpY] = -1;
                PossibleLoc.RemoveAt(xy);
                LastHit = false;
            }
        }
    }

    class Program
    {
        const int Z = 10;
        static void Main(string[] args)
        {
            Start();
        }

        static void Start()
        {
            var bdc = new BlownDeckCount();
            var fieldUser = ReadArrayFromFile();
            var radarUser = new int[Z, Z];

            var fieldComp = CreateCompArray();
            var radarComp = new int[Z, Z];

            int x = 1;
            int y = 1;

            Hit hitXY = new Hit(0, 0, 0)
            {
                Killed = true
            };

            bool endUserMove = false;

            Console.SetWindowSize(50, 40);
            ConsoleKeyInfo info;
            Console.WriteLine("Press any key to start");
            info = Console.ReadKey(true);

            do
            {
                PrintField(radarUser, fieldUser);
                PrintMove("User Move    ", ConsoleColor.Green, 1);
                PrintMove("                     ", ConsoleColor.Red, 2);
                if (bdc.TestWin())
                {
                    break;
                }
                Console.SetCursorPosition(y, x);

                if (!endUserMove)
                {
                    info = Console.ReadKey(true);
                    switch (info.Key)
                    {
                        case ConsoleKey.DownArrow:
                            {

                                if (TestXY(x + 1, y)) { continue; }
                                x++;
                                break;
                            }

                        case ConsoleKey.UpArrow:
                            {
                                if (TestXY(x - 1, y)) { continue; }
                                x--;
                                break;
                            }

                        case ConsoleKey.RightArrow:
                            {
                                if (TestXY(x, y + 1)) { continue; }
                                y += 2;
                                break;
                            }

                        case ConsoleKey.LeftArrow:
                            {
                                if (TestXY(x, y - 1)) { continue; }
                                y -= 2;
                                break;
                            }

                        case ConsoleKey.Enter:
                            {
                                if (TestUserMove(radarUser, x, y)) { continue; }
                                endUserMove = UserMove(fieldComp, radarUser, x, y, ref bdc);
                                TestFlooding(radarUser);
                                break;
                            }
                    }
                }
                else
                {
                    if (!hitXY.Killed)
                    {
                        if (hitXY.LastHit)
                        {
                            FinishOffShip(fieldUser, radarComp, ref hitXY, ref bdc);
                            TestFlooding(radarComp);
                            TestFlooding(fieldUser);
                            Thread.Sleep(1000);
                        }
                        else
                        {
                            endUserMove = false;
                            hitXY.LastHit = true;
                        }
                    }
                    else
                    {
                        PrintMove("Computer Move", ConsoleColor.Red, 1);
                        endUserMove = CompMove(fieldUser, radarComp, ref hitXY, ref bdc) ? false : true;
                        Thread.Sleep(1000);
                    }
                }
            } while (info.Key != ConsoleKey.Escape);

        }

        static void FinishOffShip(int[,] fieldUser, int[,] radarComp, ref Hit hitXY, ref BlownDeckCount bdc)
        {
            hitXY.DoMove(fieldUser, radarComp, ref bdc);
        }

        static bool CompMove(int[,] fieldUser, int[,] radarComp, ref Hit hitXY, ref BlownDeckCount bdc)
        {
            int x = 2;
            int y = 7;

            //do
            while (radarComp[x, y] > 4 || radarComp[x, y] == -1)
            {
                x = new Random().Next(Z);
                y = new Random().Next(Z);
            } //while (radarComp[x, y] > 4 || radarComp[x, y] == -1);

            if (fieldUser[x, y] > 0 && fieldUser[x, y] < 5)
            {
                fieldUser[x, y] += 5;
                radarComp[x, y] = fieldUser[x, y];
                bdc.CompCount++;

                if (fieldUser[x, y] > 6)
                {
                    hitXY = new Hit(x, y, fieldUser[x, y] - 5);
                }

                PrintMove("Hit                 ", ConsoleColor.Red, 2);
                TestFlooding(radarComp);
                TestFlooding(fieldUser);

                for (var i = x - 1; i < x + 2; i += 2)
                {
                    for (var j = y - 1; j < y + 2; j += 2)
                    {
                        if (i > radarComp.GetLength(0) - 1 || j > radarComp.GetLength(1) - 1 || i < 0 || j < 0)
                        {
                            continue;
                        }
                        radarComp[i, j] = -1;
                    }
                }
                return false;
            }
            else
            {
                radarComp[x, y] = -1;
                fieldUser[x, y] = -1;
                return true;
            }
        }

        static void TestFlooding(int[,] radar)
        {
            for (var x = 0; x < radar.GetLength(0); x++)
            {
                for (var y = 0; y < radar.GetLength(1); y++)
                {
                    if (radar[x, y] > 4)
                    {
                        int deckNum = radar[x, y] - 5;

                        if (!TestKilldeShip(radar, x, y, deckNum, true))
                        {
                            TestKilldeShip(radar, x, y, deckNum, false);
                        }
                    }
                }
            }
        }

        static bool TestKilldeShip(int[,] radar, int x, int y, int deckNum, bool direction)
        {
            int deckCount = 0;
            if ((direction ? x : y) + radar[x, y] - 6 < radar.GetLength(direction ? 0 : 1))
            {
                for (var i = direction ? x : y; i < (direction ? x : y) + deckNum; i++)
                {
                    if (radar[direction ? i : x, direction ? y : i] == radar[x, y])
                    {
                        deckCount++;
                    }
                }

                if (deckCount == deckNum)
                {
                    for (var i = direction ? x : y; i < (direction ? x : y) + deckNum; i++)
                    {
                        SetMiss(radar, direction ? i : x, direction ? y : i);
                    }
                    return true;
                }
            }
            return false;
        }

        static void SetMiss(int[,] radar, int x, int y)
        {
            for (var i = x - 1; i < x + 2; i++)
            {
                if (i < 0 || i > radar.GetLength(0) - 1)
                {
                    continue;
                }

                for (var j = y - 1; j < y + 2; j++)
                {
                    if (j < 0 || j > radar.GetLength(1) - 1)
                    {
                        continue;
                    }

                    if (radar[i, j] == 0)
                    {
                        radar[i, j] = -1;
                    }
                }
            }
        }

        static bool TestUserMove(int[,] radarUser, int x, int y)
        {
            int X = x - 1;
            int Y = y / 2;

            return radarUser[X, Y] == -1 || radarUser[X, Y] > 4;
        }

        static bool UserMove(int[,] fieldComp, int[,] radarUser, int x, int y, ref BlownDeckCount bdc)
        {
            int X = x - 1;
            int Y = y / 2;

            if (fieldComp[X, Y] > 0)
            {
                radarUser[X, Y] = fieldComp[X, Y] + 5;
                bdc.UserCount++;
                PrintMove("Hit                     ", ConsoleColor.Red, 2);
                Thread.Sleep(500);
                return false;
            }
            else
            {
                radarUser[X, Y] = -1;
                return true;
            }
        }

        static bool TestXY(int x, int y)
        {
            return (x == Z + 1 || y == Z * 2 || x < 1 || y < 1);
        }

        static int[,] CreateCompArray()
        {
            var fieldComp = new int[Z, Z];
            int shipCount = 0;
            int shipLength = 4;

            while (shipCount != 10)
            {
                for (var i = 0; i < 5 - shipLength; i++)
                {
                    CreateShip(fieldComp, shipLength);
                }
                shipLength--;
                shipCount++;
            }
            return fieldComp;
        }


        static void CreateShip(int[,] fieldComp, int shipLength)
        {
            while (true)
            {
                Random rand = new Random();
                var x = rand.Next(Z);
                var y = rand.Next(Z);
                var direction = rand.Next(2) == 1;
                var coordinate = direction ? x : y;

                if (fieldComp[x, y] != 0) continue;

                if (coordinate <= fieldComp.GetLength(direction ? 0 : 1) - shipLength && TestCoordinates(fieldComp, x, y, shipLength, direction))
                {
                    for (var i = 0; i < shipLength; i++)
                    {
                        fieldComp[direction ? x++ : x, direction ? y : y++] = shipLength;
                    }
                    break;
                }
            }
        }

        static bool TestCoordinates(int[,] field, int x, int y, int shipLength, bool direction)
        {
            for (var deckCount = 0; deckCount < shipLength; deckCount++)
            {
                for (var i = x - 1; i < x + 2; i++)
                {
                    if (i < 0 || i > field.GetLength(0) - 1)
                    {
                        continue;
                    }

                    for (var j = y - 1; j < y + 2; j++)
                    {
                        if (j < 0 || j > field.GetLength(1) - 1)
                        {
                            continue;
                        }

                        if (field[i, j] != 0)
                        {
                            return false;
                        }
                    }
                }

                if (direction)
                {
                    x++;
                }
                else
                {
                    y++;
                }
            }
            return true;
        }

        static void PrintField(int[,] radarUser, int[,] fieldUser)
        {
            Console.SetCursorPosition(0, 0);
            PrintField(radarUser);
            Console.WriteLine();
            PrintField(fieldUser);
        }

        static void PrintMove(string print, ConsoleColor color, int y)
        {
            Console.SetCursorPosition(30, y);
            Console.ForegroundColor = color;
            Console.Write(print);
            Console.ResetColor();
        }

        static void PrintField(int[,] field)
        {
            Console.WriteLine("┌────────────────────┐");
            for (var i = 0; i < Z; i++)
            {
                Console.Write("│");
                for (int j = 0; j < Z; j++)
                {
                    switch (field[i, j])
                    {
                        case -1:
                            {
                                Console.Write("* ");
                                break;
                            }
                        case 0:
                            {
                                Console.Write("  ");
                                break;
                            }
                        default:
                            if (field[i, j] > 4)
                            {
                                Console.ForegroundColor = ConsoleColor.Red;
                                Console.Write("■ ");
                                Console.ResetColor();
                            }
                            else
                            {
                                Console.Write("■ ");
                            }
                            break;
                    }
                }
                Console.Write("│");
                Console.WriteLine();

            }
            Console.WriteLine("└────────────────────┘");
        }

        static int[,] ReadArrayFromFile()
        {
            var startDir = Directory.GetParent(Directory.GetCurrentDirectory()).Parent.Parent.FullName;
            var fileName = startDir + "\\field.txt";

            if (!File.Exists(fileName))
            {
                Console.WriteLine("Source file does not exist");
                Environment.Exit(0);
            }
            var fieldUser = new int[Z, Z];
            StreamReader sr = File.OpenText(fileName);
            string read = null;
            string[] split;
            int k = 0;
            while ((read = sr.ReadLine()) != null)
            {
                int j = 0;
                split = read.Split(' ');
                foreach (string s in split)
                {
                    if (s != "")
                    {
                        int.TryParse(s, out int a);
                        fieldUser[k, j] = a;
                    }
                    j++;
                }
                k++;
            }
            sr.Close();
            return fieldUser;
        }
    }
}
