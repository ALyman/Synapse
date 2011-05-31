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
    public static class Parse
    {
        public static IParser<TToken, TToken> Match<TToken>(TToken match)
        {
            return new TokenMatchParser<TToken>(match);
        }

        public static IParser<TToken, TToken> Match<TToken>(TToken match, IEqualityComparer<TToken> comparer)
        {
            return new TokenMatchParser<TToken>(match, comparer);
        }

        public static IParser<TToken, TToken> End<TToken>()
        {
            return new EndOfInputParser<TToken>();
        }

        public static IParser<TToken, TResult> Select<TToken, TSource, TResult>(
            this IParser<TToken, TSource> source,
            Func<TSource, TResult> projection)
        {
            return new ProjectionParser<TToken, TSource, TResult>(source, projection);
        }

        public static IParser<TToken, TResult> SelectMany<TToken, TFirst, TSecond, TResult>(
            this IParser<TToken, TFirst> first,
            Func<TFirst, IParser<TToken, TSecond>> getSecond,
            Func<TFirst, TSecond, TResult> projection)
        {
            return new ConcatenationParser<TToken, TFirst, TSecond, TResult>(first, getSecond, projection);
        }

        public static IParser<TToken, IEnumerable<TResult>> ZeroOrMore<TToken, TResult>(
            this IParser<TToken, TResult> parser)
        {
            return new RepetitionParser<TToken, TResult>(parser);
        }

        public static IParser<TToken, IEnumerable<TResult>> OneOrMore<TToken, TResult>(
            this IParser<TToken, TResult> parser)
        {
            return new RepetitionParser<TToken, TResult>(parser, minimumCount: 1);
        }

        public static IParser<TToken, IEnumerable<TResult>> Repeat<TToken, TResult>(
            this IParser<TToken, TResult> parser, int minimumCount = 0, int maximumCount = Int32.MaxValue,
            bool greedy = true)
        {
            return new RepetitionParser<TToken, TResult>(parser, minimumCount, maximumCount, greedy);
        }

        public static IParser<TToken, TResult> Or<TToken, TResult>(params IParser<TToken, TResult>[] alternatives)
        {
            return new AlternativeParser<TToken, TResult>(alternatives);
        }
    }
}