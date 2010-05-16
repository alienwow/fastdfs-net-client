/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Reflection;
using System.Text;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Component
{
    public class TrackerClient
    {
        private static int _trackerServerIndex = 0;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private static IDictionary<string, int> sort = new Dictionary<string, int>();
        /// <summary>
        /// 得到tracker链接
        /// </summary>
        /// <returns></returns>
        public static TcpConnection GetTrackerConnection(string groupName)
        {
            bool isGetTcpConnection = true;
            do
            {
                IPEndPoint tracker;
                if (string.IsNullOrEmpty(groupName))//没有限制组的时候，负载均衡
                {
                    _trackerServerIndex++;
                    if (_trackerServerIndex >= TcpConnectionPoolManager.TrackerServers.Length)
                        _trackerServerIndex = 0;
                    tracker = TcpConnectionPoolManager.TrackerServers[_trackerServerIndex];
                }
                else
                {
                    IList<IPEndPoint> list = TcpConnectionPoolManager.GroupServer[groupName];
                    int index = sort.ContainsKey(groupName) ? sort[groupName] + 1 : 0;
                    if (index >= list.Count) index = 0;
                    tracker = list[index];
                    sort[groupName] = index;
                }

                IPAddress ip = tracker.Address;
                int port = tracker.Port;
                IObjectPool<TcpConnection> pool = TcpConnectionPoolManager.GetPool(ip.ToString(), port, true, false);
                try
                {
                    if (null != _logger)
                        _logger.InfoFormat("Tracker可用连接数：{0}", pool.NumIdle);
                    TcpConnection tcp = pool.GetObject(ip.ToString(), port);
                    if (null != tcp && tcp.Connected) return tcp;
                    isGetTcpConnection = false;
                }
                catch (Exception exc)
                {
                    if (null != _logger)
                        _logger.ErrorFormat("连接追踪器服务器时发生异常，异常信息为：{0}", exc.Message);
                    isGetTcpConnection = false;
                }
            } while (!isGetTcpConnection);
            return null;
        }

        /// <summary>
        /// 得到存储服务器
        /// </summary>
        /// <param name="groupName">存储的文件组名.</param>
        /// <returns></returns>
        public static StorageServerInfo GetStoreStorage(string groupName)
        {
            TcpConnection trackerConnection = GetTrackerConnection(groupName);
            if (null != _logger)
                _logger.InfoFormat("连接Tracker服务器的IP是：{0},端口是{1}", trackerConnection.IpAddress, trackerConnection.Port);
            Stream stream = trackerConnection.GetStream();

            if (null == trackerConnection)
            {
                trackerConnection = GetTrackerConnection(groupName);
                if (trackerConnection == null) return null;
            }

            try
            {
                byte cmd;
                int length;
                if (string.IsNullOrEmpty(groupName))
                {
                    cmd = Protocol.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP;
                    length = 0;
                }
                else
                {
                    cmd = Protocol.TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP;
                    length = Protocol.FDFS_GROUP_NAME_MAX_LEN;
                }

                byte[] headerBuffer = Util.PackHeader(cmd, length, 0);
                stream.Write(headerBuffer, 0, headerBuffer.Length);

                if (!string.IsNullOrEmpty(groupName))
                {
                    byte[] buffer = Encoding.GetEncoding(FastDFSService.Charset).GetBytes(groupName);
                    byte[] groupNameBuffer = new byte[Protocol.FDFS_GROUP_NAME_MAX_LEN];

                    int group_len = buffer.Length <= Protocol.FDFS_GROUP_NAME_MAX_LEN
                                        ? buffer.Length
                                        : Protocol.FDFS_GROUP_NAME_MAX_LEN;
                    Util.InitializeBuffer(groupNameBuffer, 0);
                    Array.Copy(buffer, 0, groupNameBuffer, 0, group_len);
                    stream.Write(groupNameBuffer, 0, groupNameBuffer.Length);
                }

                PackageInfo pkgInfo = Util.RecvPackage(trackerConnection.GetStream(),
                                                                  Protocol.TRACKER_PROTO_CMD_SERVICE_RESP,
                                                                  Protocol.TRACKER_QUERY_STORAGE_STORE_BODY_LEN);
                if (pkgInfo.ErrorNo != 0) return null;

                string ipAddress =
                    new String(Util.ToCharArray(pkgInfo.Body), Protocol.FDFS_GROUP_NAME_MAX_LEN, Protocol.FDFS_IPADDR_SIZE - 1).Trim();

                if (ipAddress.EndsWith("\0\0")) ipAddress = ipAddress.Remove(ipAddress.Length - 2);
                if (ipAddress.EndsWith("\0")) ipAddress = ipAddress.Remove(ipAddress.Length - 1);

                int port =
                    (int)Util.BufferToLong(pkgInfo.Body, Protocol.FDFS_GROUP_NAME_MAX_LEN + Protocol.FDFS_IPADDR_SIZE - 1);
                byte body = pkgInfo.Body[Protocol.TRACKER_QUERY_STORAGE_STORE_BODY_LEN - 1];

                return new StorageServerInfo(ipAddress.Trim(), port, body);
            }
            finally
            {
                trackerConnection.Close(true, false);
            }
        }
    }
}
