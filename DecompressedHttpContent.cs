using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;

namespace Catcher
{
    public class DecompressedHttpContent : CompressionHttpContent
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DecompressedHttpContent"/> class.
        /// </summary>
        /// <param name="content">The original HttpContent object.</param>
        /// <param name="compressor">The compressor.</param>
        public DecompressedHttpContent(HttpContent content, ICompressor compressor) : base(content, compressor)
        {
        }

        /// <summary>
        /// Serialize the HTTP content to a stream as an asynchronous operation.
        /// </summary>
        /// <param name="stream">The target stream.</param>
        /// <param name="context">Information about the transport (channel binding token, for example). This parameter may be null.</param>
        /// <returns>
        /// Returns <see cref="T:System.Threading.Tasks.Task" />.The task object representing the asynchronous operation.
        /// </returns>
        protected override Task SerializeToStreamAsync(System.IO.Stream stream, System.Net.TransportContext context)
        {
            Stream compressionStream = this.Compressor.CreateDecompressionStream(this.OriginalContent.ReadAsStreamAsync().Result);

            return compressionStream.CopyToAsync(stream).ContinueWith(task =>
            {
                if (compressionStream != null)
                {
                    compressionStream.Dispose();
                }
            });
        }
    }
}
