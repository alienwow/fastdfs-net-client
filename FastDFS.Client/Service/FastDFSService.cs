/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/
using System;
using System.Reflection;
using System.Xml;
using FastDFS.Client.Component;
using FastDFS.Client.Core;
using log4net;

namespace FastDFS.Client.Service
{
    /// <summary>
    /// FastDFS客户端服务
    /// </summary>
    public sealed class FastDFSService
    {
        private const string CharsetConfigItemName = "Charset";
        private const string DefaultConfigFilePath = @"config\FastDFS.config";
        private const string NetworkTimeoutConfigItemName = "NetworkTimeout";
        private const string StorageServerConfigItemName = "StorageServer";
        private const string TrackerServerConfigItemName = "TrackerServer";
        private const string MonitorTimeoutConfigItemName = "MonitorTimeout";
        private static int _monitorTimeout = 100;
        private static string _charset = "ISO8859-1";
        private static int _networkTimeout = 30;
        private static string _batchId = string.Empty;
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        /// <summary>
        /// 生成批次
        /// </summary>
        internal static string BatchId
        {
            get
            {
                return _batchId;
            }
        }

        /// <summary>
        /// 得到或者设置连接超时.
        /// </summary>
        /// <value>The network timeout.</value>
        public static int NetworkTimeout
        {
            get { return _networkTimeout; }
            set { _networkTimeout = value; }
        }

        /// <summary>
        /// 得到或者设置文件编码.
        /// </summary>
        /// <value>The charset.</value>
        public static string Charset
        {
            get { return _charset; }
            set { _charset = value; }
        }

        public static int MonitorTimeout
        {
            get { return _monitorTimeout; }
            set { _monitorTimeout = value; }
        }

        /// <summary>
        /// 使用默认路径下的配置文件启动FastDFS客户端服务.
        /// </summary>
        /// <remarks>
        /// 默认配置文件为"config\FastDFS.config".
        /// </remarks>
        public static void Start()
        {
            if (null != _logger)
                _logger.Info("FastDFS使用默认配置文件启动，默认配置文件路径为config/FastDFS.config!");
            Start(DefaultConfigFilePath);
        }

        /// <summary>
        /// 使用指定配置文件启动FastDFS客户端服务.
        /// </summary>
        /// <param name="configFile">FastDFS配置文件.</param>
        public static void Start(string configFile)
        {
            XmlDocument doc = ConfigReader.LoadXml(configFile);
            object network_timeout;
            if (ConfigReader.TryGetNodeValue(doc, NetworkTimeoutConfigItemName, out network_timeout))
                _networkTimeout = int.Parse(network_timeout.ToString());
            object charset;
            if (ConfigReader.TryGetNodeValue(doc, CharsetConfigItemName, out charset))
                _charset = charset.ToString();
            object monitorTimeout;
            if (ConfigReader.TryGetNodeValue(doc, MonitorTimeoutConfigItemName, out monitorTimeout))
                _monitorTimeout = int.Parse(network_timeout.ToString());
            _batchId = DateTime.Now.Ticks.ToString();

            if (null != _logger)
                _logger.InfoFormat("停止FastDFS客户端服务，当前的生成批次为{0}!", _batchId);
            TcpConnectionPoolManager.CreateTrackerServerPool(ConfigReader.Analyze(doc, TrackerServerConfigItemName)); //创建tracker连接池
            TcpConnectionPoolManager.CreateStorageServerPool(ConfigReader.Analyze(doc, StorageServerConfigItemName)); //创建storage连接池
            if (null != _logger)
                _logger.Info("FastDFS客户端服务启动成功!");
        }


        /// <summary>
        /// 停止连接池.
        /// </summary>
        public static void Stop()
        {
            TcpConnectionPoolManager.StopPool();
            if (null != _logger)
                _logger.InfoFormat("停止FastDFS客户端服务，当前的生成批次为{0}!",_batchId);
            _batchId = string.Empty;
            if (null != _logger)
                _logger.Info("停止FastDFS客户端服务，并清除连接池!");
        }

        /// <summary>
        /// 使用默认配置文件重启FastDFS
        /// </summary>
        public static void Reset()
        {
            Reset(DefaultConfigFilePath);
        }
        /// <summary>
        /// 指定配置文件重启FastDFS
        /// </summary>
        /// <param name="config">FastDFS的配置文件</param>
        public static void Reset(string config)
        {
            if (null != _logger)
                _logger.Info("开始重启连接池!");
            Stop();
            Start(config);
            if (null != _logger)
                _logger.Info("重启连接池完毕!");
        }
    }
}