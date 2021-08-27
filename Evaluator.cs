using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// AST visitor to evaluate the value of the expression by simply
    /// walking the expression tree.  ie: non-optimized evaluation
    /// </summary>
    class Evaluator : IAstNodeVisitor<bool>
    {
        /// <summary>
        /// Evaluate the expression tree
        /// </summary>
        /// <param name="node">The root expression node</param>
        /// <param name="bitNames">Bit name to mask mapper</param>
        /// <param name="value">The value to be tested</param>
        /// <returns>True if the expression matches</returns>
        public bool Evaluate(AstNode node, IBitNames bitNames, uint value)
        {
            _bitNames = bitNames;
            _input = value;
            return Evaluate(node);
        }

        IBitNames _bitNames;
        uint _input;

        bool Evaluate(AstNode node)
        {
            return node.Visit(this);
        }

        bool IAstNodeVisitor<bool>.Visit(AstNodeIdentifier node)
        {
            return (_input & _bitNames.BitFromName(node.Name)) != 0;
        }

        bool IAstNodeVisitor<bool>.Visit(AstNodeAnd node)
        {
            return node.Operands.All(x => Evaluate(x));
        }

        bool IAstNodeVisitor<bool>.Visit(AstNodeOr node)
        {
            return node.Operands.Any(x => Evaluate(x));
        }

        bool IAstNodeVisitor<bool>.Visit(AstNodeNot node)
        {
            return !Evaluate(node.Operand);
        }
    }
}
