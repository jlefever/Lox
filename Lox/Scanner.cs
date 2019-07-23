using System;
using System.Collections.Generic;

using static Lox.TokenKind;

namespace Lox
{
    public class Scanner
    {
        private readonly string _source;
        private readonly IList<Token> _tokens = new List<Token>();

        private int _start = 0;
        private int _current = 0;
        private int _line = 1;

        public Scanner(string source)
        {
            _source = source;
        }

        public IList<Token> ScanTokens()
        {
            while (!IsAtEnd())
            {
                _start = _current;
                ScanToken();
            }

            _tokens.Add(new Token(Eof, "", null, _line));
            return _tokens;
        }

        private void ScanToken()
        {
            var c = Advance();
            switch (c)
            {
                case '(': AddToken(LeftParen); break;
                case ')': AddToken(RightParen); break;
                case '{': AddToken(LeftBrace); break;
                case '}': AddToken(RightBrace); break;
                case ',': AddToken(Comma); break;
                case '.': AddToken(Dot); break;
                case '-': AddToken(Minus); break;
                case '+': AddToken(Plus); break;
                case ';': AddToken(Semicolon); break;
                case '*': AddToken(Star); break;
                case '!': AddToken(Match('=') ? BangEqual : Bang); break;
                case '=': AddToken(Match('=') ? EqualEqual : Equal); break;
                case '<': AddToken(Match('=') ? LessEqual : Less); break;
                case '>': AddToken(Match('=') ? GreaterEqual : Greater); break;

                case '/':
                    if (Match('/'))
                    {
                        // A comment goes until the end of the line.
                        while (Peek() != '\n' && !IsAtEnd()) Advance();
                    }
                    else
                    {
                        AddToken(Slash);
                    }
                    break;

                case '\n': _line++; break;
                case '"': String(); break;

                case ' ':
                case '\r':
                case '\t':
                    break;

                default:
                    if (IsDigit(c))
                    {
                        Number();
                    }
                    else if (IsAlpha(c))
                    {
                        Identifier();
                    }
                    else
                    {
                        Lox.Error(_line, "Unexpected character.");
                    }
                    break;
            }
        }

        private void Identifier()
        {
            while (IsAlphaNumeric(Peek())) Advance();

            if (!Keywords.TryGetValue(Extract(), out var kind))
            {
                kind = TokenKind.Identifier;
            }

            AddToken(kind);
        }

        private void Number()
        {
            while (IsDigit(Peek())) Advance();

            // Look for a fractional part.
            if (Peek() == '.' && IsDigit(PeekNext()))
            {
                Advance();
                while (IsDigit(Peek())) Advance();
            }

            var value = Convert.ToDouble(Extract());
            AddToken(TokenKind.Number, value);
        }

        private void String()
        {
            while (Peek() != '"' && !IsAtEnd())
            {
                if (Peek() == '\n') _line++;
                Advance();
            }

            // Handle an unterminated string.
            if (IsAtEnd())
            {
                Lox.Error(_line, "Unterminated string.");
                return;
            }

            // Advance the closing ".
            Advance();

            // Extract while trimming the surrounding quotes.
            var value = _source.Substring(_start + 1, _current - _start - 2);
            AddToken(TokenKind.String, value);
        }

        private bool Match(char expected)
        {
            if (IsAtEnd() || _source[_current] != expected)
            {
                return false;
            }

            _current++;
            return true;
        }

        private char Peek()
        {
            return IsAtEnd() ? '\0' : _source[_current];
        }

        private char PeekNext()
        {
            if (_current + 1 >= _source.Length) return '\0';
            return _source[_current + 1];
        }

        private static bool IsAlpha(char c)
        {
            return c >= 'a' && c <= 'z' || c >= 'A' && c <= 'Z' || c == '_';
        }

        private static bool IsDigit(char c)
        {
            return c >= '0' && c <= '9';
        }

        private static bool IsAlphaNumeric(char c)
        {
            return IsAlpha(c) || IsDigit(c);
        }

        private bool IsAtEnd()
        {
            return _current >= _source.Length;
        }

        private char Advance()
        {
            _current++;
            return _source[_current - 1];
        }

        private void AddToken(TokenKind kind, object literal = null)
        {
            _tokens.Add(new Token(kind, Extract(), literal, _line));
        }

        private string Extract()
        {
            return _source.Substring(_start, _current - _start);
        }

        private static readonly IDictionary<string, TokenKind> Keywords = new Dictionary<string, TokenKind>
        {
            { "and", And },
            { "class", Class },
            { "else", Else },
            { "false", False },
            { "for", For },
            { "fun", Fun },
            { "if", If },
            { "nil", Nil },
            { "or", Or },
            { "print", TokenKind.Print },
            { "return", Return },
            { "super", Super },
            { "this", This },
            { "true", True },
            { "var", Var },
            { "while", While },
        };
    }
}
