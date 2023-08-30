using System.IO;

namespace Server.Core
{
    public class XorStream : Stream
    {
        private Stream _dest;
        private byte _key;

        public XorStream(Stream dest, byte key)
        {
            _dest = dest;
            _key = key;
        }

        public override void Flush()
        {
            _dest.Flush();
        }

        public override int Read(byte[] output, int offset, int count)
        {
            int read = _dest.Read(output, offset, count);

            for (int i = 0; i < read; ++i)
                output[i] = (byte)(output[i] ^ _key);

            return read;
        }

        public override long Seek(long offset, SeekOrigin origin)
        {
            return _dest.Seek(offset, origin);
        }

        public override void SetLength(long value)
        {
            _dest.SetLength(value);
        }

        public override void Write(byte[] input, int offset, int count)
        {
            for (int i = 0; i < count; ++i)
                input[offset + i] = (byte)(input[offset + i] ^ _key);

            _dest.Write(input, offset, count);
        }

        public override bool CanRead => _dest.CanRead;

        public override bool CanSeek => _dest.CanSeek;

        public override bool CanWrite => _dest.CanWrite;

        public override long Length => _dest.Length;

        public override long Position
        {
            get { return _dest.Position; }
            set { _dest.Position = value; }
        }
    }
}