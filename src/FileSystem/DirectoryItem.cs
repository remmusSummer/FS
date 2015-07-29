using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class DirectoryItem
    {
        public String name;
        public String path;
        public int size;
        public System.IO.FileAttributes attribute;
        public DateTime creationTime;
        public DateTime lastAccessTime;
        public DateTime lastWriteTime;

        public DirectoryItem(String name, String path, int inodeIndex)
        {
            INode inode = Disk.iNodes[inodeIndex];
            this.name = name;
            this.path = path;
            this.size = inode.sizeByte;
            this.attribute = inode.attribute;
            this.creationTime = inode.creationTime;
            this.lastAccessTime = inode.lastAccessTime;
            this.lastWriteTime = inode.lastWriteTime;
        }

        public DokanNet.FileInformation GetFileInformation()
        {
            return new DokanNet.FileInformation()
            {
                Attributes = this.attribute,
                CreationTime = this.creationTime,
                LastAccessTime = this.lastAccessTime,
                LastWriteTime = this.lastWriteTime,
                FileName = this.name,
                Length = this.size
            };
        }
    }
}
