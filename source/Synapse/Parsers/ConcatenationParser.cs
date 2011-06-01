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
using Synapse.Input;
using Synapse.Results;

namespace Synapse.Parsers
{
    /// <summary>
    /// A parser that concatenates two parsers together, and projects thier results into a final value.
    /// </summary>
    /// <typeparam name="TToken">The type of the token.</typeparam>
    /// <typeparam name="TFirst">The type of the first.</typeparam>
    /// <typeparam name="TSecond">The type of the second.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public class ConcatenationParser<TToken, TFirst, TSecond, TResult> : IParser<TToken, TResult>
    {
        private readonly IParser<TToken, TFirst> first;
        private readonly Func<TFirst, IParser<TToken, TSecond>> getSecond;
        private readonly Func<TFirst, TSecond, TResult> projection;

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationParser&lt;TToken, TFirst, TSecond, TResult&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="second">The second.</param>
        /// <param name="projection">The projection.</param>
        public ConcatenationParser(IParser<TToken, TFirst> first, IParser<TToken, TSecond> second,
                                   Func<TFirst, TSecond, TResult> projection)
            : this(first, any => second, projection)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConcatenationParser&lt;TToken, TFirst, TSecond, TResult&gt;"/> class.
        /// </summary>
        /// <param name="first">The first.</param>
        /// <param name="getSecond">The get second.</param>
        /// <param name="projection">The projection.</param>
        public ConcatenationParser(IParser<TToken, TFirst> first, Func<TFirst, IParser<TToken, TSecond>> getSecond,
                                   Func<TFirst, TSecond, TResult> projection)
        {
            this.first = first;
            this.getSecond = getSecond;
            this.projection = projection;
        }

        #region IParser<TToken,TResult> Members

        /// <summary>
        /// Parses the specified input.
        /// </summary>
        /// <param name="input">The input.</param>
        /// <returns>A <see cref="IParseResult{TToken,TResult}"/> containing the result of the parsing.</returns>
        public IParseResult<TToken, TResult> Parse(IInput<TToken> input)
        {
            return this.first.Parse(input).IfSuccess(
                (firstResult) =>
                    {
                        return this.getSecond(firstResult.Result)
                            .Parse(firstResult.RemainingInput)
                            .IfSuccess(
                                (secondResult) => ParseResult.Success
                                                      (input,
                                                       secondResult.RemainingInput,
                                                       this.projection(firstResult.Result, secondResult.Result)));
                    }
                );
        }

        #endregion
    }
}