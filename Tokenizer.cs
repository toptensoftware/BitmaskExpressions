using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// Simple string tokenizer
    /// </summary>
    class Tokenizer
    {
        /// <summary>
        /// Constructs a new tokenizer and reads the first token
        /// </summary>
        /// <param name="input"></param>
        public Tokenizer(string input)
        {
            _input = input;
            _pos = 0;
            NextToken();
        }

        /// <summary>
        /// Gets the current token
        /// </summary>
        public Token CurrentToken => _currentToken;

        /// <summary>
        /// Gets the bit associated with the current identifier token
        /// </summary>
        public uint IdentifierBit => _identifierBit;

        /// <summary>
        /// Move to the next token
        /// </summary>
        public void NextToken()
        {
            // Skip whitespace
            while (_pos < _input.Length && char.IsWhiteSpace(_input[_pos]))
                _pos++;

            // EOF?
            if (_pos >= _input.Length)
            {
                _currentToken = Token.EOF;
                return;
            }

            // Bit identifier A = 0x0001, B = 0x0002, C = 0x0004 etc...
            if (_input[_pos] >= 'A' && _input[_pos] <= 'Z')
            {
                _identifierBit = (uint)(1 << (_input[_pos] - 'A'));
                _currentToken = Token.Identifier;
                _pos++;
                return;
            }

            // Operators...
            switch (_input[_pos])
            {
                case '|':
                    _currentToken = Token.OperatorOr;
                    break;

                case '&':
                    _currentToken = Token.OperatorAnd;
                    break;

                case '!':
                    _currentToken = Token.OperatorNot;
                    break;

                case '(':
                    _currentToken = Token.OpenRound;
                    break;

                case ')':
                    _currentToken = Token.CloseRound;
                    break;

                default:
                    throw new InvalidDataException($"Unknown character in input sequence '{_input[_pos]}'");
            }

            _pos++;
            return;

        }

        string _input;
        int _pos;
        Token _currentToken;
        uint _identifierBit;
    }
}
