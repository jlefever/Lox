using System;
using System.IO;

namespace Lox
{
    public class Lox
    {
        private static readonly Interpreter Interpreter = new Interpreter();
        private static bool _hadError;
        private static bool _hadRuntimeError;

        public static void Main(string[] args)
        {
            if (args.Length > 1)
            {
                Console.WriteLine("Usage: lox [script]");
                Environment.Exit((int)ExitCode.UsageError);
            }
            else if (args.Length == 1)
            {
                RunFile(args[0]);
            }
            else
            {
                RunPrompt();
            }
        }

        private static void RunFile(string path)
        {
            Run(File.ReadAllText(path));

            if (_hadError)
            {
                Environment.Exit((int)ExitCode.DataError);
            }

            if (_hadRuntimeError)
            {
                Environment.Exit((int)ExitCode.SoftwareError);
            }
        }

        private static void RunPrompt()
        {
            Console.WriteLine("The Lox Programming Language");
            while (true)
            {
                Console.Write(">> ");
                Run(Console.ReadLine());
                _hadError = false;
            }
        }

        private static void Run(string text)
        {
            var scanner = new Scanner(text);
            var tokens = scanner.ScanTokens();
            var parser = new Parser(tokens);
            var expr = parser.Parse();

            // Stop if there was a syntax error.
            if (_hadError) return;

            Interpreter.Interpret(expr);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error{where}: {message}");
            _hadError = true;
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        public static void Error(Token token, string message)
        {
            if (token.Kind == TokenKind.Eof)
            {
                Report(token.Line, " at end", message);
            }
            else
            {
                Report(token.Line, " at '" + token.Lexeme + "'", message);
            }
        }

        public static void RuntimeError(RuntimeError error)
        {
            Console.WriteLine(error.Message + "\n[line " + error.Token.Line + "]");
            _hadRuntimeError = true;
        }
    }
}
