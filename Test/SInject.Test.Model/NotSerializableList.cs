#region License
// Copyright (c) 2013 Coding Adventures
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
using System.Collections.Generic;
using System.Globalization;

namespace SInject.Test.Model
{

    public class NotSerializableList : List<int>
    {
        public Dictionary<string, int> Dictionary { get; set; }

        public int I { get; set; }

        public List<int> Ints { get; set; }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            return obj.GetType() == GetType() && Equals((NotSerializableList) obj);
        }

        private bool Equals(NotSerializableList other)
        {
            return Dictionary.Count == other.Dictionary.Count && I == other.I && Ints.Count == other.Ints.Count;
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Dictionary != null ? Dictionary.GetHashCode() : 0);
                hashCode = (hashCode*397) ^ I;
                hashCode = (hashCode*397) ^ (Ints != null ? Ints.GetHashCode() : 0);
                return hashCode;
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="NotSerializableList"/> class. Random initialize 
        /// the its properties
        /// </summary>
        public NotSerializableList()
        {
            var random = new Random();
            var firstRandom = random.Next(0, 1000);
            var secondRandom = random.Next(0, 1000);
            Dictionary = new Dictionary<string, int>
            {
                {firstRandom.ToString(CultureInfo.InvariantCulture), firstRandom},
                {secondRandom.ToString(CultureInfo.InvariantCulture), secondRandom}
            };
            I = random.Next(0, 1000);

            Ints = new List<int> {1, 2, 3, 4};
        }
    }
}
