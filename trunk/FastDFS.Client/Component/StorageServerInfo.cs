/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

namespace FastDFS.Client.Component
{
    public class StorageServerInfo
    {
        private string _ipAddress;
        private int _port;
        private int _storePathIndex;

        public virtual int StorePathIndex
        {
            get { return _storePathIndex; }
            set { _storePathIndex = value; }
        }

        public virtual string IpAddress
        {
            get { return _ipAddress; }
            set{_ipAddress = value;}
        }

        public virtual int Port
        {
            get { return _port; }
            set { _port = value; }
        }

        public StorageServerInfo(string ipAddress,int port, int storePathIndex)
        {
            _ipAddress = ipAddress;
            _port = port;
            _storePathIndex = storePathIndex;
        }

        public StorageServerInfo(string ipAddress,int port, byte storePath)
        {
            _ipAddress = ipAddress;
            _port = port;
            _storePathIndex = storePath < 0 ? 256 + storePath : storePath;
        }
    }
}