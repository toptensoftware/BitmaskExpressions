using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    /// <summary>
    /// Pluggable interface to map identifier names to bit masks
    /// </summary>
    interface IBitNames
    {
        /// <summary>
        /// Given an identifier, return it's bitmask
        /// </summary>
        /// <param name="name">The identifier</param>
        /// <returns>A bitmask bit</returns>
        ulong BitFromName(string name);
    }
}
