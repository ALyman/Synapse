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
using MbUnit.Framework;
using Synapse.Parsers;
using Synapse.Results;

namespace Synapse.Tests.Parsers
{
    [TestFixture]
    public class RepetitionParserTests
    {
        [Test]
        public void When_no_items_match_but_were_required()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                minimumCount: 1,
                parser: new MockSequenceParser<int, int>
                            {
                                ParseResult.Failure<int, int>(input)
                            }
                );
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        [Test]
        public void When_no_items_match_but_were_not_required()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                minimumCount: 0,
                parser: new MockSequenceParser<int, int>
                            {
                                ParseResult.Failure<int, int>(input)
                            }
                );
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            Assert.AreElementsEqual(new int[] {}, actualCollection);
        }

        [Test]
        public void When_a_single_item_matches()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                new MockSequenceParser<int, int>
                    {
                        ParseResult.Success(input, input, 1),
                        ParseResult.Failure<int, int>(input)
                    }
                );
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            Assert.AreElementsEqual(new[] {1}, actualCollection);
        }

        [Test]
        public void When_two_items_match()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                new MockSequenceParser<int, int>
                    {
                        ParseResult.Success(input, input, 1),
                        ParseResult.Success(input, input, 2),
                        ParseResult.Failure<int, int>(input)
                    }
                );
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            Assert.AreElementsEqual(new[] {1, 2}, actualCollection);
        }

        [Test]
        public void When_three_items_match()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                new MockSequenceParser<int, int>
                    {
                        ParseResult.Success(input, input, 1),
                        ParseResult.Success(input, input, 2),
                        ParseResult.Success(input, input, 3),
                        ParseResult.Failure<int, int>(input)
                    }
                );
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            Assert.AreElementsEqual(new[] {1, 2, 3}, actualCollection);
        }

        [Test]
        public void When_three_items_match_but_required_four()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                minimumCount: 4,
                parser: new MockSequenceParser<int, int>
                            {
                                ParseResult.Success(input, input, 1),
                                ParseResult.Success(input, input, 2),
                                ParseResult.Success(input, input, 3),
                                ParseResult.Failure<int, int>(input)
                            }
                );
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }

        [Test]
        public void When_three_items_match_but_only_wanted_two()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                maximumCount: 2,
                parser: new MockSequenceParser<int, int>
                            {
                                ParseResult.Success(input, input, 1),
                                ParseResult.Success(input, input, 2),
                                ParseResult.Success(input, input, 3),
                                ParseResult.Failure<int, int>(input)
                            }
                );
            var actualResult = parser.Parse(input);
            var actualCollection = ParseResultAssert.IsSuccess(actualResult);
            Assert.AreElementsEqual(new[] {1, 2}, actualCollection);
        }

        [Test]
        public void When_three_items_match_but_only_wanted_two_greedy()
        {
            var input = new MockInput<int>();
            var parser = new RepetitionParser<int, int>(
                maximumCount: 2,
                greedy: true,
                parser: new MockSequenceParser<int, int>
                            {
                                ParseResult.Success(input, input, 1),
                                ParseResult.Success(input, input, 2),
                                ParseResult.Success(input, input, 3),
                                ParseResult.Failure<int, int>(input)
                            }
                );
            var actualResult = parser.Parse(input);
            ParseResultAssert.IsFailure(actualResult);
        }
    }
}