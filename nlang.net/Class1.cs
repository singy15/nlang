using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nlang.net
{
    public enum TokenClass
    {
        LP, // left paren
        RP, // right paren
        SYM, // symbol
        NUM, // number
        STR, // string
        QTE, // quote
    }

    public class Token
    {
        public TokenClass Cls { get; set; }

        public string Text { get; set; }

        public Token(TokenClass cls, string text)
        {
            this.Cls = cls;
            this.Text = text;
        }
    }

    public class TokenizerState {
        public string Source { get; set; }

        public string CurChar1 { get; set; }

        public string CurChar2 { get; set; }

        public int Index { get; set; }

        public string Buffer { get; set; }

        public int Count { get; set; }

        public List<Token> Tokens { get; set; }
    }

    public class Tokenizer
    {
        public static readonly int TokeninzeErrorCount = 10000;  

        public TokenizerState State { get; set; }

        public List<Token> Tokenize(string source)
        {
            State = new TokenizerState();
            return new List<Token>();
        }

        private bool IsDelimiter(string c)
        {
            return c == " " || c == Environment.NewLine;
        }

        private bool Read() {
            State.Count++;
            if (State.Count > TokeninzeErrorCount) return false;
            State.Index += 1;
            if (State.Index >= State.Source.Length) return false;
            State.CurChar1 = State.Source.Substring(State.Index, 1);
            State.CurChar2 = State.Source.Substring(State.Index + 1, 1);
            return true;
        }

        private void Skip() { }

        private void PushBuffer() {
            State.Buffer += State.CurChar1;
        }

        private void Back()
        {
            State.Index -= 1;
        }

        private void Accept(TokenClass cls)
        {
            State.Tokens.Add(new Token(cls, State.Buffer));
            ClearBuffer();
        }

        private void ClearBuffer()
        {
            State.Buffer = "";
        }


    }
}
