using System;
using System.Collections.Generic;
using System.Text;

namespace BoggleLibrary
{
    /// <summary>
    /// Implementation of a simple prefixing Trie.
    /// </summary>
    internal class TrieNode
    {
        public TrieNode Parent { get; private set; }
        public Boolean IsWord { get; private set; }
        public readonly byte Value;
        public Boolean HasChildren;

        public Int32 LastFound;

        private TrieNode[] children;
        internal const byte FirstChar = (byte)'a';
        internal const byte NumLetters = 'z' - 'a' + 1;

        public TrieNode(byte value, TrieNode parent, Boolean isWord)
        {
            this.children = new TrieNode[NumLetters];

            this.Value = value;
            this.Parent = parent;
            this.IsWord = isWord;
            this.HasChildren = false;
        }

        /// <summary>
        /// Access this TrieNode's children by the byte offset from FirstChar. Null if TrieNode does not contain a child node for that byte offset.
        /// </summary>
        /// <param name="i"></param>
        /// <returns></returns>
        public TrieNode this[byte i]
        {
            get { return children[i]; }
            private set { children[i] = value; }
        }

        /// <summary>
        /// Access this TrieNode's children by character. Null if TrieNode does not contain a child node for that character.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        public TrieNode this[Char c]
        {
            get { return children[c - FirstChar]; }
            private set { children[c - FirstChar] = value; }
        }

        /// <summary>
        /// Add a new child node to this TrieNode.
        /// </summary>
        /// <param name="c">Char identifier for new child.</param>
        /// <param name="isWord">Is the child node a word terminator?</param>
        /// <returns>Returns the child TrieNode added.</returns>
        public TrieNode Put(Char c, Boolean isWord)
        {
            this.HasChildren = true;
            if (this[c] == null)
                this[c] = new TrieNode((byte)(c - FirstChar), this, isWord);
            if (isWord)
                this[c].IsWord = true;
            return this[c];
        }

        /// <summary>
        /// Adds successive child TrieNodes for each character of a given string.
        /// </summary>
        /// <param name="word"></param>
        public void AddWord(String word)
        {
            TrieNode node = this;
            for (int i = 0; i < word.Length; i++)
                node = node.Put(word[i], (i == word.Length - 1));
        }

        /// <summary>
        /// Recurses over the trie and applies an Action to TrieNodes which match a Predicate.
        /// </summary>
        /// <param name="node">TrieNode to perform Action on.</param>
        /// <param name="predicate">Predicate to match desired TrieNodes.</param>
        /// <param name="action">Action to perform on TrieNode.</param>
        public static void Iterate(TrieNode node, Predicate<TrieNode> predicate, Action<TrieNode> action)
        {
            if (node == null)
                throw new ArgumentNullException("node");
            if (predicate(node))
                action(node);
            foreach (TrieNode n in node.children)
                if (n != null)
                    Iterate(n, predicate, action);
        }

        /// <summary>
        /// Get the full word representing the node's position in the Trie.
        /// </summary>
        /// <returns></returns>
        public String GetWord()
        {
            StringBuilder s = new StringBuilder();
            TrieNode n = this;
            while (n.Parent != null)
            {
                s.Insert(0, (char)(FirstChar + n.Value));
                n = n.Parent;
            }
            return s.ToString();
        }

        /// <summary>
        /// Returns the char represented by the TrieNode.
        /// </summary>
        /// <returns></returns>
        public override String ToString()
        {
            return ((char)(FirstChar + Value)).ToString();
        }
    }
}
