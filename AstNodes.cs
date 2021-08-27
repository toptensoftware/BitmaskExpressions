using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// Abstract base class for all experession nodes
    /// </summary>
    abstract class AstNode
    {
        public abstract T Visit<T>(IAstNodeVisitor<T> visitor);
    }

    /// <summary>
    /// Represents an identifier in the expression 
    /// </summary>
    /// <remarks>
    /// A = 0x0001, B = 0x0002, C = 0x0004, etc...
    /// </remarks>
    class AstNodeIdentifier : AstNode
    {
        /// <summary>
        /// Constructs a new identifier node
        /// </summary>
        /// <param name="bit">The bit represented by this identifier</param>
        public AstNodeIdentifier(string name)
        {
            _name = name;
        }

        /// <summary>
        /// The bit represented by this identifier
        /// </summary>
        public string Name => _name;

        /// <inheritdoc />
        public override T Visit<T>(IAstNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }

        string _name;
    }

    /// <summary>
    /// Abstract base class for And and Or nodes which support
    /// multiple inputs
    /// </summary>
    abstract class AstNodeMultiOperand : AstNode
    {
        /// <summary>
        /// Add an input operand to this node
        /// </summary>
        /// <param name="node"></param>
        public void AddOperand(AstNode node)
        {
            _operands.Add(node);
        }

        /// <summary>
        /// Get the list of input operands 
        /// </summary>
        public IList<AstNode> Operands => _operands;

        /// <summary>
        /// Goes through input operands and any that are the same
        /// type as this operand, remove from child node and place
        /// directly in this node.
        /// </summary>
        /// <remarks>
        /// This effectively converts "A || (B || C)" to "A || B || C"
        /// and same for And
        /// </remarks>
        public void Simplify()
        {
            for (int i = 0; i < Operands.Count; i++)
            {
                if (Operands[i].GetType() == this.GetType())
                {
                    // Promote it's operands up to us
                    var oper = (AstNodeMultiOperand)Operands[i];
                    Operands.RemoveAt(i);
                    for (int j = 0; j < oper.Operands.Count; j++)
                    {
                        Operands.Insert(i + j, oper.Operands[j]);
                    }
                }
            }
        }

        List<AstNode> _operands = new();
    }

    /// <summary>
    /// Represents a boolean And operator
    /// </summary>
    class AstNodeAnd : AstNodeMultiOperand
    {
        /// <inheritdoc />
        public override T Visit<T>(IAstNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    /// <summary>
    /// Represents a boolean Or operator
    /// </summary>
    class AstNodeOr : AstNodeMultiOperand
    {
        /// <inheritdoc />
        public override T Visit<T>(IAstNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }

    /// <summary>
    /// Represents a boolean Or operator
    /// </summary>
    class AstNodeNot : AstNode
    {
        /// <summary>
        /// The input operand (RHS)
        /// </summary>
        public AstNode Operand { get; set; }

        /// <inheritdoc />
        public override T Visit<T>(IAstNodeVisitor<T> visitor)
        {
            return visitor.Visit(this);
        }
    }
}
