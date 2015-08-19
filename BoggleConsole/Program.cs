using System;
using System.Collections.Generic;
using System.Text;
using BoggleLibrary;
using System.Diagnostics;
using System.IO;

namespace Harness
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Boggle solver");
            Stopwatch s = new Stopwatch();

            // Initialise solver and load dictionary
            Solver solver = new Solver();
            Console.Write("Loading dictionary... ");
            solver.LoadDictionary();
            Console.WriteLine("loaded.");

            while (true)
            {
                try
                {
                    // Choose mode
                    Console.Write(Environment.NewLine + "Solve a single board (s) or multiple boards (m): ");
                    if (Console.ReadLine().ToLower() == "m")
                    {
                        Console.Write("Source (b: boggle dice; r: random; f: file): ");
                        String boardSource = Console.ReadLine();

                        Console.Write("Number of boards to solve: ");
                        Int32 boards = Int32.Parse(Console.ReadLine());

                        byte[][,] b;
                        String modeText;
                        switch (boardSource)
                        {
                            case "r":
                                {
                                    b = Solver.GetRandomBoards(boards, 4);
                                    modeText = "from random letters";
                                    break;
                                }
                            case "f":
                                {
                                    b = Solver.GetBoardsFromFile(BoggleLibrary.Properties.Settings.Default.DefaultBoardsFile, boards);
                                    modeText = "from file";
                                    break;
                                }
                            default:
                                {
                                    b = Solver.GetBoards(boards);
                                    modeText = "from boggle dice";
                                    break;
                                }
                        }

                        Console.WriteLine(boards + " boards generated " + modeText);

                        // Solve boards
                        int words = 0, score = 0;
                        s.Start();
                        for (int i = 0; i < boards; i++)
                        {
                            Solution solution = solver.Solve(b[i]);
                            words += solution.Words;
                            score += solution.Score;
                        }
                        s.Stop();
                        double time = ((double)s.ElapsedTicks / Stopwatch.Frequency) * 1000;

                        // Report back
                        Console.WriteLine("Solved at " + (int)(boards / (time / 1000.0)) + " boards/s (total: " + time.ToString("0.00") + "ms, average: "
                            + (time / (double)boards).ToString("0.000") + "ms, " + (words / (double)boards).ToString("0") + " words, " + (score / (double)boards).ToString("0") + " score)");
                        b = null;
                    }
                    else
                    {
                        // Input board, or use default
                        Console.Write("Enter single board: ");
                        String b = Console.ReadLine();
                        if (String.IsNullOrEmpty(b))
                            b = "catdlinemaropets";
                        byte[,] board = Solver.Convert(b);

                        // Solve single board
                        s.Start();
                        int count = solver.Solve(board).Words;
                        s.Stop();
                        double time = ((double)s.ElapsedTicks / Stopwatch.Frequency) * 1000;

                        // Report back
                        Console.WriteLine("Solved \"" + b + "\" in " + time.ToString("0.00") + "ms, found " + (count) + " words: ");
                        foreach (var pair in solver.GetLastWordList())
                            Console.Write(pair.Key + " ");
                    }

                    s.Reset();
                }
                catch (ArgumentException x)
                {
                    Console.WriteLine("Exception: " + x.Message);
                }
            }
        }
    }
}
