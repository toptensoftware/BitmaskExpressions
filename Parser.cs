using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// Parses an expression AST from a string
    /// </summary>
    class Parser
    {
        /// <summary>
        /// Constructs a new parser
        /// </summary>
        /// <param name="str">The expression string</param>
        public Parser(string str)
        {
            _tokenizer = new Tokenizer(str);
        }

        /// <summary>
        /// Parse an expression
        /// </summary>
        /// <returns>The AstNode for the expression</returns>
        public AstNode ParseExpression()
        {
            var node = ParseExpressionNode();
            if (_tokenizer.CurrentToken != Token.EOF)
            {
                throw new InvalidDataException($"Syntax error, expected EOF not {_tokenizer.CurrentToken}");
            }
            return node;
        }

        /// <summary>
        /// Parse an expression node
        /// </summary>
        /// <returns></returns>
        AstNode ParseExpressionNode()
        {
            return ParseBooleanOr();
        }

        /// <summary>
        /// Parse a chain or Boolean Or operators
        /// </summary>
        /// <returns>An AstNode</returns>
        AstNode ParseBooleanOr()
        {
            var lhs = ParseBooleanAnd();

            if (_tokenizer.CurrentToken == Token.OperatorOr)
            {
                var orOp = new AstNodeOr();
                orOp.AddOperand(lhs);
                while (_tokenizer.CurrentToken == Token.OperatorOr)
                {
                    _tokenizer.NextToken();
                    orOp.AddOperand(ParseBooleanAnd());
                }
                orOp.Simplify();
                return orOp;
            }

            return lhs;
        }

        /// <summary>
        /// Parse a chain or Boolean And operators
        /// </summary>
        /// <returns>An AstNode</returns>
        AstNode ParseBooleanAnd()
        {
            var lhs = ParseUnary();

            if (_tokenizer.CurrentToken == Token.OperatorAnd)
            {
                var andOp = new AstNodeAnd();
                andOp.AddOperand(lhs);
                while (_tokenizer.CurrentToken == Token.OperatorAnd)
                {
                    _tokenizer.NextToken();
                    andOp.AddOperand(ParseUnary());
                }
                andOp.Simplify();
                return andOp;
            }

            return lhs;
        }

        /// <summary>
        /// Parse unary operator (ie: Not)
        /// </summary>
        /// <returns>An AstNode</returns>
        AstNode ParseUnary()
        {
            if (_tokenizer.CurrentToken == Token.OperatorNot)
            {
                _tokenizer.NextToken();
                return new AstNodeNot()
                {
                    Operand = ParseUnary()
                };
            }

            return ParseLeaf();
        }

        /// <summary>
        /// Parse a leaf node (either an identifier or parenthesized expression)
        /// </summary>
        /// <returns>An AstNode</returns>
        AstNode ParseLeaf()
        {
            switch (_tokenizer.CurrentToken)
            {
                case Token.Identifier:
                    {
                        var node = new AstNodeIdentifier(_tokenizer.Identifier);
                        _tokenizer.NextToken();
                        return node;
                    }

                case Token.OpenRound:
                    {
                        _tokenizer.NextToken();
                        var node = ParseExpressionNode();
                        if (_tokenizer.CurrentToken != Token.CloseRound)
                        {
                            throw new InvalidDataException($"Syntax error, expected close round not {_tokenizer.CurrentToken}");
                        }
                        _tokenizer.NextToken();
                        return node;
                    }

                default:
                    throw new InvalidDataException($"Unexpected token in input stream: {_tokenizer.CurrentToken}");
            }
        }

        Tokenizer _tokenizer;
    }
}
