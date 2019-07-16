using System;
using System.IO;

namespace Lox
{
    public class Lox
    {
        private static bool _hadError;

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
        }

        private static void RunPrompt()
        {
            Console.WriteLine("The Lox Programming Language");
            while (true)
            {
                Console.Write(">> ");
                Run(Console.ReadLine());
            }
        }

        private static void Run(string text)
        {
            var scanner = new Scanner(text);

            foreach (var token in scanner.ScanTokens())
            {
                Console.WriteLine(token);
            }
        }

        public static void Error(int line, string message)
        {
            Report(line, "", message);
        }

        private static void Report(int line, string where, string message)
        {
            Console.WriteLine($"[line {line}] Error{where}: {message}");
            _hadError = true;
        }
    }
}
