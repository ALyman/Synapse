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
using Synapse.Parsers;

namespace Synapse.Results
{
    /// <summary>
    /// A generic parse result, see <see cref="ISuccessfulParseResult{TToken,TResult}" /> and <see cref="IFailureParseResult{TToken,TResult}" />
    /// </summary>
    /// <typeparam name="TToken">The type of the token.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public interface IParseResult<TToken, out TResult>
    {
        /// <summary>
        /// Gets the first input that was read by the parse.
        /// </summary>
        IInput<TToken> FirstInput { get; }

        /// <summary>
        /// Gets the remaining input that was left after the parse.
        /// </summary>
        IInput<TToken> RemainingInput { get; }
    }

    /// <summary>
    /// Provides utility methods for <see cref="IParseResult{TToken,TResult}" />
    /// </summary>
    public static class ParseResult
    {
        /// <summary>
        /// Creates a successful result with the given value.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="firstInput">The first input.</param>
        /// <param name="remainingInput">The remaining input.</param>
        /// <param name="result">The result.</param>
        /// <returns>The result.</returns>
        public static ISuccessfulParseResult<TToken, TResult> Success<TToken, TResult>(IInput<TToken> firstInput,
                                                                                       IInput<TToken> remainingInput,
                                                                                       TResult result)
        {
            return new SuccessfulParseResult<TToken, TResult>(firstInput, remainingInput, result);
        }

        /// <summary>
        /// Creates a generic failure result.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="failedParser">The failed parser.</param>
        /// <returns>The result.</returns>
        public static IFailureParseResult<TToken, TResult> Failure<TToken, TResult>(IInput<TToken> input, IParser<TToken, TResult> failedParser)
        {
            return new FailureParseResult<TToken, TResult>(input, new[] { failedParser });
        }

        /// <summary>
        /// Creates a generic failure result.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="input">The input.</param>
        /// <param name="failedParsers">The failed parsers.</param>
        /// <returns>The result.</returns>
        public static IFailureParseResult<TToken, TResult> Failure<TToken, TResult>(IInput<TToken> input, IEnumerable<IParser<TToken>> failedParsers)
        {
            return new FailureParseResult<TToken, TResult>(input, failedParsers);
        }

        /// <summary>
        /// Creates a failure result that is a combination of underlying failures.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="failures">The failures.</param>
        /// <returns>The failure result.</returns>
        public static IFailureParseResult<TToken, TResult> CombinedFailure<TToken, TResult>(
            IEnumerable<IFailureParseResult<TToken, TResult>> failures)
        {
            return new FailureParseResult<TToken, TResult>(
                failures.First().FirstInput,
                from f in failures
                from p in f.FailedParsers
                select p);
        }

        /// <summary>
        /// Casts the specified source value to the required type.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TSource">The type of the original.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <returns></returns>
        public static IParseResult<TToken, TResult> Cast<TToken, TSource, TResult>(
            this IParseResult<TToken, TSource> source)
        {
            var successfulResult = source as ISuccessfulParseResult<TToken, TSource>;
            var failureResult = source as IFailureParseResult<TToken, TSource>;
            if (successfulResult != null)
                return Success(
                    successfulResult.FirstInput,
                    successfulResult.RemainingInput,
                    Utilities.DynamicCast<TSource, TResult>(successfulResult.Result));
            else
                return Failure<TToken, TResult>(failureResult.FirstInput, failureResult.FailedParsers);
        }

        /// <summary>
        /// If the given result is successful, then run the given action and return its result, otherwise cast the failure result to the given type and return it.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TOriginal">The type of the original.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns>The result.</returns>
        public static IParseResult<TToken, TResult> IfSuccess<TToken, TOriginal, TResult>(
            this IParseResult<TToken, TOriginal> source,
            Func<ISuccessfulParseResult<TToken, TOriginal>, IParseResult<TToken, TResult>> action)
        {
            var successfulResult = source as ISuccessfulParseResult<TToken, TOriginal>;
            if (successfulResult != null)
                return action(successfulResult);
            else
                return source.Cast<TToken, TOriginal, TResult>();
        }

        /// <summary>
        /// If the given result is a failure, then run the given action and return its result, otherwise cast the successful result to the given type and return it.
        /// </summary>
        /// <typeparam name="TToken">The type of the token.</typeparam>
        /// <typeparam name="TOriginal">The type of the original.</typeparam>
        /// <typeparam name="TResult">The type of the result.</typeparam>
        /// <param name="source">The source.</param>
        /// <param name="action">The action.</param>
        /// <returns>The result.</returns>
        public static IParseResult<TToken, TResult> IfFailure<TToken, TOriginal, TResult>(
            this IParseResult<TToken, TOriginal> source,
            Func<IFailureParseResult<TToken, TOriginal>, IParseResult<TToken, TResult>> action)
        {
            var failureResult = source as IFailureParseResult<TToken, TOriginal>;
            if (failureResult != null)
                return action(failureResult);
            else
                return source.Cast<TToken, TOriginal, TResult>();
        }
    }
}