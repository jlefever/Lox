﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lox.Tools
{
    public static class AstGenerator
    {
        public static string DefineAst(IEnumerable<string> descriptions, string baseName, string @namespace)
        {
            var types = descriptions.Select(ExtractType).ToArray();

            var builder = new StringBuilder();
            var name = typeof(AstGenerator).FullName;

            builder.AppendLine("// This file was generated by " + name + " on " + DateTime.Now);
            builder.AppendLine("using System.Collections.Generic;");
            builder.AppendLine();
            builder.AppendLine("namespace " + @namespace);
            builder.AppendLine("{");
            builder.AppendLine(Indent(DefineBaseType(baseName)));
            builder.AppendLine();
            builder.AppendLine(Indent(DefineVisitor(types, baseName)));

            foreach (var type in types)
            {
                builder.AppendLine();
                builder.AppendLine(Indent(DefineType(type, baseName)));
            }

            builder.Append("}");
            return builder.ToString();
        }

        private static string DefineBaseType(string baseName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("public abstract class " + baseName);
            builder.AppendLine("{");
            builder.Append(Indent("public abstract TResult Accept<TResult>"));
            builder.AppendLine("(I" + baseName + "Visitor<TResult> visitor);");
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
            builder.AppendLine();
            builder.AppendLine(Indent(DefineAcceptImpl(type, baseName)));
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

        private static string DefineVisitor(IEnumerable<Type> types, string baseName)
        {
            var builder = new StringBuilder();
            builder.AppendLine("public interface I" + baseName + "Visitor<out TResult>");
            builder.AppendLine("{");

            foreach (var type in types)
            {
                builder.Append(Indent("TResult Visit" + type.TypeName + baseName));
                builder.Append("(" + type.TypeName + " " + ToCamelCase(baseName) + ");");
                builder.AppendLine();
            }

            builder.Append("}");
            return builder.ToString();
        }

        private static string DefineAcceptImpl(Type type, string baseName)
        {
            var builder = new StringBuilder();
            builder.Append("public override TResult Accept<TResult>");
            builder.AppendLine("(I" + baseName + "Visitor<TResult> visitor)");
            builder.AppendLine("{");
            builder.Append(Indent("return visitor.Visit"));
            builder.AppendLine(type.TypeName + baseName + "(this);");
            builder.Append("}");
            return builder.ToString();
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
