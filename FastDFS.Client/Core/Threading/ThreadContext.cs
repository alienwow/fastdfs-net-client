/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System.Collections;

namespace FastDFS.Client.Core.Threading
{
    public sealed class ThreadContext
    {
        private static Hashtable _data = Hashtable.Synchronized(new Hashtable());

        public static object GetData(string name)
        {
            return _data[name];
        }
        public static void SetData(string name, object value)
        {
            _data.Add(name, value);
        }
        public static void FreeNamedDataSlot(string name)
        {
            _data.Remove(name);
        }
        public static void FreeNamedDataSlot()
        {
            _data.Clear();
        }
    }
}