using System;

using System.Globalization;

using System.Text;
using System.Text.RegularExpressions;

namespace Bridge.Visualizers.Utils
{
    internal class TypeNameHelper
    {
        public static string GetDisplayName(Type type, bool fullName)
        {
            if (type == null)
            {
                return string.Empty;
            }

            if (type.IsGenericParameter)
            {
                return type.Name;
            }

            if (!type.IsGenericType && !type.IsArray)
            {
                return fullName ? type.FullName : type.Name;
            }

            // replace `2 with <type1, type2="">
            var regex = new Regex("`[0-9]+");
            var evaluator = new GenericsMatchEvaluator(type.GetGenericArguments(), fullName);

            // Remove [[fullName1, ..., fullNameX]]
            var name = fullName ? type.FullName : type.Name;
            var start = name.IndexOf("[[", StringComparison.Ordinal);
            var end = name.LastIndexOf("]]", StringComparison.Ordinal);
            if (start > 0 && end > 0)
            {
                name = name.Substring(0, start) + name.Substring(end + 2);
            }
            return regex.Replace(name, evaluator.Evaluate);
        }

        class GenericsMatchEvaluator
        {
            readonly Type[] _generics;
            int _index;
            readonly bool _fullName;

            public GenericsMatchEvaluator(Type[] generics, bool fullName)
            {
                _generics = generics;
                _index = 0;
                _fullName = fullName;
            }

            public string Evaluate(Match match)
            {
                var numberOfParameters = int.Parse(match.Value.Substring(1), CultureInfo.InvariantCulture);

                var sb = new StringBuilder();

                // matched "`N" is replaced by "<type1, ...,="" typen="">"
                sb.Append("<");

                for (var i = 0; i < numberOfParameters; i++)
                {
                    if (i > 0)
                    {
                        sb.Append(", ");
                    }

                    sb.Append(GetDisplayName(_generics[this._index++], _fullName));
                }

                sb.Append(">");

                return sb.ToString();
            }
        }
    }
}
