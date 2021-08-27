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
        /// Constructs a new tokenizer and loads the first token
        /// </summary>
        /// <param name="input">The string to be tokenized</param>
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
        /// Gets the identifier name associated with the current identifier token
        /// </summary>
        public string Identifier
        {
            get
            {
                if (_currentToken != Token.Identifier)
                    throw new InvalidOperationException("Current token isn't an identifier");
                return _identifier;
            }
        }

        /// <summary>
        /// Move to the next token
        /// </summary>
        public void NextToken()
        {
            // Skip whitespace
            while (char.IsWhiteSpace(CurrentChar))
                _pos++;

            // EOF?
            if (_pos >= _input.Length)
            {
                _currentToken = Token.EOF;
                return;
            }

            // Identifier?
            if (char.IsLetter(CurrentChar))
            {
                var start = _pos;
                while (char.IsLetterOrDigit(CurrentChar))
                    _pos++;
                _identifier = _input.Substring(start, _pos - start);
                _currentToken = Token.Identifier;
                return;
            }

            // Operators...
            switch (CurrentChar)
            {
                case '|':
                    if (NextChar == '|')
                    {
                        _currentToken = Token.OperatorOr;
                        _pos += 2;
                        return;
                    }
                    break;

                case '&':
                    if (NextChar == '&')
                    {
                        _currentToken = Token.OperatorAnd;
                        _pos += 2;
                        return;
                    }
                    break;

                case '!':
                    _currentToken = Token.OperatorNot;
                    _pos++;
                    return;

                case '(':
                    _currentToken = Token.OpenRound;
                    _pos++;
                    return;

                case ')':
                    _currentToken = Token.CloseRound;
                    _pos++;
                    return;
            }

            throw new InvalidDataException($"Unknown token in input sequence '{CurrentChar}'");

        }

        /// <summary>
        /// Get the character at the current position
        /// </summary>
        char CurrentChar => CharAtOffset(0);

        /// <summary>
        /// Get the character at the next position
        /// </summary>
        char NextChar => CharAtOffset(1);

        /// <summary>
        /// Get the character at an offset from the current position
        /// </summary>
        /// <param name="offset">The characte roffset</param>
        /// <returns>The character or '\0' if outside bounds</returns>
        char CharAtOffset(int offset)
        {
            var pos = _pos + offset;
            if (pos < 0 || pos >= _input.Length)
                return '\0';
            else
                return _input[pos];
        }

        string _input;
        int _pos;
        Token _currentToken;
        string _identifier;
    }
}
