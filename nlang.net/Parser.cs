using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace nlang.net
{
    public class Node
    {
        public List<object> Children { get; set; } = new List<object>();
    }

    public class ParserState
    {
        public Token CurToken { get; set; }

        public List<Token> Tokens { get; set; }

        public List<Node> CurNode { get; set; } = new List<Node>();

        public Node Root { get; set; }

        public int Index { get; set; } = -1;

        public int Count { get; set; } = 0;

        public ParserState(List<Token> tokens)
        {
            this.Tokens = tokens;
            Root = new Node();
            Root.Children.Add(new Token(TokenClass.SYM, "progn"));
            CurNode.Add(Root);
        }
    }

    public class Parser
    {
        public static readonly int ParserErrorCount = 10000;

        public ParserState State { get; set; }

        public Node Parse(List<Token> tokens)
        {
            State = new ParserState(tokens);

            while (Next())
            {
                if (State.CurToken.Cls == TokenClass.LP)
                {
                    OpenNode();
                }
                else if (State.CurToken.Cls == TokenClass.RP)
                {
                    CloseNode();
                }
                else
                {
                    State.CurNode[0].Children.Add(State.CurToken);
                }
            }

            return State.Root;
        }

        public bool Next()
        {
            State.Count++;
            if (State.Count > ParserErrorCount) return false;
            State.Index++;
            if (State.Index >= State.Tokens.Count) return false;
            State.CurToken = State.Tokens[State.Index];
            return true;
        }

        public void OpenNode()
        {
            var node = new Node();
            State.CurNode[0].Children.Add(node);
            State.CurNode.Insert(0, node);
        }

        public void CloseNode()
        {
            State.CurNode.RemoveAt(0);
        }
    }
}
