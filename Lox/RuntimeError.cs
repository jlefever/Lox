using System;
using System.Collections.Generic;
using System.Text;

namespace Lox
{
    public class RuntimeError : Exception
    {
        public override string Message { get; }
        public Token Token { get; }

        public RuntimeError(Token token, string message)
        {
            Token = token;
            Message = message;
        }
    }
}
