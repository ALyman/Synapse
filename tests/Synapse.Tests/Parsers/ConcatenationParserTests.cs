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
using MbUnit.Framework;
using Synapse.Parsers;

namespace Synapse.Tests.Parsers
{
    [TestFixture]
    public class ConcatenationParserTests
    {
        [Test]
        public void When_both_parts_match()
        {
            var input = new MockInput<int>();
            var parser = new ConcatenationParser<int, char, char, string>(
                new MockSuccessfulParser<int, char>('s'),
                (first) =>
                    {
                        Assert.AreEqual('s', first);
                        return new MockSuccessfulParser<int, char>('d');
                    },
                (a, b) =>
                    {
                        Assert.AreEqual('s', a);
                        Assert.AreEqual('d', b);
                        return "x";
                    });
            var result = parser.Parse(input);
            ParseResultAssert.IsSuccess(result);
            ParseResultAssert.AreEqual("x", result);
        }

        [Test]
        public void When_both_parts_fail()
        {
            var input = new MockInput<int>();
            var parser = new ConcatenationParser<int, char, char, string>(
                new MockFailureParser<int, char>(),
                (first) =>
                    {
                        Assert.Fail();
                        return new MockFailureParser<int, char>();
                    },
                (a, b) =>
                    {
                        Assert.Fail();
                        return "x";
                    });
            var result = parser.Parse(input);
            ParseResultAssert.IsFailure(result);
        }

        [Test]
        public void When_the_first_part_matches_but_the_second_fails()
        {
            var input = new MockInput<int>();
            var parser = new ConcatenationParser<int, char, char, string>(
                new MockSuccessfulParser<int, char>('s'),
                new MockFailureParser<int, char>(),
                (a, b) =>
                    {
                        Assert.Fail();
                        return "";
                    });
            var result = parser.Parse(input);
            ParseResultAssert.IsFailure(result);
        }
    }
}