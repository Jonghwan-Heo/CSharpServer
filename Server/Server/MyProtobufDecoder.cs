using DotNetty.Buffers;
using DotNetty.Codecs;
using DotNetty.Transport.Channels;
using Google.Protobuf;
using network;
using Server.Core;
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;

namespace Server
{
    public class MyProtobufDecoder : MessageToMessageDecoder<IByteBuffer>
    {
        private readonly byte _key;

        public MyProtobufDecoder(byte key)
        {
            _key = key;
        }

        protected override void Decode(IChannelHandlerContext context, IByteBuffer message, List<object> output)
        {
            Contract.Requires(context != null);
            Contract.Requires(message != null);
            Contract.Requires(output != null);

            int length = message.ReadableBytes;
            if (length <= 0)
                return;

            Stream inputStream = null;
            try
            {
                var obj = new Packet();
                if (message.IoBufferCount == 1)
                {
                    ArraySegment<byte> bytes = message.GetIoBuffer(message.ReaderIndex, length);

                    using var ms = new MemoryStream(bytes.Array, message.ArrayOffset, length);
                    using var xor = new XorStream(ms, _key);
                    using var codeInputStream = new CodedInputStream(xor);
                    obj.MergeFrom(xor);
                }
                else
                {
                    using var ms = new ByteBufReadStream(message);
                    using var xor = new XorStream(ms, _key);
                    using var codeInputStream = new CodedInputStream(xor);
                    obj.MergeFrom(xor);
                }

                if (obj != null)
                    output.Add(obj);
            }
            catch (Exception exception)
            {
                throw new CodecException(exception);
            }
            finally
            {
                inputStream?.Dispose();
            }
        }
    }
}