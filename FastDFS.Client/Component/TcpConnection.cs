/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System.Net;
using System.Net.Sockets;

namespace FastDFS.Client.Component
{
    public class TcpConnection : TcpClient
    {
        private string _ipAddress = string.Empty;
        private int _port;
        private int _index;
        private string _batchId = string.Empty;

        /// <summary>
        /// 生成对象批次Id
        /// </summary>
        internal string BatchId
        {
            set
            {
                _batchId = value;
            }
            get
            {
                return _batchId;
            }
        }

        public int Index
        {
            get { return _index; }
            set { _index = value; }
        }

        /// <summary>
        /// 得到或者设置连接服务器的IP地址
        /// </summary>
        /// <value>The ip address.</value>
        public string IpAddress
        {
            get
            {
                return _ipAddress;
            }
            internal set
            {
                _ipAddress = value;
            }
        }

        /// <summary>
        /// 得到或者设置连接服务器的端口
        /// </summary>
        /// <value>The port.</value>
        public int Port
        {
            get
            {
                return _port;
            }
            internal set
            {
                _port = value;
            }
        }

        /// <summary>
        /// 连接到服务器
        /// </summary>
        public void Connect()
        {
            Connect(IPAddress.Parse(IpAddress), Port);
        }

        private bool _isFromPool = false;
        internal bool IsFromPool
        {
            get
            {
                return _isFromPool;
            }
            set
            {
                _isFromPool = value;
            }
        }
        

        /// <summary>
        /// 释放此 <see cref="T:System.Net.Sockets.TcpClient"/> 实例，而不关闭基础连接。
        /// </summary>
        /// <param name="isTracker">if set to <c>true</c> [is tracker].</param>
        /// <param name="isStorage">if set to <c>true</c> [is storage].</param>
        public void Close(bool isTracker,bool isStorage)
        {
            if (!_isFromPool)
            {
                byte[] headerBuffer = Util.PackHeader(Protocol.FDFS_PROTO_CMD_QUIT, 0, 0);
                GetStream().Write(headerBuffer, 0, headerBuffer.Length);
                GetStream().Close();
                Close();
            }
            TcpConnectionPoolManager.GetPool(_ipAddress, _port, isTracker,isStorage).ReturnObject(this);
        }
    }
}
