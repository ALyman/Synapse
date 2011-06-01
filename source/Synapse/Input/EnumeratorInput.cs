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

namespace Synapse.Input
{
    /// <summary>
    /// An input that reads from an <see cref="IEnumerator{T}" />
    /// </summary>
    /// <typeparam name="TToken">The type of the token.</typeparam>
    public class EnumeratorInput<TToken> : IInput<TToken>
    {
        private readonly IEnumerator<TToken> enumerator;
        private bool endOfInput;
        private IInput<TToken> next;
        private bool read;
        private TToken token;

        /// <summary>
        /// Initializes a new instance of the <see cref="EnumeratorInput&lt;TToken&gt;"/> class.
        /// </summary>
        /// <param name="reader">The reader.</param>
        public EnumeratorInput(IEnumerator<TToken> reader)
        {
            this.enumerator = reader;
        }

        #region IInput<TToken> Members

        /// <summary>
        /// Gets the current token.
        /// </summary>
        public TToken Current
        {
            get
            {
                Read();

                if (this.endOfInput)
                    throw new EndOfStreamException();

                return this.token;
            }
        }

        /// <summary>
        /// Gets a value indicating whether the stream is at the end of input.
        /// </summary>
        /// <value>
        ///   <c>true</c> if at end of input; otherwise, <c>false</c>.
        /// </value>
        public bool EndOfInput
        {
            get
            {
                Read();
                return this.endOfInput;
            }
        }

        /// <summary>
        /// Moves to the next token.
        /// </summary>
        /// <returns>The new input that describes the next token.</returns>
        public IInput<TToken> MoveNext()
        {
            Read();
            if (this.endOfInput)
                return this;

            if (this.next == null)
                this.next = new EnumeratorInput<TToken>(this.enumerator);

            return this.next;
        }

        #endregion

        private void Read()
        {
            if (!this.read)
            {
                this.read = true;

                if (this.enumerator.MoveNext())
                    this.token = this.enumerator.Current;
                else
                    this.endOfInput = true;
            }
        }
    }
}