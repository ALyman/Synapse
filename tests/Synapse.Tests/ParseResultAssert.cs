﻿#region LICENSE

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
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Synapse.Results;

namespace Synapse.Tests
{
    public static class ParseResultAssert
    {
        public static void IsSuccess<TToken, TResult>(IParseResult<TToken, TResult> result)
        {
            if (!(result is ISuccessfulParseResult<TToken, TResult>))
                throw new AssertFailedException("ParseResultAssert.IsSuccess failed: ParseResult was not a success");
        }

        public static void IsFailure<TToken, TResult>(IParseResult<TToken, TResult> result)
        {
            if (!(result is IFailureParseResult<TToken, TResult>))
                throw new AssertFailedException("ParseResultAssert.IsFailure failed: ParseResult was not a failure");
        }

        public static void AreEqual<TToken, TResult>(TResult expected, IParseResult<TToken, TResult> result)
        {
            var successfulParseResult = result as ISuccessfulParseResult<TToken, TResult>;
            if (successfulParseResult == null)
                throw new AssertFailedException("ParseResultAssert.AreEqual failed: ParseResult was not a success");
            Assert.AreEqual(expected, successfulParseResult.Result);
        }
    }
}