﻿using System;
using System.Collections.Generic;
using static Lox.TokenKind;

namespace Lox
{
    public class Interpreter : IExprVisitor<object>, IStmtVisitor<object>
    {
        private Environment _env = new Environment();

        public void Interpret(IList<Stmt> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError error)
            {
                Lox.RuntimeError(error);
            }
        }

        public object VisitBinaryExpr(Binary expr)
        {
            var left = Evaluate(expr.Left);
            var right = Evaluate(expr.Right);

            switch (expr.Op.Kind)
            {
                case Greater:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left > (double)right;
                case GreaterEqual:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left >= (double)right;
                case Less:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left < (double)right;
                case LessEqual:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left <= (double)right;
                case Minus:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left - (double)right;
                case Slash:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left / (double)right;
                case Star:
                    CheckNumberOperands(expr.Op, left, right);
                    return (double)left * (double)right;
                case BangEqual:
                    return !IsEqual(left, right);
                case EqualEqual:
                    return IsEqual(left, right);
                case Plus:
                    if (left is double ld && right is double rd)
                    {
                        return ld + rd;
                    }

                    if (left is string ls && right is string rs)
                    {
                        return ls + rs;
                    }

                    throw new RuntimeError(expr.Op,
                        "Operands must be two numbers or two strings.");
            }

            // Unreachable.
            return null;
        }

        public object VisitGroupingExpr(Grouping expr)
        {
            return Evaluate(expr.Expression);
        }

        public object VisitLiteralExpr(Literal expr)
        {
            return expr.Value;
        }

        public object VisitUnaryExpr(Unary expr)
        {
            var right = Evaluate(expr.Right);

            switch (expr.Op.Kind)
            {
                case Bang:
                    return !IsTruthy(right);
                case Minus:
                    CheckNumberOperand(expr.Op, right);
                    return -(double)right;
            }

            // Unreachable
            return null;
        }

        public object VisitVariableExpr(Variable expr)
        {
            return _env.Get(expr.Name);
        }

        private static void CheckNumberOperand(Token @operator, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(@operator, "Operand must be a number");
        }

        private static void CheckNumberOperands(Token @operator, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(@operator, "Operands must be a number");
        }

        private static bool IsTruthy(object obj)
        {
            switch (obj)
            {
                case null:
                    return false;
                case bool b:
                    return b;
                default:
                    return true;
            }
        }

        private static bool IsEqual(object a, object b)
        {
            switch (a)
            {
                case null when b == null:
                    return true;
                case null:
                    return false;
                default:
                    return a.Equals(b);
            }
        }

        private static string Stringify(object obj)
        {
            switch (obj)
            {
                case null:
                    return "nil";
                case double _:
                    {
                        var text = obj.ToString();

                        if (text.EndsWith(".0"))
                        {
                            text = text.Substring(0, text.Length - 2);
                        }

                        return text;
                    }
                default:
                    return obj.ToString();
            }
        }

        public object VisitExpressionStmt(Expression stmt)
        {
            Evaluate(stmt.Expr);
            return null;
        }

        public object VisitPrintStmt(Print stmt)
        {
            Console.WriteLine(Stringify(Evaluate(stmt.Expr)));
            return null;
        }

        public object VisitVarStmt(Var stmt)
        {
            object value = null;

            if (stmt.Initializer != null)
            {
                value = Evaluate(stmt.Initializer);
            }

            _env.Define(stmt.Name.Lexeme, value);
            return null;
        }

        public object VisitAssignExpr(Assign expr)
        {
            var value = Evaluate(expr.Value);
            _env.Assign(expr.Name, value);
            return value;
        }

        private object Evaluate(Expr expr)
        {
            return expr.Accept(this);
        }

        private void Execute(Stmt stmt)
        {
            stmt.Accept(this);
        }

        public object VisitBlockStmt(Block stmt)
        {
            ExecuteBlock(stmt.Statements, new Environment(_env));
            return null;
        }

        private void ExecuteBlock(IEnumerable<Stmt> statements, Environment environment)
        {
            var previous = _env;

            try
            {
                _env = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _env = previous;
            }
        }
    }
}
