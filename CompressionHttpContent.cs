using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.Net.Http;

namespace Catcher
{
    public abstract class CompressionHttpContent : HttpContent
    {
        /// <summary>
        /// The original content.
        /// </summary>
        private readonly HttpContent originalContent;

        /// <summary>
        /// The compressor.
        /// </summary>
        private readonly ICompressor compressor;

        /// <summary>
        /// Initializes a new instance of the <see cref="CompressionHttpContent"/> class.
        /// </summary>
        /// <param name="content">The original HttpContent object.</param>
        /// <param name="compressor">The compressor.</param>
        public CompressionHttpContent(HttpContent content, ICompressor compressor)
        {
            this.originalContent = content;
            this.compressor = compressor;

            this.SetContentHeaders();
        }

        /// <summary>
        /// Gets the <see cref="ICompressor"/>.
        /// </summary>
        /// <value>
        /// The compressor.
        /// </value>
        protected ICompressor Compressor
        {
            get
            {
                return this.compressor;
            }
        }

        /// <summary>
        /// Gets the original <see cref="HttpContent"/>.
        /// </summary>
        /// <value>
        /// The original <see cref="HttpContent"/>.
        /// </value>
        protected HttpContent OriginalContent
        {
            get
            {
                return this.originalContent;
            }
        }

        /// <summary>
        /// Determines whether the HTTP content has a valid length in bytes.
        /// </summary>
        /// <param name="length">The length in bytes of the HHTP content.</param>
        /// <returns>
        /// Returns <see cref="T:System.Boolean" />.true if <paramref name="length" /> is a valid length; otherwise, false.
        /// </returns>
        protected override bool TryComputeLength(out long length)
        {
            length = -1;
            return false;
        }

        /// <summary>
        /// The set content headers.
        /// </summary>
        private void SetContentHeaders()
        {
            //// copy headers from original content
            foreach (var header in this.originalContent.Headers)
            {
                Headers.TryAddWithoutValidation(header.Key, header.Value);
            }

            //// add the content encoding header
            Headers.ContentEncoding.Add(this.compressor.EncodingType);
        }
    }
}
