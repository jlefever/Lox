using System.IO;

namespace Lox.Tools
{
    public class Program
    {
        public static void Main()
        {
            const string directory = "../../../../Lox/";
            const string @namespace = "Lox";
            const string baseName = "Expr";

            var descriptions = new[]
            {
                "Binary   : Expr Left, Token Op, Expr Right",
                "Grouping : Expr Expression",
                "Literal  : object Value",
                "Unary    : Token Op, Expr Right"
            };

            var generator = new AstGenerator(descriptions, baseName, @namespace);
            var ast = generator.DefineAst();

            var path = Path.Combine(directory, baseName + ".cs");
            using var writer = new StreamWriter(path);
            writer.WriteLine(ast);
        }
    }
}
