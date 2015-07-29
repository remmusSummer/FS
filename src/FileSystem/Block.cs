using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class Block
    {
        public byte[] data;

        public Block()
        {
            data = new byte[Disk.sizePerBlock];
        }
    }
}
