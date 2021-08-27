using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// AST visitor to evaluate the value of the expression
    /// </summary>
    class Evaluator : IAstNodeVisitor<bool>
    {
        /// <summary>
        /// Evaluate the expression tree
        /// </summary>
        /// <param name="node">The root expression node</param>
        /// <param name="value">The value to be tested</param>
        /// <returns>True if the expression matches</returns>
        public bool Evaluate(AstNode node, uint value)
        {
            _input = value;
            return Evaluate(node);
        }

        uint _input;

        bool Evaluate(AstNode node)
        {
            return node.Visit(this);
        }

        public bool Visit(AstNodeIdentifier node)
        {
            return (_input & node.Bit) != 0;
        }

        public bool Visit(AstNodeAnd node)
        {
            return node.Operands.All(x => Evaluate(x));
        }

        public bool Visit(AstNodeOr node)
        {
            return node.Operands.Any(x => Evaluate(x));
        }

        public bool Visit(AstNodeNot node)
        {
            return !Evaluate(node.Operand);
        }
    }
}
