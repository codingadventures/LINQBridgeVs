#region License
// Copyright (c) 2013 - 2018 Coding Adventures
//
// Permission is hereby granted, free of charge, to any person
// obtaining a copy of this software and associated documentation
// files (the "Software"), to deal in the Software without
// restriction, including without limitation the rights to use,
// copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the
// Software is furnished to do so, subject to the following
// conditions:
//
// The above copyright notice and this permission notice shall be
// included in all copies or substantial portions of the Software.
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND,
// EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES
// OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND
// NONINFRINGEMENT. IN NO EVENT SHALL THE AUTHORS OR COPYRIGHT
// HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY,
// WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR
// OTHER DEALINGS IN THE SOFTWARE.
#endregion


using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BridgeVs.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                IEnumerable<string> mm = Enumerable.Range(0, 100).Select(p => p.ToString());
                var resu = Process(mm);
            }
            catch (Exception e)
            {
                System.Console.WriteLine(e);
                throw;
            }
        }

        static IEnumerable Process(object target)
        {
            var en = IsEnumerable(target);
            ArrayList r = new ArrayList();
            if (en)
            {
                return ((IEnumerable<object>)target).ToList();
            }

            return r;
        }

        static bool IsEnumerable(object target)
        {
            Type @type = target.GetType();

            if (!@type.IsNestedPrivate || !@type.Name.Contains("Iterator") ||
                !@type.FullName.Contains("System.Linq.Enumerable") || !(target is IEnumerable))
                return false;

            return @type.BaseType == null || @type.BaseType.FullName.Contains("Object") || true;
        }



    }
}
