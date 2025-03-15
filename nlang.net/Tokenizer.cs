using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
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

    public class TokenizerState
    {
        public string Source { get; set; }

        public string CurChar1 { get; set; }

        public string CurChar2 { get; set; }

        public int Index { get; set; } = -1;

        public string Buffer { get; set; } = "";

        public int Count { get; set; } = 0;

        public List<Token> Tokens { get; set; } = new List<Token>();

        public TokenizerState(string source)
        {
            this.Source = source;
        }
    }

    public class Tokenizer
    {
        public static readonly int TokeninzeErrorCount = 10000;

        public static readonly Regex RegexNumberStartPattern = new Regex("[0-9]");

        public static readonly Regex RegexNumberPattern = new Regex("[0-9\\.]");

        public static readonly Regex RegexSymbolStartPattern = new Regex("[a-zA-Z\\+\\-\\/\\*=:><\\?!%&]");

        public static readonly Regex RegexSymbolPattern = new Regex("[a-zA-Z\\+\\-\\/\\*=:><\\?!0-9%&]");


        public TokenizerState State { get; set; }

        public List<Token> Tokenize(string source)
        {
            State = new TokenizerState(source.Replace("\r\n","\n"));

            while (Read())
            {
                if (IsDelimiter())
                {
                    Skip();

                }
                else if (State.CurChar1 == "(")
                {
                    PushBuffer();
                    Accept(TokenClass.LP);
                }
                else if (State.CurChar1 == ")")
                {
                    PushBuffer();
                    Accept(TokenClass.RP);
                }
                else if (State.CurChar1 == "\"")
                {
                    ReadString("\"");
                }
                else if (State.CurChar1 == "'")
                {
                    ReadString("'");
                }
                else if (RegexNumberStartPattern.IsMatch(State.CurChar1)
                    || (State.CurChar1 == "-" && RegexNumberStartPattern.IsMatch(State.CurChar2)))
                {
                    ReadNumber();
                }
                else if (RegexSymbolStartPattern.IsMatch(State.CurChar1))
                {
                    ReadSymbol();
                }
                else
                {
                    throw new NLangException($"Syntax error at character index {State.Index}:'{State.CurChar1}'");
                }

            }

            return State.Tokens;
        }

        private bool IsDelimiter()
        {
            return State.CurChar1 == " " || State.CurChar1 == "\n";
        }

        private bool Read()
        {
            State.Count++;
            if (State.Count > TokeninzeErrorCount) return false;
            State.Index += 1;
            if (State.Index >= State.Source.Length) return false;
            State.CurChar1 = State.Source.Substring(State.Index, 1);

            if (State.Index < (State.Source.Length - 1))
            {
                State.CurChar2 = State.Source.Substring(State.Index + 1, 1);
            }
            else
            {
                State.CurChar2 = "";
            }
            return true;
        }

        private void Skip() { }

        private void PushBuffer()
        {
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

        private void ReadString(string delimiter)
        {
            var escape = false;
            while (Read())
            {
                if (State.CurChar1 == delimiter)
                {
                    if (escape)
                    {
                        PushBuffer();
                        escape = false;
                    }
                    else
                    {
                        Accept(TokenClass.STR);
                        return;
                    }
                }
                else if (State.CurChar1 == "\\")
                {
                    if (escape)
                    {
                        PushBuffer();
                        escape = false;
                    }
                    else
                    {
                        escape = true;
                    }
                }
                else
                {
                    PushBuffer();
                }
            }
        }


        private void ReadNumber()
        {
            PushBuffer();
            while (Read())
            {
                if (RegexNumberPattern.IsMatch(State.CurChar1))
                {
                    PushBuffer();
                }
                else
                {
                    Accept(TokenClass.NUM);
                    Back();
                    return;
                }
            }
        }

        private void ReadSymbol()
        {
            PushBuffer();
            while (Read())
            {
                if (RegexSymbolPattern.IsMatch(State.CurChar1))
                {
                    PushBuffer();
                }
                else
                {
                    Accept(TokenClass.SYM);
                    Back();
                    return;
                }
            }
        }
    }
}
