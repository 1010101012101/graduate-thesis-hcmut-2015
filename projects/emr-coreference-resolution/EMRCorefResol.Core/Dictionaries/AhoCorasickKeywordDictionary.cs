﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace HCMUT.EMRCorefResol.Utilities
{
    public class AhoCorasickKeywordDictionary : IKeywordDictionary
    {
        private readonly TrieNode _root = new TrieNode();
        private readonly List<string> _kwList;

        public int Count
        {
            get { return _kwList.Count; }
        }

        public string this[int index]
        {
            get { return _kwList[index]; }
        }

        public AhoCorasickKeywordDictionary(params string[] keywords)
            : this(keywords.AsEnumerable())
        { }

        public AhoCorasickKeywordDictionary(IEnumerable<string> keywords)
        {
            _kwList = new List<string>(keywords);
            BuildTree();
            BuildACAutomaton();
        }

        public AhoCorasickKeywordDictionary(string fileName)
        {
            //var keywordRoot = @"..\..\..\EMRCorefResol.English\Keywords\";
            var keywords = File.ReadAllLines(fileName).AsEnumerable();
            _kwList = new List<string>(keywords);
            BuildTree();
            BuildACAutomaton();
        }

        private void BuildTree()
        {
            for (int i = 0; i < _kwList.Count; i++)
            {
                var kw = _kwList[i].ToLower();
                var curNode = _root;
                var sb = new StringBuilder();

                for (int j = 0; j < kw.Length; j++)
                {
                    var c = kw[j];
                    sb.Append(c);

                    if (curNode.NextNodes.ContainsKey(c))
                    {
                        curNode = curNode.NextNodes[c];
                    }
                    else
                    {
                        var node = new TrieNode();
                        node.ExactKWIndex = -1;
                        curNode.NextNodes.Add(c, node);
                        curNode = node;
                    }
                }

                curNode.ExactKWIndex = i;
                curNode.KWIndices.Add(i);
            }
        }

        private void BuildACAutomaton()
        {
            var q = new Queue<TrieNode>();
            foreach (var n in _root.NextNodes.Values)
            {
                q.Enqueue(n);
                n.FailNode = _root;
            }

            while (q.Count > 0)
            {
                var r = q.Dequeue();
                foreach (var t in r.NextNodes)
                {
                    var a = t.Key;
                    var u = t.Value;

                    q.Enqueue(u);
                    var v = r.FailNode;
                    while (v == _root && !v.NextNodes.ContainsKey(a))
                    {
                        v = v.FailNode;
                    }
                    u.FailNode = (v != null && v.NextNodes.ContainsKey(a)) ? v.NextNodes[a] : _root;
                    u.KWIndices.UnionWith(u.FailNode.KWIndices);
                }
            }
        }

        public IEnumerator<string> GetEnumerator()
        {
            return _kwList.AsEnumerable().GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public bool Match(string s, KWSearchOptions options)
        {
            var found = false;

            SearchWithAction(s, options, (i, node) =>
            {
                if (options.HasFlag(KWSearchOptions.WholeWord))
                {
                    if (i == s.Length - 1 || char.IsWhiteSpace(s, i + 1))
                    {
                        foreach (var kwi in node.KWIndices)
                        {
                            var kw = _kwList[kwi];
                            var bIndex = i - kw.Length;
                            if (bIndex < 0 || char.IsWhiteSpace(s, bIndex))
                            {
                                found = true;
                                break;
                            }
                        }
                    }
                }
                else
                {
                    found = true;
                }
                return found;
            });

            return found;
        }

        public string[] SearchKeywords(string s, KWSearchOptions options)
        {
            var indices = SearchDictionaryIndices(s, options);
            return indices.Select(i => _kwList[i]).ToArray();
        }

        public int[] SearchStringIndices(string s, KWSearchOptions options)
        {
            var outIndices = new HashSet<int>();

            SearchWithAction(s, options, (i, node) =>
            {
                if (options.HasFlag(KWSearchOptions.WholeWord))
                {
                    if (i == s.Length - 1 || char.IsWhiteSpace(s, i + 1))
                    {
                        foreach (var kwi in node.KWIndices)
                        {
                            var kw = _kwList[kwi];
                            var bIndex = i - kw.Length;
                            if (bIndex < 0 || char.IsWhiteSpace(s, bIndex))
                                outIndices.Add(bIndex + 1);
                        }
                    }
                }
                else
                {
                    outIndices.UnionWith(node.KWIndices.Select(kwi => i - _kwList[kwi].Length + 1));
                }
                return false;
            });

            return outIndices.ToArray();
        }

        public int[] SearchDictionaryIndices(string s, KWSearchOptions options)
        {
            var outIndices = new HashSet<int>();

            SearchWithAction(s, options, (i, node) =>
            {
                if (options.HasFlag(KWSearchOptions.WholeWord))
                {
                    if (i == s.Length - 1 || char.IsWhiteSpace(s, i + 1))
                    {
                        foreach (var kwi in node.KWIndices)
                        {
                            var kw = _kwList[kwi];
                            var bIndex = i - kw.Length;
                            if (bIndex < 0 || char.IsWhiteSpace(s, bIndex))
                                outIndices.Add(kwi);
                        }
                    }
                }
                else
                {
                    outIndices.UnionWith(node.KWIndices);
                }
                return false;
            });

            return outIndices.ToArray();
        }

        public string RemoveKeywords(string s, KWSearchOptions options)
        {
            var keywords = new List<string>();
            var indices = new List<int>();

            SearchWithAction(s, options, (i, node) =>
            {
                string longestKW = null;
                int index = -1;

                if (i == s.Length - 1 || char.IsWhiteSpace(s, i + 1))
                {
                    foreach (var kwi in node.KWIndices)
                    {
                        var kw = _kwList[kwi];
                        var bIndex = i - kw.Length;
                        if (bIndex < 0 || char.IsWhiteSpace(s, bIndex))
                        {
                            if (longestKW == null || longestKW.Length < kw.Length)
                            {
                                index = bIndex + 1;
                                longestKW = kw;
                            }
                        }
                    }
                }

                if (longestKW != null)
                {
                    keywords.Add(longestKW);
                    indices.Add(index);
                }

                return false;
            });

            var newS = s;
            var totalLength = 0;

            for (int i = 0; i < keywords.Count; i++)
            {
                var index = indices[i] - totalLength;
                var length = keywords[i].Length;

                if (index >= 0 && index < newS.Length)
                {
                    var t = newS.Substring(index, length);
                    if (string.Equals(t, keywords[i]))
                    {
                        newS = newS.Remove(index, length);
                    }
                }

                totalLength += length;
            }

            var result = new StringBuilder();
            var space = false;

            for (int i = 0; i < newS.Length; i++)
            {
                if (char.IsWhiteSpace(newS, i))
                {
                    if (!space)
                    {
                        result.Append(newS[i]);
                        space = true;
                    }
                }
                else
                {
                    result.Append(newS[i]);
                    space = false;
                }
            }

            return result.ToString().Trim();
        }

        private void SearchWithAction(string s, KWSearchOptions option, Func<int, TrieNode, bool> processResult)
        {
            var curNode = _root;
            if (option.HasFlag(KWSearchOptions.IgnoreCase))
            {
                s = s.ToLower();
            }

            for (int i = 0; i < s.Length; i++)
            {
                var c = s[i];
                while (curNode != null && !curNode.NextNodes.ContainsKey(c))
                {
                    curNode = curNode.FailNode;
                }

                if (curNode != null)
                {
                    curNode = curNode.NextNodes[c];
                    if (curNode.KWIndices.Count > 0)
                    {
                        if (processResult(i, curNode))
                            return;
                    }
                }
                else
                {
                    curNode = _root;
                }
            }
        }

        class TrieNode
        {
            public Dictionary<char, TrieNode> NextNodes { get; }
                = new Dictionary<char, TrieNode>();

            public HashSet<int> KWIndices { get; }
                = new HashSet<int>();

            public int ExactKWIndex { get; set; }

            public TrieNode FailNode { get; set; }
        }
    }
}
