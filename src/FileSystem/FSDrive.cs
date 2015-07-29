using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.AccessControl;
using System.Text;
using DokanNet;

namespace FileSystem
{
    class FSDrive : IDokanOperations
    {
        public DokanError Cleanup(string fileName, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError CloseFile(string fileName, DokanFileInfo info)
        {
            //Console.WriteLine("CloseFile: {0}", fileName);

            // 可能打开文件时有 deleteOnClose 标记（Windows 8+）
            if (info.Context != null)
            {
                File f = (File)info.Context;
                if (f.flagDeleteOnClose)
                {
                    Directory dir = new Directory(Util.GetPathDirectory(f.path));
                    if (!dir.Exists())
                    {
                        return DokanError.ErrorSuccess;
                    }

                    String name = Util.GetPathFileName(f.path);

                    if (!dir.Contains(name))
                    {
                        return DokanError.ErrorSuccess;
                    }

                    dir.Delete(name);
                }
            }

            return DokanError.ErrorSuccess;
        }

        public DokanError CreateDirectory(string fileName, DokanFileInfo info)
        {
            //Console.WriteLine("CreateDirectory: {0}", fileName);

            if (fileName == "\\")
            {
                return DokanError.ErrorSuccess;
            }

            if (fileName.EndsWith("\\"))
            {
                fileName = fileName.Substring(0, fileName.Length - 1);
            }

            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }
            
            String name = Util.GetPathFileName(fileName);

            if (dir.Contains(name))
            {
                return DokanError.ErrorAlreadyExists;
            }

            dir.CreateDirectory(name);

            return DokanError.ErrorSuccess;
        }

        public DokanError CreateFile(string fileName, DokanNet.FileAccess access, FileShare share, FileMode mode, FileOptions options, FileAttributes attributes, DokanFileInfo info)
        {
            info.DeleteOnClose = (options & FileOptions.DeleteOnClose) != 0;
            //Console.WriteLine("CreateFile: {0}, mode = {1}", fileName, mode);
            
            if (fileName == "\\")
            {
                return DokanError.ErrorSuccess;
            }

            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            String name = Util.GetPathFileName(fileName);

            if (name.Length == 0)
            {
                return DokanError.ErrorInvalidName;
            }
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) > -1)
            {
                return DokanError.ErrorInvalidName;
            }
            
            // dokan API 要求在目标文件是目录时候，设置 info.IsDirectory = true
            if (dir.Contains(name) && (dir.GetItemInfo(name).attribute & FileAttributes.Directory) != 0)
            {
                info.IsDirectory = true;
                return DokanError.ErrorSuccess;
            }

            try
            {
                File f = new File(fileName, mode);
                f.flagDeleteOnClose = info.DeleteOnClose;
                info.Context = f;
            }
            catch (FileNotFoundException)
            {
                return DokanError.ErrorFileNotFound;
            }
            catch (IOException)
            {
                return DokanError.ErrorAlreadyExists;
            }
            catch (NotImplementedException)
            {
                return DokanError.ErrorAccessDenied;
            }
            catch (Exception)
            {
                return DokanError.ErrorError;
            }
            
            return DokanError.ErrorSuccess;
        }

        public DokanError DeleteDirectory(string fileName, DokanFileInfo info)
        {
            Directory currentDir = new Directory(fileName);
            if (!currentDir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            // Windows Explorer 将会处理递归删除部分，所以这里不需要处理
            if (currentDir.Count() > 0)
            {
                return DokanError.ErrorDirNotEmpty;
            }

            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            String name = Util.GetPathFileName(fileName);

            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }

            dir.Delete(name);
            return DokanError.ErrorSuccess;
        }

        public DokanError DeleteFile(string fileName, DokanFileInfo info)
        {
            //Console.WriteLine("DeleteFile: {0}", fileName);

            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            String name = Util.GetPathFileName(fileName);

            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }

            dir.Delete(name);
            return DokanError.ErrorSuccess;
        }

        public DokanError FindFiles(string fileName, out IList<FileInformation> files, DokanFileInfo info)
        {
            //Console.WriteLine("FindFiles: {0}", fileName);

            var ret = new List<FileInformation>();
            var dir = new Directory(fileName);

            if (!dir.Exists())
            {
                files = null;
                return DokanError.ErrorPathNotFound;
            }

            var list = dir.ListDirectory();
            foreach (var item in list)
            {
                ret.Add(item.GetFileInformation());
            }

            files = ret;
            return DokanError.ErrorSuccess;
        }

        public DokanError FlushFileBuffers(string fileName, DokanFileInfo info)
        {
            //Console.WriteLine("FlushFileBuffers: {0}", fileName);
            return DokanError.ErrorSuccess;
        }

        public DokanError GetDiskFreeSpace(out long freeBytesAvailable, out long totalBytes, out long totalFreeBytes, DokanFileInfo info)
        {
            totalBytes = Disk.blockCapacity * Disk.sizePerBlock;
            freeBytesAvailable = Disk.blockCapacity * Disk.sizePerBlock - Disk.blockAllocated * Disk.sizePerBlock;
            totalFreeBytes = freeBytesAvailable;
            return DokanError.ErrorSuccess;
        }

        public DokanError GetFileInformation(string fileName, out FileInformation fileInfo, DokanFileInfo info)
        {
            //Console.WriteLine("GetFileInformation: {0}, isDirectory = {1}", fileName, info.IsDirectory);

            if (fileName == "\\" || info.IsDirectory)
            {
                Directory dir = new Directory(fileName);
                if (!dir.Exists())
                {
                    //Console.WriteLine("GetFileInformation: PathNotFound");
                    fileInfo = new FileInformation();
                    return DokanError.ErrorPathNotFound;
                }

                String name;
                String path;

                if (fileName == "\\")
                {
                    name = "\\"; path = "\\";
                }
                else
                {
                    name = Util.GetPathFileName(fileName);
                    path = fileName;
                }

                fileInfo = new DirectoryItem(name, path, dir.GetSelfINodeIndex()).GetFileInformation();
                //Console.WriteLine("GetFileInformation: Success");
                return DokanError.ErrorSuccess;
            }

            {
                Directory dir = new Directory(Util.GetPathDirectory(fileName));

                if (!dir.Exists())
                {
                    fileInfo = new FileInformation();
                    return DokanError.ErrorPathNotFound;
                }

                String name = Util.GetPathFileName(fileName);

                if (!dir.Contains(name))
                {
                    fileInfo = new FileInformation();
                    return DokanError.ErrorPathNotFound;
                }

                fileInfo = dir.GetItemInfo(name).GetFileInformation();
                //Console.WriteLine("GetFileInformation: Success");
                return DokanError.ErrorSuccess;
            }
        }

        public DokanError GetFileSecurity(string fileName, out FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            security = null;
            return DokanError.ErrorAccessDenied;
        }

        public DokanError GetVolumeInformation(out string volumeLabel, out FileSystemFeatures features, out string fileSystemName, DokanFileInfo info)
        {
            volumeLabel = "虚拟文件系统";
            fileSystemName = "MyFS 1352920 冯夏令";
            features = FileSystemFeatures.CasePreservedNames;
            return DokanError.ErrorSuccess;
        }

        public DokanError LockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            // 不支持锁定文件
            return DokanError.ErrorSuccess;
        }

        public DokanError MoveFile(string oldName, string newName, bool replace, DokanFileInfo info)
        {
            //Console.WriteLine("MoveFile: {0} -> {1}, replace = {2}", oldName, newName, replace);

            var dirNameOld = Util.GetPathDirectory(oldName);
            var dirNameNew = Util.GetPathDirectory(newName);
            if (dirNameNew.IndexOf(dirNameOld) > -1 && dirNameNew != dirNameOld)
            {
                return DokanError.ErrorError;
            }

            var dirOld = new Directory(dirNameOld);
            var dirNew = new Directory(dirNameNew);

            if (!dirOld.Exists() || !dirNew.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            var nameOld = Util.GetPathFileName(oldName);
            var nameNew = Util.GetPathFileName(newName);

            if (dirNew.Contains(nameNew))
            {
                return DokanError.ErrorAlreadyExists;
            }

            var inode = dirOld.GetItemINodeIndex(nameOld);
            dirOld.Delete(nameOld);
            dirNew.AddItem(nameNew, inode);

            return DokanError.ErrorSuccess;
        }

        public DokanError OpenDirectory(string fileName, DokanFileInfo info)
        {
            if (fileName == "\\" || info.IsDirectory)
            {
                Directory dir = new Directory(fileName);
                if (!dir.Exists())
                {
                    return DokanError.ErrorFileNotFound;
                }
            }
            return DokanError.ErrorSuccess;
        }

        public DokanError ReadFile(string fileName, byte[] buffer, out int bytesRead, long offset, DokanFileInfo info)
        {
            // 可以直接使用打开文件时 info.Context 保存下来的对象(file descriptor)
            
            File f = (File)info.Context;
            int br = f.Read(ref buffer, (int)offset);
            bytesRead = br;
            
            return DokanError.ErrorSuccess;
        }

        public DokanError SetAllocationSize(string fileName, long length, DokanFileInfo info)
        {
            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            String name = Util.GetPathFileName(fileName);
            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }

            INode inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
            inode.SetEndOfFile((int)length);
            return DokanError.ErrorSuccess;
        }

        public DokanError SetEndOfFile(string fileName, long length, DokanFileInfo info)
        {
            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }

            String name = Util.GetPathFileName(fileName);
            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }

            INode inode = Disk.iNodes[dir.GetItemINodeIndex(name)];
            inode.SetEndOfFile((int)length);
            return DokanError.ErrorSuccess;
        }

        public DokanError SetFileAttributes(string fileName, FileAttributes attributes, DokanFileInfo info)
        {
            //Console.WriteLine("SetFileAttribute: {0}", fileName);

            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }
            String name = Util.GetPathFileName(fileName);
            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }
            dir.SetFileAttribute(name, attributes);

            return DokanError.ErrorSuccess;
        }

        public DokanError SetFileSecurity(string fileName, FileSystemSecurity security, AccessControlSections sections, DokanFileInfo info)
        {
            return DokanError.ErrorAccessDenied;
        }

        public DokanError SetFileTime(string fileName, DateTime? creationTime, DateTime? lastAccessTime, DateTime? lastWriteTime, DokanFileInfo info)
        {
            Directory dir = new Directory(Util.GetPathDirectory(fileName));
            if (!dir.Exists())
            {
                return DokanError.ErrorPathNotFound;
            }
            String name = Util.GetPathFileName(fileName);
            if (!dir.Contains(name))
            {
                return DokanError.ErrorFileNotFound;
            }
            dir.SetFileTime(name, creationTime, lastAccessTime, lastWriteTime);

            return DokanError.ErrorSuccess;
        }

        public DokanError UnlockFile(string fileName, long offset, long length, DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError Unmount(DokanFileInfo info)
        {
            return DokanError.ErrorSuccess;
        }

        public DokanError WriteFile(string fileName, byte[] buffer, out int bytesWritten, long offset, DokanFileInfo info)
        {
            File f = (File)info.Context;
            int bw = f.Write(buffer, (int)offset);
            bytesWritten = bw;
            
            return DokanError.ErrorSuccess;
        }
    }
}
