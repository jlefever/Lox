using System.IO;

namespace Lox.Tools
{
    public class Program
    {
        public static void Main()
        {
            const string directory = "../../../../Lox/";
            const string @namespace = "Lox";

            const string exprName = "Expr";
            const string stmtName = "Stmt";

            var expr = AstGenerator.DefineAst(new[]
            {
                "Assign   : Token Name, Expr Value",
                "Binary   : Expr Left, Token Op, Expr Right",
                "Grouping : Expr Expression",
                "Literal  : object Value",
                "Unary    : Token Op, Expr Right",
                "Variable : Token Name"
            }, exprName, @namespace);

            var stmt = AstGenerator.DefineAst(new[]
            {
                "Block      : ICollection<Stmt> Statements",
                "Expression : Expr Expr",
                "Print      : Expr Expr",
                "Var        : Token Name, Expr Initializer"
            }, stmtName, @namespace);

            WriteFile(expr, directory, exprName);
            WriteFile(stmt, directory, stmtName);
        }

        private static void WriteFile(string text, string directory, string name)
        {
            var path = Path.Combine(directory, name + ".cs");
            using var writer = new StreamWriter(path);
            writer.WriteLine(text);
        }
    }
}
