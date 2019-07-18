﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox.Tools
{
    public class AstGenerator
    {
        private readonly IEnumerable<Type> _types;
        private readonly string _baseName;
        private readonly string _namespace;

        public AstGenerator(IEnumerable<string> descriptions, string baseName, string @namespace)
        {
            _types = descriptions.Select(ExtractType);
            _baseName = baseName;
            _namespace = @namespace;
        }

        public string DefineAst()
        {
            var builder = new StringBuilder();
            var name = GetType().FullName;
            builder.AppendLine("// This file was generated by " + name + " on " + DateTime.Now);
            builder.AppendLine("namespace " + _namespace);
            builder.AppendLine("{");
            builder.AppendLine(Indent("public abstract class Expr { }"));

            foreach (var type in _types)
            {
                builder.AppendLine();
                builder.AppendLine(Indent(DefineType(type, _baseName)));
            }

            builder.Append("}");
            return builder.ToString();
        }

        private static string DefineType(Type type, string baseName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("public class " + type.TypeName + " : " + baseName);
            builder.AppendLine("{");

            foreach (var field in type.Fields)
            {
                builder.AppendLine(Indent(DefineField(field)));
            }

            builder.AppendLine();
            builder.AppendLine(Indent(DefineConstructor(type)));
            builder.Append("}");
            return builder.ToString();
        }

        private static string DefineField(Field field)
        {
            return "public " + field.TypeName + " " + field.FieldName + " { get; }";
        }

        private static string DefineConstructor(Type type)
        {
            var builder = new StringBuilder();
            builder.Append("public " + type.TypeName + " (");
            builder.Append(string.Join(", ", type.Fields.Select(DefineParam)));
            builder.AppendLine(")");
            builder.AppendLine("{");

            foreach (var field in type.Fields)
            {
                var statement = field.FieldName + " = " + ToCamelCase(field.FieldName) + ";";
                builder.AppendLine(Indent(statement));
            }

            builder.Append("}");
            return builder.ToString();
        }

        private static string DefineParam(Field field)
        {
            return field.TypeName + " " + ToCamelCase(field.FieldName);
        }

        private static string ToCamelCase(string text)
        {
            var first = text.Substring(0, 1).ToLower();
            return first + text.Substring(1);
        }

        private static string Indent(string text, int indent = 1)
        {
            var tab = string.Concat(Enumerable.Repeat(" ", indent * 4));
            text = tab + text;
            return text.Replace(Environment.NewLine, Environment.NewLine + tab);
        }

        private static Type ExtractType(string desc)
        {
            var split = desc.Trim().Split(':');
            return new Type(split[0].Trim(), ExtractFields(split[1]));
        }

        private static IEnumerable<Field> ExtractFields(string desc)
        {
            return desc.Trim().Split(',').Select(ExtractField);
        }

        private static Field ExtractField(string desc)
        {
            var split = desc.Trim().Split(' ');
            return new Field(split[0].Trim(), split[1].Trim());
        }

        #region Intermediate Representation

        private class Type
        {
            public string TypeName { get; }
            public IEnumerable<Field> Fields { get; }

            public Type(string typeName, IEnumerable<Field> fields)
            {
                TypeName = typeName;
                Fields = fields;
            }
        }

        private class Field
        {
            public string TypeName { get; }
            public string FieldName { get; }

            public Field(string typeName, string fieldName)
            {
                TypeName = typeName;
                FieldName = fieldName;
            }
        }

        #endregion
    }
}
