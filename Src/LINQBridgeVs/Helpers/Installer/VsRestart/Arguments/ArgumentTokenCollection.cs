using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace MidnightDevelopers.VisualStudio.VsRestart.Arguments
{
    internal class ArgumentTokenCollection : IEnumerable<IArgumentToken>
    {
        private readonly List<IArgumentToken> _arguments;
        public static readonly ArgumentTokenCollection Empty = new ArgumentTokenCollection();

        public ArgumentTokenCollection()
        {
            _arguments = new List<IArgumentToken>();
        }

        public void Add(IArgumentToken token)
        {
            _arguments.Add(token);
        }

        public void Replace<TArgument>(IArgumentToken target) where TArgument : class, IArgumentToken
        {
            var source = _arguments.OfType<TArgument>().FirstOrDefault();
            if (source != null)
            {
                var index = _arguments.IndexOf(source);
                if (index >= 0)
                {
                    _arguments[index] = target;
                }

                Debug.Assert(_arguments.IndexOf(source) == -1);
            }
        }

        public IEnumerator<IArgumentToken> GetEnumerator()
        {
            return _arguments.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            using (var enumerator = GetEnumerator())
            {
                if (enumerator.MoveNext())
                {
                    sb.Append(enumerator.Current);
                }

                while (enumerator.MoveNext())
                {
                    sb.Append(" ").Append(enumerator.Current);
                }
            }

            return sb.ToString();
        }
    }
}