using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class Directory 
    {
        private int inodeIndex;
        private string path;
        private Boolean exist;

        public Directory(string path)
        {
            if (!path.EndsWith("\\"))
            {
                path += "\\";
            }
            this.path = path;

            DirectoryBase root = new DirectoryBase(Disk.iNodes[0]);
            root.Load();

            var pathCom = path.Split('\\');
            var node = root;

            exist = true;

            for (var i = 1; i < pathCom.Length - 1; ++i)
            {
                if (node.Contains(pathCom[i]) && (Disk.iNodes[node.Find(pathCom[i])].attribute & System.IO.FileAttributes.Directory) != 0)
                {
                    node = new DirectoryBase(Disk.iNodes[node.Find(pathCom[i])]);
                    node.Load();
                }
                else
                {
                    exist = false;
                    break;
                }
            }
            
            inodeIndex = node.inode.index;
        }

        public void CreateDirectory(String name)
        {
            if (!exist)
            {
                throw new System.IO.DirectoryNotFoundException();
            }
            var inode = Disk.AllocateNewINode(System.IO.FileAttributes.Directory);
            DirectoryBase.Initialize(inode);
            DirectoryBase.AddItem(Disk.iNodes[inodeIndex], name, inode.index);
        }

        public DirectoryItem GetItemInfo(String name)
        {
            var inodeIndex = GetItemINodeIndex(name);
            return new DirectoryItem(name, path + name, inodeIndex);
        }

        public List<DirectoryItem> ListDirectory()
        {
            if (!exist)
            {
                throw new System.IO.DirectoryNotFoundException();
            }
            var ret = new List<DirectoryItem>();
            var list = DirectoryBase.ListItems(Disk.iNodes[inodeIndex]);
            foreach (var pair in list)
            {
                var item = new DirectoryItem(pair.Key, this.path + pair.Key, pair.Value);
                ret.Add(item);
            }
            return ret;
        }

        public Boolean Contains(String name)
        {
            if (!exist)
            {
                throw new System.IO.DirectoryNotFoundException();
            }
            return DirectoryBase.Contains(Disk.iNodes[inodeIndex], name);
        }

        public void Delete(String name)
        {
            DirectoryBase.DeleteItem(Disk.iNodes[inodeIndex], name);
        }

        public int GetItemINodeIndex(String name)
        {
            if (!exist)
            {
                throw new System.IO.DirectoryNotFoundException();
            }
            return DirectoryBase.GetItem(Disk.iNodes[inodeIndex], name);
        }

        public void AddItem(String name, int inode)
        {
            if (!exist)
            {
                throw new System.IO.DirectoryNotFoundException();
            }
            DirectoryBase.AddItem(Disk.iNodes[inodeIndex], name, inode);
        }

        public Boolean Exists()
        {
            return exist;
        }

        public int GetSelfINodeIndex()
        {
            return inodeIndex;
        }

        public int Count()
        {
            return DirectoryBase.Count(Disk.iNodes[inodeIndex]);
        }

        public void SetFileAttribute(String name, System.IO.FileAttributes attribute)
        {
            int i = DirectoryBase.GetItem(Disk.iNodes[inodeIndex], name);
            Boolean isDirectory = (Disk.iNodes[i].attribute & System.IO.FileAttributes.Directory) != 0;
            if (isDirectory)
            {
                attribute |= System.IO.FileAttributes.Directory;
            }
            Disk.iNodes[i].attribute = attribute;
        }

        public void SetFileTime(String name, DateTime? creationTime, DateTime? accessTime, DateTime? writeTime)
        {
            int i = DirectoryBase.GetItem(Disk.iNodes[inodeIndex], name);
            if (creationTime != null)
            {
                Disk.iNodes[i].creationTime = creationTime.Value;
            }
            if (accessTime != null)
            {
                Disk.iNodes[i].lastAccessTime = accessTime.Value;
            }
            if (writeTime != null)
            {
                Disk.iNodes[i].lastWriteTime = writeTime.Value;
            }
        }
    }
}
