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
using System.Net;
using System.Reflection;
using System.Xml;
using FastDFS.Client.Core;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Core.Pool.Support;
using log4net;
using ThreadContext = FastDFS.Client.Core.Threading.ThreadContext;

namespace FastDFS.Client.Component
{
    internal sealed class TcpConnectionPoolManager
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        private const string AddressConfigItemName = "Address";
        private const string PoolSizeConfigItemName = "PoolSize";
        private const string PortCoutConfigItemName = "Port";
        private const string GroupNameCoutConfigItemName = "GroupName";


        private static IDictionary<string, IList<IPEndPoint>> _groupServer;
        internal static IDictionary<string, IList<IPEndPoint>> GroupServer
        {
            get
            {
                return _groupServer;
            }
        }


        private static IPEndPoint[] _trackerServers;
        internal static IPEndPoint[] TrackerServers
        {
            get { return _trackerServers; }
        }

        private static IPEndPoint[] _storageServers;

        internal static IPEndPoint[] _StorageServers
        {
            get { return _storageServers; }
        }


        /// <summary>
        ///创建tracker连接池.
        /// </summary>
        /// <param name="nodes">tracker配置项.</param>
        internal static void CreateTrackerServerPool(XmlNodeList nodes)
        {
            CreatePool(nodes, out _trackerServers, true, false);
        }

        /// <summary>
        /// 创建storage连接池
        /// </summary>
        /// <param name="nodes">storage配置项.</param>
        internal static void CreateStorageServerPool(XmlNodeList nodes)
        {
            CreatePool(nodes, out _storageServers, false, true);
        }

        /// <summary>
        /// 创建连接池.
        /// </summary>
        /// <param name="nodes">连接池配置项.</param>
        /// <param name="servers"></param>
        /// <param name="isTrackerPool">如果设置为true，得到tracker的唯一key.</param>
        /// <param name="isStoragePool">如果设置为true，得到storage的唯一key.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool不能同时为true或者false
        /// </remarks>
        internal static void CreatePool(XmlNodeList nodes, out IPEndPoint[] servers, bool isTrackerPool, bool isStoragePool)
        {
            servers = new IPEndPoint[nodes.Count];
            object ipAddress;
            object port;
            object poolSize;
            object groupName;
            for (int i = 0; i < nodes.Count; i++)
            {
                if (!ConfigReader.TryGetAttributeValue(nodes[i], AddressConfigItemName, out ipAddress) ||
                    !ConfigReader.TryGetAttributeValue(nodes[i], PortCoutConfigItemName, out port))
                    continue;

                if (!ConfigReader.TryGetAttributeValue(nodes[i], PoolSizeConfigItemName, out poolSize))
                    poolSize = 50;

                servers[i] = new IPEndPoint(IPAddress.Parse(ipAddress.ToString()),
                                            Int32.Parse(port.ToString()));

                //分组只有对于tracker才有,后面功能扩展的时候加上去的
                if (isTrackerPool && ConfigReader.TryGetAttributeValue(nodes[i], GroupNameCoutConfigItemName, out groupName))
                {
                    if (null == _groupServer) _groupServer = new Dictionary<string, IList<IPEndPoint>>();
                    if (_groupServer.ContainsKey(groupName.ToString()))
                    {
                        _groupServer[groupName.ToString()].Add(servers[i]);//modify by seapeak.xu 2009-12-01
                                                                            //groupName写成了_groupServer。tostring
                    }
                    else
                    {
                        IList<IPEndPoint> list = new List<IPEndPoint>();
                        list.Add(servers[i]);
                        _groupServer.Add(groupName.ToString(), list);
                    }
                }

                IObjectPool<TcpConnection> pool =
                    new ObjectPool<TcpConnection>(new TcpConnectionFactory<TcpConnection>(),
                                                  Int32.Parse(poolSize.ToString()));
                ThreadContext.SetData(
                    GetPoolKey(ipAddress.ToString(), Int32.Parse(port.ToString()), isTrackerPool, isStoragePool),
                    pool);
            }
        }

        /// <summary>
        /// 得到连接池池的唯一key
        /// </summary>
        /// <param name="ipAddress">连接的IP地址.</param>
        /// <param name="port">连接对象的端口.</param>
        /// <param name="isTrackerPool">如果设置为true，得到tracker的唯一key.</param>
        /// <param name="isStoragePool">如果设置为true，得到storage的唯一key.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool不能同时为true或者false
        /// </remarks>
        /// <returns></returns>
        internal static string GetPoolKey(string ipAddress, int port, bool isTrackerPool, bool isStoragePool)
        {
            string message;
            if (!isTrackerPool && !isStoragePool)
            {
                message = "无法得到线程池的唯一key,原因是传入的tracker和storage标识符不明确,都为false!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            if (isTrackerPool && isStoragePool)
            {
                message = "无法得到线程池的唯一key,原因是传入的tracker和storage标识符不明确,都为true!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            if (isTrackerPool && !isStoragePool)
                return String.Format("TrackerPool:{0}:{1}", ipAddress, port);
            if (!isTrackerPool && isStoragePool)
                return String.Format("StoragePool:{0}:{1}", ipAddress, port);

            message = "无法得到线程池的唯一key,原因是传入的tracker和storage标识符不明确!";
            if (null != _logger)
                _logger.Error(message);
            throw new Exception(message);
        }

        /// <summary>
        /// 得到指定的连接池.
        /// </summary>
        /// <param name="ipAddress">需要得到连接对象的IP.</param>
        /// <param name="port">需要得到连接对象的端口.</param>
        /// <param name="isTrackerPool">如果设置为true，得到tracker的唯一key.</param>
        /// <param name="isStoragePool">如果设置为true，得到storage的唯一key.</param>
        /// <remarks>
        /// isTrackerPool,isStoragePool不能同时为true或者false
        /// </remarks>
        internal static IObjectPool<TcpConnection> GetPool(string ipAddress, int port, bool isTrackerPool,
                                                           bool isStoragePool)
        {
            object obj = ThreadContext.GetData(GetPoolKey(ipAddress, port, isTrackerPool, isStoragePool));
            string message = string.Empty;
            if (null == obj)
            {
                if (isTrackerPool) message = "无法从指定key的对象池中取得tracker对象!";
                if (isStoragePool) message = "无法从指定key的对象池中取得storage对象!";
                if (null != _logger)
                    _logger.Error(message);
                throw new Exception(message);
            }
            return (IObjectPool<TcpConnection>)obj;
        }

        /// <summary>
        /// 停止线程池.
        /// </summary>
        internal static void StopPool()
        {

            ThreadContext.FreeNamedDataSlot();
        }
    }
}