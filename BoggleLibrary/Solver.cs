using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Collections;

namespace BoggleLibrary
{
    /// <summary>
    /// Boggle solver class.
    /// </summary>
    public class Solver
    {
        public delegate String WordAction(String s);

        /// <summary>
        /// Delegate to run on each word in dictionary. Default delegate replaces qu with q.
        /// </summary>
        public WordAction Processor { get; set; }

        /// <summary>
        /// Delegate to determine which words in dictionary will be accepted. Default is to accept words between 3 and 16 characters.
        /// </summary>
        public Predicate<String> Matcher { get; set; }

        /// <summary>
        /// Random number generator for board generation.
        /// </summary>
        private static Random random;

        /// <summary>
        /// Unique identifier for each time Solve() is run. Used to flag words found.
        /// </summary>
        private Int32 solveNumber;

        private TrieNode trie;

        private byte[,] board; // TODO consider making a jagged array?
        private bool[][] used; // Faster as a jagged array than multidimensional
        private static byte len;
        private static int found;
        private static int score;

        /// <summary>
        /// List of strings representing characters on each standard Boggle dice.
        /// </summary>
        private static string[] dice = new string[]
        {
            "aaeegn",
            "elrtty",
            "aoottw",
            "abbjoo",
            "ehrtvw",
            "cimotu",
            "distty",
            "eiosst",
            "delrvy",
            "achops",
            "himnqu",
            "eeinsu",
            "eeghnw",
            "affkps",
            "hlnnrz",
            "deilrx"
        };

        /// <summary>
        /// Boggle solver class. Usage: LoadDictionary(); Solve(StringToByte("abcdefghijklmnop")).
        /// </summary>
        public Solver()
        {
            this.solveNumber = Int32.MinValue;

            this.Processor = new WordAction(s => s.Replace("qu", "q"));
            this.Matcher = new Predicate<String>(s => s.Length >= 3 && s.Length <= 16);
        }

        /// <summary>
        /// Static constructor. Initialises random number generator.
        /// </summary>
        static Solver()
        {
            Solver.random = new Random();
        }

        #region Load Dictionary
        /// <summary>
        /// Load the default dictionary from the current working directory.
        /// </summary>
        public void LoadDictionary()
        {
            String dir = Directory.GetCurrentDirectory();
            String file = Properties.Settings.Default.DefaultDictionaryFile;
            String path = Path.Combine(dir, file);
            LoadDictionary(path);
        }

        /// <summary>
        /// Load the dictionary from the given file path. If a cached version exists, it will be loaded.
        /// </summary>
        /// <param name="dictionaryPath"></param>
        public void LoadDictionary(String dictionaryPath)
        {
            try
            {
                this.trie = loadFromCache(dictionaryPath);
            }
            catch (Exception)
            {
                try
                {
                    this.trie = loadFromFile(dictionaryPath);
                }
                catch (Exception x)
                {
                    throw new DictionaryNotLoadedException(x);
                }
            }
        }

        private TrieNode loadFromCache(String dictionaryPath)
        {
            throw new NotImplementedException(); // TODO implement pickling of trie object
        }

        private TrieNode loadFromFile(String path)
        {
            TrieNode node = new TrieNode(Byte.MaxValue, null, false);
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (!reader.EndOfStream)
                    {
                        String line = reader.ReadLine().Trim().ToLower();
                        line = Processor(line);
                        if (Matcher(line))
                            node.AddWord(line);
                    }
                }
            }
            return node;
        }
        #endregion

        #region Generate random boards
        /// <summary>
        /// Generate a 4x4 board made up of characters according to the Boggle dice probabilities.
        /// </summary>
        /// <returns>Byte array representing the board.</returns>
        public static byte[,] GetBoard()
        {
            Byte[,] b = new byte[4, 4];
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    b[i, j] = (byte)(dice[(4 * i) + j][Solver.random.Next(5)] - TrieNode.FirstChar);
            return b;
        }

        /// <summary>
        /// Generate a list of n boards made up of Boggle characters.
        /// </summary>
        /// <param name="count">Number of boards to generate.</param>
        /// <returns>Array of byte arrays representing the boards.</returns>
        public static byte[][,] GetBoards(int count)
        {
            byte[][,] rc = new byte[count][,];
            for (int i = 0; i < count; i++)
                rc[i] = GetBoard();
            return rc;
        }

        /// <summary>
        /// Generate a board made up of random a-z characters.
        /// </summary>
        /// <param name="side">Side length, e.g. 4 or 5.</param>
        /// <returns>Byte array representing the board.</returns>
        public static byte[,] GetRandomBoard(int side)
        {
            Byte[,] b = new byte[side, side];
            for (int i = 0; i < side; i++)
                for (int j = 0; j < side; j++)
                    b[i, j] = (byte)Solver.random.Next(26);
            return b;
        }

        /// <summary>
        /// Generate a list of n boards made up of random a-z characters.
        /// </summary>
        /// <param name="count">Number of boards to generate.</param>
        /// <param name="side">Side length, e.g. 4 or 5.</param>
        /// <returns>Array of byte arrays representing the boards.</returns>
        public static byte[][,] GetRandomBoards(int count, int side)
        {
            byte[][,] rc = new byte[count][,];
            for (int i = 0; i < count; i++)
                rc[i] = GetRandomBoard(side);
            return rc;
        }

        /// <summary>
        /// Generate a list of boards from a file.
        /// </summary>
        /// <param name="path">Path to board file.</param>
        /// <param name="boards">Maximum number of boards to load from file.</param>
        /// <returns>Array of byte arrays representing the boards.</returns>
        public static byte[][,] GetBoardsFromFile(string path, int boards)
        {
            byte[][,] rc = new byte[boards][,];

            int i = 0;
            using (FileStream stream = File.Open(path, FileMode.Open, FileAccess.Read))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    while (i < boards)
                    {
                        if (reader.EndOfStream)
                            reader.BaseStream.Seek(0, SeekOrigin.Begin);
                        String line = reader.ReadLine().Trim();
                        rc[i++] = Convert(line);
                    }
                }
            }
            return rc;
        }
        #endregion

        #region String and byte array conversion
        /// <summary>
        /// Convert a board stored as a byte array into a string.
        /// </summary>
        /// <param name="board"></param>
        /// <returns>String representing the board.</returns>
        public static String Convert(byte[,] board)
        {
            StringBuilder s = new StringBuilder();
            for (int i = 0; i < board.GetLength(0); i++)
                for (int j = 0; j < board.GetLength(1); j++)
                    s.Append((char)(board[i, j] + TrieNode.FirstChar));
            return s.ToString();
        }

        /// <summary>
        /// Convert a board stored as a string into a byte array.
        /// </summary>
        /// <param name="board"></param>
        /// <returns>Byte array representing the board.</returns>
        public static byte[,] Convert(String board)
        {
            if (String.IsNullOrEmpty(board))
                throw new ArgumentException("Argument 'board' was null or empty. Expected string with a square number of characters.");

            int side = (int)Math.Sqrt(board.Length);
            if (side * side != board.Length)
                throw new ArgumentException("Argument 'board' was " + board.Length + " characters long. Expected a square number.");

            board = board.ToLower();

            Byte[,] b = new byte[side, side];
            for (int i = 0; i < board.Length; i++)
            {
                if (!Char.IsLower(board[i]))
                    throw new ArgumentException("Unexpected character on board: " + board[i]);
                b[i / side, i % side] = (byte)(board[i] - TrieNode.FirstChar);
            }
            return b;
        }
        #endregion

        /// <summary>
        /// Solve a Boggle game. Use Solve(byte[] board) instead if possible.
        /// </summary>
        /// <param name="board">String representing the Boggle board. Will be converted to byte[,] using Convert(board).</param>
        /// <returns>Number of words found.</returns>
        public Solution Solve(String board)
        {
            return Solve(Convert(board));
        }

        /// <summary>
        /// Solve a Boggle game.
        /// </summary>
        /// <param name="board">Byte array representing the Boggle board.</param>
        /// <returns>Number of words found.</returns>
        public Solution Solve(byte[,] board)
        {
            if (board == null)
                throw new ArgumentNullException("board");

            len = (byte)board.GetLength(0);
            if (len != board.GetLength(1))
                throw new ArgumentException("Argument 'board' was not a square array.");

            if (this.trie == null)
                throw new InvalidOperationException("Called Solve method when dictionary not successfully loaded.");

            this.solveNumber++;
            this.board = board;

            this.used = new bool[len][];
            for (int i = 0; i < len; i++)
                this.used[i] = new bool[len];

            found = 0;
            score = 0;

            for (byte i = 0; i < len; i++)
                for (byte j = 0; j < len; j++)
                    find(trie[this.board[i, j]], i, j);

            return new Solution(found, score);
        }

        /// <summary>
        /// Get the full word list of the last solved board.
        /// </summary>
        /// <returns></returns>
        public Dictionary<String, int> GetLastWordList()
        {
            Dictionary<String, int> words = new Dictionary<string, int>();
            TrieNode.Iterate(this.trie, (n => n.IsWord && n.LastFound == this.solveNumber), (n => words.Add(n.GetWord(), 0)));
            return words;
        }

        private void find(TrieNode current, int x, int y)
        {
            if (current.IsWord && current.LastFound != this.solveNumber)
            {
                found++;
                current.LastFound = this.solveNumber;
            }

            if (!current.HasChildren)
                return;

            used[x][y] = true;

            byte imax = (byte)(x + 2 > len ? len : x + 2);
            byte jmax = (byte)(y + 2 > len ? len : y + 2);
            byte jmin = (byte)(y - 1 < 0 ? 0 : y - 1);

            for (byte i = (byte)(x - 1 < 0 ? 0 : x - 1); i < imax; i++)
                for (byte j = jmin; j < jmax; j++)
                    if ((current[board[i, j]]) != null && !used[i][j])
                        find(current[board[i, j]], i, j);

            used[x][y] = false;
        }
    }

    /// <summary>
    /// Exception thrown when the dictionary is not successfully loaded. Check InnerException for details.
    /// </summary>
    public class DictionaryNotLoadedException : Exception
    {
        public DictionaryNotLoadedException(Exception inner)
            : base("Failed to load dictionary file.", inner)
        {
        }
    }

    public struct Solution
    {
        public int Words;
        public int Score;
        public Solution(int words, int score)
        {
            Words = words;
            Score = score;
        }
    }
}
