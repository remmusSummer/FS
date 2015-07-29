using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class File
    {
        private INode inode;
        public String path;
        public Boolean flagDeleteOnClose = false;
        
        public File(string path, System.IO.FileMode fileMode)
        {
            this.path = path;
            var dir = new Directory(Util.GetPathDirectory(path));
            var name = Util.GetPathFileName(path);
            
            switch (fileMode)
            {
                case System.IO.FileMode.Append:
                    // 不支持
                    throw new NotImplementedException();
                    break;
                case System.IO.FileMode.Create:
                    if (dir.Contains(name))
                    {
                        this.inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
                        this.inode.SetEndOfFile(0);
                    }
                    else
                    {
                        this.inode = Disk.AllocateNewINode(System.IO.FileAttributes.Normal);
                        dir.AddItem(name, inode.index);
                    }
                    break;
                case System.IO.FileMode.CreateNew:
                    //Console.WriteLine("*** CreateNew {0}", name);
                    if (dir.Contains(name))
                    {
                        throw new System.IO.IOException();
                    }
                    this.inode = Disk.AllocateNewINode(System.IO.FileAttributes.Normal);
                    dir.AddItem(name, inode.index);
                    break;
                case System.IO.FileMode.Open:
                    //Console.WriteLine("*** Open {0}", name);
                    if (!dir.Contains(name))
                    {
                        throw new System.IO.FileNotFoundException();
                    }
                    this.inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
                    break;
                case System.IO.FileMode.OpenOrCreate:
                    if (!dir.Contains(name))
                    {
                        this.inode = Disk.AllocateNewINode(System.IO.FileAttributes.Normal);
                        dir.AddItem(name, inode.index);
                    }
                    else
                    {
                        this.inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
                    }
                    break;
                case System.IO.FileMode.Truncate:
                    if (!dir.Contains(name))
                    {
                        throw new System.IO.FileNotFoundException();
                    }
                    this.inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
                    this.inode.SetEndOfFile(0);
                    break;
            }
        }

        public void SetEndOfFile(int length)
        {
            inode.SetEndOfFile(length);
        }

        public int Write(byte[] buffer, int offset)
        {
            this.inode.lastWriteTime = DateTime.Now;
            return inode.Write(buffer, offset);
        }

        public int Read(ref byte[] buffer, int offset)
        {
            this.inode.lastAccessTime = DateTime.Now;
            return inode.Read(ref buffer, offset);
        }
    }
}
