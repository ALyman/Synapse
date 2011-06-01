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
using System.IO;
using System.Linq;
using MbUnit.Framework;
using Synapse.Input;

namespace Synapse.Tests
{
    [TestFixture]
    public abstract class InputTestsBase
    {
        protected abstract IInput<char> CreateInputFrom(IEnumerable<char> source);

        [Test]
        public void When_I_have_an_empty_source_I_start_and_the_end_of_input()
        {
            var input = CreateInputFrom(new char[] {});
            Assert.IsTrue(input.EndOfInput);
        }

        [Test]
        public void When_I_read_the_only_token_I_recieve_it()
        {
            var input = CreateInputFrom(new[] {'a'});
            Assert.AreEqual('a', input.Current);
            input = input.MoveNext();
            Assert.IsTrue(input.EndOfInput);
        }

        [Test]
        public void When_I_read_the_last_token_I_no_longer_move_forward()
        {
            var input = CreateInputFrom(new[] {'a', 'b'});
            Assert.AreEqual('a', input.Current);
            input = input.MoveNext();
            Assert.IsFalse(input.EndOfInput);
            Assert.AreEqual('b', input.Current);
            Assert.IsFalse(input.EndOfInput);
            input = input.MoveNext();
            Assert.IsTrue(input.EndOfInput);
            Assert.AreEqual(input, input.MoveNext());
        }

        [Test]
        public void When_I_try_to_get_the_current_token_at_the_end_of_input_it_throws()
        {
            var input = CreateInputFrom(new char[] {});
            Assert.IsTrue(input.EndOfInput);

            try
            {
                var value = input.Current;
                Assert.Fail("Retrieving the current token did not throw the expected exception");
            }
            catch (EndOfStreamException)
            {
            }
        }
    }
}