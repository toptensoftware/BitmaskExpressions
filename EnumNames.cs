using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BitmaskExpressions
{
    public class EnumNames : IBitNames
    {
        public EnumNames(Type type)
        {
            if (!type.IsEnum)
                throw new InvalidOperationException("Exepected an enum type");
            _type = type;

            var values = Enum.GetValues(_type);
            var names = Enum.GetNames(_type);
            for (int i = 0; i < values.Length; i++)
            {
                _map.Add(names[i], (ulong)Convert.ChangeType(values.GetValue(i), typeof(ulong)));
            }

        }

        Type _type;
        Dictionary<string, ulong> _map = new();

        public ulong BitFromName(string name)
        {
            if (_map.TryGetValue(name, out var value))
                return value;

            throw new InvalidOperationException($"'{name}' is not a member of '{_type}'");
        }
    }

    public class EnumNames<T> : EnumNames where T:Enum
    {
        public EnumNames() : base(typeof(T))
        {
        }
    }
}
