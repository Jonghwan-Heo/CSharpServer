using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using network;
using Server.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;

namespace Server
{
    public class MyProtobufEncoder : MessageToMessageEncoder<Packet>
    {
        private readonly byte _key;

        public MyProtobufEncoder(byte key)
        {
            _key = key;
        }

        protected override void Encode(IChannelHandlerContext context, Packet p, List<object> output)
        {
            Contract.Requires(context != null);
            Contract.Requires(p != null);
            Contract.Requires(output != null);

            IByteBuffer buffer = null;
            try
            {
                buffer = context.Allocator.HeapBuffer();

                using (var xor = new XorStream(new ByteBufWriteStream(buffer), _key))
                {
                    using var outputStream = new CodedOutputStream(xor);
                    p.WriteTo(outputStream);
                    outputStream.Flush();
                }

                output.Add(buffer);
                buffer = null;
            }
            catch (Exception exception)
            {
                throw new CodecException(exception);
            }
            finally
            {
                buffer?.Release();
            }
        }
    }
}