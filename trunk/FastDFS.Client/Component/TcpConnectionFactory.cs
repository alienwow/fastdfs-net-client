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
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Component
{
    /// <summary>
    /// 连接池创建工厂
    /// </summary>
    public class TcpConnectionFactory<T> : IPoolableObjectFactory<T>
        where T : TcpConnection, new()
    {

        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        /// <summary>
        /// 创建对象
        /// </summary>
        public T CreateObject()
        {
            T obj = Activator.CreateInstance<T>();
            obj.IsFromPool = true;
            obj.BatchId = FastDFSService.BatchId;
            return obj;
        }

        /// <summary>
        /// 销毁对象.
        /// </summary>
        public void DestroyObject(T obj)
        {
            if (obj.Connected)
            {
                obj.GetStream().Close();
                obj.Close();
            }
            if (obj is IDisposable)
            {
                ((IDisposable)obj).Dispose();
            }
        }

        /// <summary>
        /// 检查并确保对象的安全
        /// </summary>
        public bool ValidateObject(T obj)
        {
            return obj.Connected;
        }

        /// <summary>
        /// 激活对象池中待用对象. 
        /// </summary>
        public void ActivateObject(T obj,string ipAdderess,int port)
        {
            try
            {
                if (obj.Connected) return;
                obj.IpAddress = ipAdderess;
                obj.Port = port;
                obj.ReceiveTimeout = FastDFSService.NetworkTimeout;
                obj.Connect(); 
            }
            catch(Exception exc)
            {
                //if (null != _logger)_logger.WarnFormat("连接池激活对象时发生异常,异常信息为:{0}",exc.Message);
            }
        }

        /// <summary>
        /// 卸载内存中正在使用的对象.
        /// </summary>
        public void PassivateObject(T obj)
        {
            //if (obj.Connected) obj.Close();
        }
    }
}