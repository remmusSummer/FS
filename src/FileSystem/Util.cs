using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileSystem
{
    public class Util
    {
        public static String GetPathDirectory(String path)
        {
            var pos = path.LastIndexOf('\\');
            var ret = path.Substring(0, pos + 1);
            return ret;
        }

        public static String GetPathFileName(String path)
        {
            var pos = path.LastIndexOf('\\');
            var ret = path.Substring(pos + 1);
            return ret;
        }
    }
}
