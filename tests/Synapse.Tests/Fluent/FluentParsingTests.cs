#region LICENSE

// Copyright (c) 2011, Alex Lyman
// All rights reserved.
// 
// Redistribution and use in source and binary forms, with or without modification, are permitted provided that the following conditions are met:
// - Redistributions of source code must retain the above copyright notice, this list of conditions and the following disclaimer.
// - Redistributions in binary form must reproduce the above copyright notice, this list of conditions and the following disclaimer in the documentation and/or other materials provided with the distribution.
// 
// Neither the name of Synapse nor the names of its contributors may be used to endorse or promote products derived from this software without specific prior written permission.
// THIS SOFTWARE IS PROVIDED BY THE COPYRIGHT HOLDERS AND CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT SHALL THE COPYRIGHT HOLDER OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT, INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE, EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.

#endregion

using System;
using System.Collections.Generic;
using System.Linq;
#if TEST_MBUNIT
using TestFixtureAttribute =  MbUnit.Framework.TestFixtureAttribute;
using TestAttribute =  MbUnit.Framework.TestAttribute;
using Assert =  MbUnit.Framework.Assert;
using CollectionAssert =  MbUnit.Framework.Assert;
#else
using TestFixtureAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestClassAttribute;
using TestAttribute = Microsoft.VisualStudio.TestTools.UnitTesting.TestMethodAttribute;
using Assert = Microsoft.VisualStudio.TestTools.UnitTesting.Assert;
using CollectionAssert = Synapse.Tests.Utilities.CollectionAssert;
#endif

namespace Synapse.Tests.Fluent
{
    [TestFixture]
    public class FluentParsingTests
    {
        [Test]
        public void When_I_match_a_token()
        {
            var input = new[] {1}.AsInput();
            var parser = Parse.Match(1);
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(1, actualResult);
        }

        [Test]
        public void When_I_match_a_token_with_a_special_comparer()
        {
            var input = new[] {1}.AsInput();
            var parser = Parse.Match(2, new SpecialIntComparer());
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(1, actualResult);
        }

        [Test]
        public void When_I_concatenate_two_tokens()
        {
            var input = new[] {1, 2}.AsInput();
            var parser = from a in Parse.Match(1)
                         from b in Parse.Match(2)
                         select a + b;
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(3, actualResult);
        }

        [Test]
        public void When_I_project_a_value()
        {
            var input = new[] {1, 2}.AsInput();
            var parser = from a in Parse.Match(1)
                         select 7;
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(7, actualResult);
        }

        [Test]
        public void When_I_repeat_zero_or_more_but_dont_match()
        {
            var input = new[] {2}.AsInput();
            var parser = Parse.Match(1).ZeroOrMore();
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            CollectionAssert.AreElementsEqual(new int[0], actualCollection);
        }

        [Test]
        public void When_I_repeat_zero_or_more_and_match()
        {
            var input = new[] {1, 1, 1, 2}.AsInput();
            var parser = Parse.Match(1).ZeroOrMore();
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            CollectionAssert.AreElementsEqual(new[] {1, 1, 1}, actualCollection);
        }

        [Test]
        public void When_I_repeat_one_or_more_but_dont_match()
        {
            var input = new[] {2}.AsInput();
            var parser = Parse.Match(1).OneOrMore();
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        [Test]
        public void When_I_repeat_one_or_more_and_match()
        {
            var input = new[] {1, 1, 1, 2}.AsInput();
            var parser = Parse.Match(1).OneOrMore();
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            CollectionAssert.AreElementsEqual(new[] {1, 1, 1}, actualCollection);
        }

        [Test]
        public void When_I_repeat_some_and_match()
        {
            var input = new[] {1, 1, 1, 2}.AsInput();
            var parser = Parse.Match(1).Repeat(minimumCount: 2, maximumCount: 4);
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            CollectionAssert.AreElementsEqual(new[] {1, 1, 1}, actualCollection);
        }

        [Test]
        public void When_I_repeat_some_and_match_miss_count()
        {
            var input = new[] {1, 1, 1, 2}.AsInput();
            var parser = Parse.Match(1).Repeat(minimumCount: 2, maximumCount: 2, greedy: true);
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        [Test]
        public void When_I_or_and_the_first_alternative_matches()
        {
            var input = new[] {1}.AsInput();
            var parser = Parse.Or(Parse.Match(1), Parse.Match(2));
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(1, actualResult);
        }

        [Test]
        public void When_I_or_and_the_second_alternative_matches()
        {
            var input = new[] {2}.AsInput();
            var parser = Parse.Or(Parse.Match(1), Parse.Match(2));
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
            ParseResultAssert.AreEqual(2, actualResult);
        }

        [Test]
        public void When_I_or_and_no_alternative_matches()
        {
            var input = new[] {3}.AsInput();
            var parser = Parse.Or(Parse.Match(1), Parse.Match(2));
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        [Test]
        public void When_I_match_the_end_of_input_when_at_the_end_of_input()
        {
            var input = new int[] {}.AsInput();
            var parser = Parse.End<int>();
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsSuccess(actualResult);
        }

        [Test]
        public void When_I_match_the_end_of_input_when_not_at_the_end_of_input()
        {
            var input = new[] {1}.AsInput();
            var parser = Parse.End<int>();
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        #region Nested type: SpecialIntComparer

        private class SpecialIntComparer : IEqualityComparer<int>
        {
            #region IEqualityComparer<int> Members

            public bool Equals(int x, int y)
            {
                return x + 1 == y;
            }

            public int GetHashCode(int obj)
            {
                return obj.GetHashCode();
            }

            #endregion
        }

        #endregion
    }
}