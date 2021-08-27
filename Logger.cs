using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// AST visitor to log the AST tree to console
    /// </summary>
    class Logger : IAstNodeVisitor<bool>
    {
        int _indent = 0;

        /// <summary>
        /// Log a node to console
        /// </summary>
        /// <param name="node"></param>
        public void Log(AstNode node)
        {
            _indent++;
            node.Visit(this);
            _indent--;
        }

        /// <summary>
        /// Helper to output an indented line
        /// </summary>
        /// <param name="str"></param>
        void WriteLine(string str)
        {
            Console.WriteLine($"{new string(' ', _indent * 2)}{str}");
        }

        public bool Visit(AstNodeIdentifier node)
        {
            WriteLine($"Identifier 0x{node.Bit:X2}");
            return true;
        }

        public bool Visit(AstNodeAnd node)
        {
            WriteLine($"AND");
            foreach (var o in node.Operands)
                Log(o);
            return true;
        }

        public bool Visit(AstNodeOr node)
        {
            WriteLine($"OR");
            foreach (var o in node.Operands)
                Log(o);
            return true;
        }

        public bool Visit(AstNodeNot node)
        {
            WriteLine($"NOT");
            Log(node.Operand);
            return true;
        }
    }
}
