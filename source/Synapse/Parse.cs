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
using Synapse.Parsers;

namespace Synapse
{
    /// <summary>
    /// Provides the fluent and LINQ syntaxes for creating parsers.
    /// </summary>
    public static class Parse
    {
        /// <summary>
        /// Gets a parser that matches the specified token.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <param name="match">The token to match.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, TToken> Match<TToken>(TToken match)
        {
            return new TokenMatchParser<TToken>(match);
        }

        /// <summary>
        /// Gets a parser that matches the specified token, using the specified comparer.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <param name="match">The token to match.</param>
        /// <param name="comparer">The comparer.</param>
        /// <returns>
        /// The parser.
        /// </returns>
        public static IParser<TToken, TToken> Match<TToken>(TToken match, IEqualityComparer<TToken> comparer)
        {
            return new TokenMatchParser<TToken>(match, comparer);
        }

        /// <summary>
        /// Creates a parser that matches at the end of an input stream.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <returns>The parser.</returns>
        public static IParser<TToken, TToken> End<TToken>()
        {
            return new EndOfInputParser<TToken>();
        }

        /// <summary>
        /// Creates a parser that projects the result-value of the given parser with a given function.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TSource">The type of the source.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, TResult> Select<TToken, TSource, TResult>(
            this IParser<TToken, TSource> source,
            Func<TSource, TResult> projection)
        {
            return new ProjectionParser<TToken, TSource, TResult>(source, projection);
        }

        /// <summary>
        /// Creates a parser that concatenates two parsers together, and projects the result-values.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TFirst">The type of the first.</typeparam>
        /// <typeparam name="TSecond">The type of the second.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="first">The first.</param>
        /// <param name="getSecond">The get second.</param>
        /// <param name="projection">The projection.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, TResult> SelectMany<TToken, TFirst, TSecond, TResult>(
            this IParser<TToken, TFirst> first,
            Func<TFirst, IParser<TToken, TSecond>> getSecond,
            Func<TFirst, TSecond, TResult> projection)
        {
            return new ConcatenationParser<TToken, TFirst, TSecond, TResult>(first, getSecond, projection);
        }

        /// <summary>
        /// Creates a parser that reads zero or more of the given parser.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, IEnumerable<TResult>> ZeroOrMore<TToken, TResult>(
            this IParser<TToken, TResult> parser)
        {
            return new RepetitionParser<TToken, TResult>(parser);
        }

        /// <summary>
        /// Creates a parser that reads one or more of the given parser.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, IEnumerable<TResult>> OneOrMore<TToken, TResult>(
            this IParser<TToken, TResult> parser)
        {
            return new RepetitionParser<TToken, TResult>(parser, minimumCount: 1);
        }

        /// <summary>
        /// Creates a parser that reads zero or more of the given parser, but fails if the count does not fall within the given range.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="parser">The parser.</param>
        /// <param name="minimumCount">The minimum count.</param>
        /// <param name="maximumCount">The maximum count.</param>
        /// <param name="greedy">if set to <c>true</c>, then we will read as many as possible, then check the range; otherwise, we will stop reading at the maximum count.</param>
        /// <returns>
        /// The parser.
        /// </returns>
        public static IParser<TToken, IEnumerable<TResult>> Repeat<TToken, TResult>(
            this IParser<TToken, TResult> parser, int minimumCount = 0, int maximumCount = Int32.MaxValue,
            bool greedy = true)
        {
            return new RepetitionParser<TToken, TResult>(parser, minimumCount, maximumCount, greedy);
        }

        /// <summary>
        /// Creates a parser that parses one of the given alternatives.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="alternatives">The alternatives.</param>
        /// <returns>The parser.</returns>
        public static IParser<TToken, TResult> Or<TToken, TResult>(params IParser<TToken, TResult>[] alternatives)
        {
            return new AlternativeParser<TToken, TResult>(alternatives);
        }
    }
}