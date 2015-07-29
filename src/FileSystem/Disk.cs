using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class Disk
    {
        public const int sizePerBlock = 4096;

        public static int blockCapacity = 0;
        public static int inodeCapacity = 0;
        public static int blockAllocated = 0;
        public static int inodeAllocated = 0;

        public static INode[] iNodes;
        
        public static void Format(int size, int inodeCount)
        {
            int blockCount = size / sizePerBlock;

            blockCapacity = blockCount;
            blockAllocated = 0;
            inodeCapacity = inodeCount;
            inodeAllocated = 0;

            iNodes = new INode[inodeCapacity];

            CreateRoot();
        }
        
        public static int GetFreeInodeIndex()
        {
            for (int i = 0; i < iNodes.Length; i++)
            {
                if (iNodes[i] == null)
                {
                    return i;
                }
            }
            return -1;
        }

        public static INode AllocateNewINode(System.IO.FileAttributes attribute)
        {
            var index = GetFreeInodeIndex();
            if (index != -1)
            {
                iNodes[index] = new INode(index, attribute);
                return iNodes[index];
            }
            return null;
        }

        private static void CreateRoot()
        {
            var inode = AllocateNewINode(System.IO.FileAttributes.Directory);
            if (inode.index != 0)
            {
                throw new Exception("Unknown error!");
            }
            DirectoryBase.Initialize(inode);
        }
    }
}
