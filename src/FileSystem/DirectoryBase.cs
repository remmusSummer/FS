using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FileSystem
{
    public class DirectoryBase
    {
        public Dictionary<String, int> dir;
        public INode inode;

        public DirectoryBase(INode inode)
        {
            this.inode = inode;
            dir = new Dictionary<string,int>();
        }

        public void Save()
        {
            MemoryStream ms = new MemoryStream();
            var formatter = new BinaryFormatter();
            formatter.Serialize(ms, dir);
            var byteArray = ms.ToArray();
            inode.SetEndOfFile(byteArray.Length);
            inode.Write(byteArray, 0);
        }

        public void Load()
        {
            byte[] dataArr = new byte[inode.sizeByte];
            inode.Read(ref dataArr, 0);
            MemoryStream ms = new MemoryStream(dataArr);
            var formatter = new BinaryFormatter();
            dir = (Dictionary<String, int>)formatter.Deserialize(ms);
        }

        public List<KeyValuePair<String,int>> List()
        {    
            return dir.ToList();
        }

        public Boolean Contains(String name)
        {
            return dir.ContainsKey(name);
        }

        public int Find(String name)
        {
            return dir[name];
        }

        /// <summary>
        /// 将 inode 初始化为一个 Directory
        /// </summary>
        /// <param name="inode"></param>
        public static void Initialize(INode inode)
        {
            var dir = new DirectoryBase(inode);
            dir.Save();
        }

        public static void AddItem(INode inode, String itemName, int itemINodeIndex)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            dir.dir[itemName] = itemINodeIndex;
            dir.Save();
        }

        public static void DeleteItem(INode inode, String itemName)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            if (!dir.Contains(itemName))
            {
                throw new System.IO.FileNotFoundException();
            }
            dir.dir.Remove(itemName);
            dir.Save();
        }

        public static void RenameItem(INode inode, String itemOldName, String itemNewName)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            if (!dir.Contains(itemOldName))
            {
                throw new System.IO.FileNotFoundException();
            }
            if (dir.Contains(itemNewName))
            {
                throw new System.IO.IOException();
            }
            var inodeIndex = dir.dir[itemOldName];
            dir.dir.Remove(itemOldName);
            dir.dir[itemNewName] = inodeIndex;
        }

        public static int GetItem(INode inode, String itemName)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            return dir.Find(itemName);
        }

        public static Boolean Contains(INode inode, String name)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            return dir.Contains(name);
        }

        public static List<KeyValuePair<String, int>> ListItems(INode inode)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            return dir.List();
        }

        public static int Count(INode inode)
        {
            var dir = new DirectoryBase(inode);
            dir.Load();
            return dir.dir.Count;
        }
    }
}
