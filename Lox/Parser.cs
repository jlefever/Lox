using System;
using System.Collections.Generic;
using System.Reflection.Metadata;
using static Lox.TokenKind;

namespace Lox
{
    public class Parser
    {
        private class ParseError : Exception { }

        private readonly IList<Token> _tokens;
        private int _current;

        public Parser(IList<Token> tokens)
        {
            _tokens = tokens;
        }

        public IList<Stmt> Parse()
        {
            var statements = new List<Stmt>();

            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Stmt Declaration()
        {
            try
            {
                if (Match(TokenKind.Var))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Stmt Statement()
        {
            if (Match(TokenKind.Print)) return PrintStatement();
            if (Match(LeftBrace)) return new Block(Block());
            return ExpressionStatement();
        }

        private Stmt PrintStatement()
        {
            var value = Expression();
            Consume(Semicolon, "Expect ';' after value.");
            return new Print(value);
        }

        private Stmt VarDeclaration()
        {
            var name = Consume(Identifier, "Expect variable name.");

            Expr initializer = null;

            if (Match(Equal))
            {
                initializer = Expression();
            }

            Consume(Semicolon, "Expect ';' after variable declaration.");
            return new Var(name, initializer);
        }

        private Stmt ExpressionStatement()
        {
            var expr = Expression();
            Consume(Semicolon, "Expect ';' after expression.");
            return new Expression(expr);
        }

        private ICollection<Stmt> Block()
        {
            var statements = new LinkedList<Stmt>();

            while (!Check(RightBrace) && !IsAtEnd())
            {
                statements.AddLast(Declaration());
            }

            Consume(RightBrace, "Expect '}' after block.");
            return statements;
        }

        private Expr Expression()
        {
            return Assignment();
        }

        // This is right-associative.
        private Expr Assignment()
        {
            var expr = Comma();

            if (Match(Equal))
            {
                var equals = Previous();
                var value = Assignment();

                if (expr is Variable variable)
                {
                    return new Assign(variable.Name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expr;
        }

        private Expr Comma()
        {
            return BinaryHelper(Equality, TokenKind.Comma);
        }

        private Expr Equality()
        {
            return BinaryHelper(Comparison, BangEqual, EqualEqual);
        }

        private Expr Comparison()
        {
            return BinaryHelper(Addition, Greater, GreaterEqual, Less, LessEqual);
        }

        private Expr Addition()
        {
            return BinaryHelper(Multiplication, Minus, Plus);
        }

        private Expr Multiplication()
        {
            return BinaryHelper(Unary, Slash, Star);
        }

        private Expr Unary()
        {
            if (Match(Bang, Minus))
            {
                var op = Previous();
                var right = Unary();
                return new Unary(op, right);
            }

            return Primary();
        }

        private Expr Primary()
        {
            if (Match(False)) return new Literal(false);
            if (Match(True)) return new Literal(true);
            if (Match(Nil)) return new Literal(null);

            if (Match(Number, TokenKind.String))
            {
                return new Literal(Previous().Literal);
            }

            if (Match(Identifier))
            {
                return new Variable(Previous());
            }

            if (Match(LeftParen))
            {
                var expr = Expression();
                Consume(RightParen, "Expect ')' after expression.");
                return new Grouping(expr);
            }

            throw Error(Peek(), "Expect expression.");
        }

        private Token Consume(TokenKind kind, string message)
        {
            if (Check(kind)) return Advance();
            throw Error(Peek(), message);
        }

        private static ParseError Error(Token token, string message)
        {
            Lox.Error(token, message);
            return new ParseError();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Kind == Semicolon) return;

                switch (Peek().Kind)
                {
                    case Class:
                    case Fun:
                    case TokenKind.Var:
                    case For:
                    case If:
                    case While:
                    case TokenKind.Print:
                    case Return:
                        return;
                }

                Advance();
            }
        }

        private Expr BinaryHelper(Func<Expr> next, params TokenKind[] kinds)
        {
            var expr = next();

            while (Match(kinds))
            {
                var op = Previous();
                var right = next();
                expr = new Binary(expr, op, right);
            }

            return expr;
        }

        private bool Match(params TokenKind[] kinds)
        {
            foreach (var kind in kinds)
            {
                if (Check(kind))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenKind kind)
        {
            if (IsAtEnd()) return false;
            return Peek().Kind == kind;
        }

        private Token Advance()
        {
            if (!IsAtEnd()) _current++;
            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Kind == Eof;
        }

        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }
    }
}
