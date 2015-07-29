using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class INode
    {
        public System.IO.FileAttributes attribute;

        public int sizeByte;
        public List<Block> blocks;
        public int index;

        public DateTime creationTime;
        public DateTime lastAccessTime;
        public DateTime lastWriteTime;

        public INode(int index, System.IO.FileAttributes attribute)
        {
            this.index = index;
            this.attribute = attribute;
            sizeByte = 0;
            blocks = new List<Block>();

            creationTime = DateTime.Now;
            lastAccessTime = creationTime;
            lastWriteTime = creationTime;
        }
        
        /// <summary>
        /// Resize
        /// </summary>
        /// <param name="length"></param>
        public void SetEndOfFile(int length)
        {
            if (length < 0)
            {
                throw new ArgumentOutOfRangeException();
            }

            var blockId = (length - 1) / Disk.sizePerBlock;
            var deltaAllocateSize = blocks.Count - (blockId + 1);

            if (Disk.blockAllocated - deltaAllocateSize > Disk.blockCapacity)
            {
                throw new System.IO.IOException("磁盘空间已满");
            }
            
            if (blockId < 0)
            {
                blocks = new List<Block>();
            }
            else
            {
                if (blockId + 1 <= blocks.Count - 1)
                {
                    blocks.RemoveRange(blockId + 1, blocks.Count - blockId - 1);
                }
                // 创建新 block
                for (var i = blocks.Count; i <= blockId; ++i)
                {
                    blocks.Add(new Block());
                }
            }

            sizeByte = length;
            Disk.blockAllocated -= deltaAllocateSize;
        }

        public int Write(byte[] buffer, int offset)
        {
            // 写入内容超出实际分配空间的话，resize
            if (offset + buffer.Length >= sizeByte)
            {
                SetEndOfFile(offset + buffer.Length);
            }

            var bytesWritten = buffer.Length;
            var srcOffset = 0;

            //Console.WriteLine("Begin Write {0} bytes at offset {1}", buffer.Length, offset);

            while (srcOffset < buffer.Length)
            {
                var blockId = offset / Disk.sizePerBlock;
                var dstOffset = offset % Disk.sizePerBlock;
                var bytes = Math.Min(sizeByte - offset, Disk.sizePerBlock - dstOffset);

                if (bytes == 0)
                {
                    break;
                }

                //Console.WriteLine("Write Copied src from {0} to dest from {1} with length {2}, bufferSize = {3}", srcOffset, dstOffset, bytes, buffer.Length);
                Buffer.BlockCopy(buffer, srcOffset, blocks[blockId].data, dstOffset, bytes);

                offset += bytes;
                srcOffset += bytes;
            }

            return bytesWritten;
        }

        public int Read(ref byte[] buffer, int offset)
        {
            if (offset >= sizeByte)
            {
                return 0;
            }

            //Console.WriteLine("Begin Read max {0} bytes at offset {1}", buffer.Length, offset);

            var bytesToRead = Math.Min(sizeByte - offset, buffer.Length);
            var dstOffset = 0;
            
            while (offset < sizeByte)
            {
                var blockId = offset / Disk.sizePerBlock;
                var srcOffset = offset % Disk.sizePerBlock;
                var bytes = Math.Min(bytesToRead - dstOffset, Disk.sizePerBlock - srcOffset);

                if (bytes == 0)
                {
                    break;
                }

                //Console.WriteLine("Read Copied src from {0} to dest from {1} with length {2}", srcOffset, dstOffset, bytes);
                Buffer.BlockCopy(blocks[blockId].data, srcOffset, buffer, dstOffset, bytes);

                offset += bytes;
                dstOffset += bytes;
            }

            return bytesToRead;
        }

    }
}