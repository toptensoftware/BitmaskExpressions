using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// A dummy identifier mapper that maps letters
    /// to bit positions A = 0b0001, B = 0b0010, C = 0b0100 etc...
    /// </summary>
    class BitFromLetter : IBitNames
    {
        /// <inheritdoc />
        public ulong BitFromName(string name)
        {
            if (name.Length == 1 && name[0] >= 'A' && name[0] <= 'Z')
                return 1u << (name[0] - 'A');
            throw new InvalidDataException($"Unknown bit '{name}'");
        }
    }
}
