using DotNetty.Buffers;
using System;
using System.IO;

namespace Server
{
    public class ByteBufReadStream : Stream
    {
        private IByteBuffer _src;

        public ByteBufReadStream(IByteBuffer src)
        {
            _src = src;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] output, int offset, int count)
        {
            int remaining = Math.Max(0, _src.ReadableBytes - _src.ReaderIndex);
            if (count >= remaining)
                count = remaining;

            _src.ReadBytes(output, offset, remaining);
            return remaining;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                _src.SetReaderIndex((int)offset);
            else if (origin == SeekOrigin.Current)
                _src.SetReaderIndex((int)(_src.ReaderIndex + offset));
            else if (origin == SeekOrigin.End)
                _src.SetReaderIndex((int)(_src.ReadableBytes + offset));
            return offset;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] input, int offset, int count)
        {
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _src.ReadableBytes;

        public override long Position
        {
            get { return _src.ReaderIndex; }
            set { _src.SetReaderIndex((int)value); }
        }
    }
}