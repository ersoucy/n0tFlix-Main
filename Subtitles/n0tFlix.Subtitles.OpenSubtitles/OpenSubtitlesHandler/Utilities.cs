/* This file is part of OpenSubtitles Handler
   A library that handle OpenSubtitles.org XML-RPC methods.

   Copyright © Ala Ibrahim Hadid 2013

   This program is free software: you can redistribute it and/or modify
   it under the terms of the GNU General Public License as published by
   the Free Software Foundation, either version 3 of the License, or
   (at your option) any later version.

   This program is distributed in the hope that it will be useful,
   but WITHOUT ANY WARRANTY; without even the implied warranty of
   MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
   GNU General Public License for more details.

   You should have received a copy of the GNU General Public License
   along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MediaBrowser.Common.Net;
using MediaBrowser.Model.Cryptography;

namespace OpenSubtitlesHandler
{
    /// <summary>
    /// Include helper methods. All member are statics.
    /// </summary>
    public static class Utilities
    {
        public static HttpClient HttpClient { get; set; }
        private const string XML_RPC_SERVER = "https://vip-api.opensubtitles.org/xml-rpc";
        //private const string XML_RPC_SERVER = "https://92.240.234.122/xml-rpc";
        private const string HostHeader = "vip-api.opensubtitles.org:443";

        /// <summary>
        /// Compute movie hash
        /// </summary>
        /// <returns>The hash as Hexadecimal string</returns>
        public static string ComputeHash(Stream stream)
        {
            byte[] hash = MovieHasher.ComputeMovieHash(stream);
            return MovieHasher.ToHexadecimal(hash);
        }

        /// <summary>
        /// Decompress data using GZip
        /// </summary>
        /// <param name="dataToDecompress">The stream that hold the data</param>
        /// <returns>Bytes array of decompressed data</returns>
        public static byte[] Decompress(Stream dataToDecompress)
        {
            using (var target = new MemoryStream())
            {
                using (var decompressionStream = new System.IO.Compression.GZipStream(dataToDecompress, System.IO.Compression.CompressionMode.Decompress))
                {
                    decompressionStream.CopyTo(target);
                }
                return target.ToArray();
            }
        }

        /// <summary>
        /// Compress data using GZip (the retunred buffer will be WITHOUT HEADER)
        /// </summary>
        /// <param name="dataToCompress">The stream that hold the data</param>
        /// <returns>Bytes array of compressed data WITHOUT HEADER bytes</returns>
        public static byte[] Compress(Stream dataToCompress)
        {
            /*using (var compressed = new MemoryStream())
            {
                using (var compressor = new System.IO.Compression.GZipStream(compressed,
                    System.IO.Compression.CompressionMode.Compress))
                {
                    dataToCompress.CopyTo(compressor);
                }
                // Get the compressed bytes only after closing the GZipStream
                return compressed.ToArray();
            }*/
            //using (var compressedOutput = new MemoryStream())
            //{
            //    using (var compressedStream = new ZlibStream(compressedOutput,
            //        Ionic.Zlib.CompressionMode.Compress,
            //        CompressionLevel.Default, false))
            //    {
            //        var buffer = new byte[4096];
            //        int byteCount;
            //        do
            //        {
            //            byteCount = dataToCompress.Read(buffer, 0, buffer.Length);

            //            if (byteCount > 0)
            //            {
            //                compressedStream.Write(buffer, 0, byteCount);
            //            }
            //        } while (byteCount > 0);
            //    }
            //    return compressedOutput.ToArray();
            //}

            throw new NotImplementedException();
        }

        /// <summary>
        /// Handle server response stream and decode it as given encoding string.
        /// </summary>
        /// <returns>The string of the stream after decode using given encoding</returns>
        public static string GetStreamString(Stream responseStream)
        {
            using (responseStream)
            {
                // Handle response, should be XML text.
                var data = new List<byte>();
                while (true)
                {
                    int r = responseStream.ReadByte();
                    if (r < 0)
                        break;
                    data.Add((byte)r);
                }
                var bytes = data.ToArray();
                return Encoding.ASCII.GetString(bytes, 0, bytes.Length);
            }
        }

        public static byte[] GetASCIIBytes(string text)
        {
            return Encoding.ASCII.GetBytes(text);
        }

        /// <summary>
        /// Send a request to the server
        /// </summary>
        /// <param name="request">The request buffer to send as bytes array.</param>
        /// <param name="userAgent">The user agent value.</param>
        /// <returns>Response of the server or stream of error message as string started with 'ERROR:' keyword.</returns>
        public static Stream SendRequest(byte[] request, string userAgent)
        {
            var result = SendRequestAsync(request, userAgent, CancellationToken.None).Result;
            return result.Item1;
        }

        public static async Task<(Stream, int?, HttpStatusCode)> SendRequestAsync(byte[] request, string userAgent, CancellationToken cancellationToken)
        {
            var requestMessage = new HttpRequestMessage(HttpMethod.Post, XML_RPC_SERVER);
            requestMessage.Content = new StringContent(Encoding.UTF8.GetString(request), Encoding.UTF8, "text/xml");
            requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue(userAgent));
            requestMessage.Headers.Host = HostHeader;

            if (string.IsNullOrEmpty(requestMessage.Headers.UserAgent.FirstOrDefault().Comment.ToString()))
            {
                requestMessage.Headers.UserAgent.Add(new ProductInfoHeaderValue("xmlrpc-epi-php/0.2 (PHP)"));
            }
            var result = await HttpClient.SendAsync(requestMessage).ConfigureAwait(false);

            IEnumerable<string> values;
            int? limit = null;
            if (result.Headers.TryGetValues("X-RateLimit-Remaining", out values))
            {
                int num;
                if(int.TryParse(values.FirstOrDefault(), out num))
                {
                    limit = num;
                }
            }

            return (result.Content.ReadAsStream(), limit, result.StatusCode);
        }
    }
}
