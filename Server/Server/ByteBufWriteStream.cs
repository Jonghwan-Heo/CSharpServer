using DotNetty.Buffers;
using System.IO;

namespace network
{
    public class ByteBufWriteStream : Stream
    {
        private IByteBuffer _dest;

        public ByteBufWriteStream(IByteBuffer dest)
        {
            _dest = dest;
        }

        public override void Flush()
        {
        }

        public override int Read(byte[] output, int offset, int count)
        {
            return -1;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (origin == SeekOrigin.Begin)
                _dest.SetWriterIndex((int)offset);
            else if (origin == SeekOrigin.Current)
                _dest.SetWriterIndex((int)(_dest.WriterIndex + offset));
            else if (origin == SeekOrigin.End)
                _dest.SetWriterIndex((int)(_dest.ReadableBytes + offset));
            return offset;
        }

        public override void SetLength(long value)
        {
        }

        public override void Write(byte[] input, int offset, int count)
        {
            _dest.WriteBytes(input, offset, count);
        }

        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length => _dest.ReadableBytes;

        public override long Position
        {
            get { return _dest.WriterIndex; }
            set { _dest.SetWriterIndex((int)value); }
        }
    }
}