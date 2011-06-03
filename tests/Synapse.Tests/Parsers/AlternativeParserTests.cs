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
using Synapse.Parsers;

namespace Synapse.Tests.Parsers
{
    [TestFixture]
    public class AlternativeParserTests
    {
        [Test]
        public void When_all_alternatives_match()
        {
            var input = new MockInput<int>();
            var parser = new AlternativeParser<int, char>(
                new MockSuccessfulParser<int, char>('1'),
                new MockSuccessfulParser<int, char>('2'),
                new MockSuccessfulParser<int, char>('3'));
            var result = parser.Parse(input);
            ParseResultAssert.IsSuccess(result);
            ParseResultAssert.AreEqual('1', result);
        }

        [Test]
        public void When_the_first_alternative_matches()
        {
            var input = new MockInput<int>();
            var parser = new AlternativeParser<int, char>(
                new MockSuccessfulParser<int, char>('1'),
                new MockFailureParser<int, char>());
            var result = parser.Parse(input);
            ParseResultAssert.IsSuccess(result);
            ParseResultAssert.AreEqual('1', result);
        }

        [Test]
        public void When_the_second_alternative_matches()
        {
            var input = new MockInput<int>();
            var parser = new AlternativeParser<int, char>(
                new MockFailureParser<int, char>(),
                new MockSuccessfulParser<int, char>('2'));
            var result = parser.Parse(input);
            ParseResultAssert.IsSuccess(result);
            ParseResultAssert.AreEqual('2', result);
        }

        [Test]
        public void When_the_third_alternative_matches()
        {
            var input = new MockInput<int>();
            var parser = new AlternativeParser<int, char>(
                new MockFailureParser<int, char>(),
                new MockSuccessfulParser<int, char>('2'),
                new MockSuccessfulParser<int, char>('3'));
            var result = parser.Parse(input);
            ParseResultAssert.IsSuccess(result);
            ParseResultAssert.AreEqual('2', result);
        }

        [Test]
        public void When_no_alternative_matches()
        {
            var input = new MockInput<int>();
            var parser = new AlternativeParser<int, char>(
                new MockFailureParser<int, char>(),
                new MockFailureParser<int, char>(),
                new MockFailureParser<int, char>());
            var result = parser.Parse(input);
            ParseResultAssert.IsFailure(result);
            Assert.AreEqual(input, result.FirstInput);
            Assert.AreEqual(input, result.RemainingInput);
        }
    }
}