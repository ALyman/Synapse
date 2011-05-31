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
using Synapse.Input;

namespace Synapse.Results
{
    public interface IParseResult<out TToken, out TResult>
    {
        IInput<TToken> FirstInput { get; }
        IInput<TToken> RemainingInput { get; }
    }

    public static class ParseResult
    {
        public static IParseResult<TToken, TResult> Success<TToken, TResult>(IInput<TToken> firstInput,
                                                                             IInput<TToken> remainingInput,
                                                                             TResult result)
        {
            return new SuccessfulParseResult<TToken, TResult>(firstInput, remainingInput, result);
        }

        public static IParseResult<TToken, TResult> Failure<TToken, TResult>(IInput<TToken> firstInput,
                                                                             IInput<TToken> remainingInput)
        {
            return new FailureParseResult<TToken, TResult>(firstInput, remainingInput);
        }

        public static IParseResult<TToken, TToken> UnexpectedTokenFailure<TToken>(IInput<TToken> input,
                                                                                  params TToken[] expectedTokens)
        {
            return new FailureParseResult<TToken, TToken>(input, input);
        }

        public static IParseResult<TToken, TToken> UnexpectedEndOfInput<TToken>(IInput<TToken> input,
                                                                                params TToken[] expectedTokens)
        {
            return new FailureParseResult<TToken, TToken>(input, input);
        }

        public static IParseResult<TToken, TResult> Cast<TToken, TOriginal, TResult>(
            this IParseResult<TToken, TOriginal> source)
        {
            var successfulResult = source as ISuccessfulParseResult<TToken, TOriginal>;
            var failureResult = source as IFailureParseResult<TToken, TOriginal>;
            if (successfulResult != null)
                return Success(
                    successfulResult.FirstInput,
                    successfulResult.RemainingInput,
                    Utilities.DynamicCast<TOriginal, TResult>(successfulResult.Result));
            else
                return new FailureParseResult<TToken, TResult>(
                    failureResult.FirstInput,
                    failureResult.RemainingInput);
        }

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