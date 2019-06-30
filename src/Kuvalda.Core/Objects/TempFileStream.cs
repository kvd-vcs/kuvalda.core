using System;
using System.IO;
using System.IO.Abstractions;
using Serilog;

namespace Kuvalda.Core
{
    public class TempFileStream : Stream
    {
        private readonly Stream _stream;
        private readonly string _path;
        private readonly IFileSystem _fs;
        private readonly ILogger _logger;

        public TempFileStream(Stream baseStream, string path, IFileSystem fs, ILogger logger)
        {
            _stream = baseStream ?? throw new ArgumentNullException(nameof(baseStream));
            _fs = fs ?? throw new ArgumentNullException(nameof(fs));
            _logger = logger;
            _path = !string.IsNullOrEmpty(path)
                ? path
                : throw new ArgumentException($"argument {nameof(path)} is empty or null");
            
            _logger?.Debug("Created temp file {path}", _path);
        }

        public override void Flush() 
            => _stream.Flush();

        public override int Read(byte[] buffer, int offset, int count) 
            => _stream.Read(buffer, offset, count);

        public override long Seek(long offset, SeekOrigin origin)
            => _stream.Seek(offset, origin);

        public override void SetLength(long value)
            => _stream.SetLength(value);

        public override void Write(byte[] buffer, int offset, int count)
            => _stream.Write(buffer, offset, count);

        public override void Close()
        {
            _stream.Close();
            _fs.File.Delete(_path);
            _logger?.Debug("Delete temp file {path}", _path);
        }

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => _stream.CanSeek;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => _stream.Length;
        public override long Position
        {
            get => _stream.Position;
            set => _stream.Position = value;
        }
    }
}