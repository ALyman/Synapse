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
using Synapse.Input;
using Synapse.Results;

namespace Synapse.Parsers
{
    public class RepetitionParser<TToken, TResult> : IParser<TToken, IEnumerable<TResult>>
    {
        private readonly bool greedy;
        private readonly int maximumCount;
        private readonly int minimumCount;
        private readonly IParser<TToken, TResult> parser;

        public RepetitionParser(IParser<TToken, TResult> parser, int minimumCount = 0, int maximumCount = Int32.MaxValue,
                                bool greedy = false)
        {
            this.parser = parser;
            this.minimumCount = minimumCount;
            this.maximumCount = maximumCount;
            this.greedy = greedy;
        }

        #region IParser<TToken,IEnumerable<TResult>> Members

        public IParseResult<TToken, IEnumerable<TResult>> Parse(IInput<TToken> input)
        {
            var first = input;
            var results = new List<TResult>();
            var current = this.parser.Parse(input);

            while (current is ISuccessfulParseResult<TToken, TResult> &&
                   (this.greedy || results.Count < this.maximumCount))
            {
                results.Add(((ISuccessfulParseResult<TToken, TResult>) current).Result);
                input = current.RemainingInput;
                current = this.parser.Parse(input);
            }

            if (results.Count < this.minimumCount || results.Count > this.maximumCount)
            {
                return ParseResult.Failure<TToken, IEnumerable<TResult>>(first);
            }
            else
            {
                return ParseResult.Success(first, input, results);
            }
        }

        #endregion
    }
}