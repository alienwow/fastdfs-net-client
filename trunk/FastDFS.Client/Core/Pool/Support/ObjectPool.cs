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
using System.Reflection;
using System.Threading;
using FastDFS.Client.Component;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Core.Pool.Support
{
    /// <summary>
    ///  对象池
    /// </summary>
    public class ObjectPool<T> : IObjectPool<T> where T : TcpConnection, new()
    {
        private readonly IPoolableObjectFactory<T> _factory;
        private IList<T> _busy = new List<T>();
        private bool _closed;
        private IList<T> _free = new List<T>();
        private static object locker = new object();
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public ObjectPool(IPoolableObjectFactory<T> factory, int size)
        {
            if (null == factory)
            {
                if (null != _logger)
                    _logger.Error("创建对象池时发生异常，对象池化工厂不能为空");
                throw new ArgumentNullException("factory", "对象创建工厂不能为空！");
            }
            _factory = factory;
            InitItems(size);

            if (null != _logger)
                _logger.InfoFormat("对象池已经创建，对象池长度为：{0}", size);
        }

        #region IObjectPool Members

        /// <summary>
        /// Gets the object.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        public T GetObject(string ipAddress,int port)
        {
            return DoGetObject(ipAddress,port);
        }

        /// <summary>
        /// 将使用完毕的对象返回到对象池.
        /// </summary>
        public void ReturnObject(T target)
        {
            DoReturnObject(target);
        }

        /// <summary>
        /// 关闭对象池并释放池中所有的资源
        /// </summary>
        public void Close()
        {
            DoClose();
        }

        /// <summary>
        /// 得到当前对象池中正在使用的对象数. 
        /// </summary>
        public int NumActive
        {
            get { return _busy.Count; }
        }

        /// <summary>
        /// 得到当前对象池中可用的对象数
        /// </summary>
        public int NumIdle
        {
            get { return _free.Count; }
        }

        #endregion

        protected void InitItems(int initialInstances)
        {
            if (initialInstances <= 0)
            {
                if (null != _logger)
                    _logger.Error("实例化对象池项时发生异常：对象池长度不能为空");
                throw new ArgumentException("对象池长度不能为空！", "initialInstances");
            }
            for (int i = 0; i < initialInstances; ++i)
            {
                _free.Add(_factory.CreateObject());
            }
        }

        protected T DoGetObject(string ipAddress,int port)
        {
            bool isLock = false;
            try 
            {
                if(_closed)
                {
                    if (null != _logger)
                        _logger.Warn("从对象池中获取对象时发生异常：对象池已经关闭，无法取得对象，对象池自行创建一个短连接对象。");
                    return RescueObject(ipAddress, port);
                }
                if (!Monitor.TryEnter(locker, FastDFSService.MonitorTimeout))
                {
                    if (null != _logger)
                        _logger.Warn("对象池锁阻塞，无法取得对象，对象池自行创建一个短连接对象。");
                    return RescueObject(ipAddress, port);
                }
                isLock = true;
                while (_free.Count > 0)
                {
                    int i = _free.Count - 1;
                    T o = _free[i];
                    _free.RemoveAt(i);
                    _factory.ActivateObject(o,ipAddress,port);
                    if (!_factory.ValidateObject(o)) continue;

                    _busy.Add(o);
                    if (null != _logger)
                        _logger.InfoFormat("连接池状态：现在空闲对象长度为:{0},忙碌对象长度为{1}.", NumIdle, NumActive);
                    return o;
                }

                if (null != _logger)
                    _logger.InfoFormat("从对象池中获取对象时发生异常：对象池中没有可用对象!现在空闲对象长度为:{0},忙碌对象长度为{1}.", NumIdle, NumActive);
               return RescueObject(ipAddress,port);
            }
            catch(Exception exc)
            {
                if (null != _logger)
                    _logger.ErrorFormat("从对象池中获取对象时发生异常：{0}.",exc.Message);
                return RescueObject(ipAddress, port);
            }
            finally
            {
                try
                {
                    if(isLock)  Monitor.Exit(locker);
                }
                catch(Exception exc)
                {
                    if (isLock)
                    {
                        if (null != _logger)
                            _logger.ErrorFormat("对象池中释放对象锁时发生异常：{0}.", exc.Message);
                    }
                    else
                    {
                        if (null != _logger)
                            _logger.ErrorFormat("在未获取锁的对象上释放锁时发生异常：{0}.", exc.Message);
                    }
                }
            }
        }

        protected bool DoReturnObject(T target)
        {
            if (_closed)
            {
                _factory.DestroyObject(target);
                if (null != _logger) _logger.Info("连接池已经关闭，放回对象被释放！");
                return true;
            }
           if(null != target && FastDFSService.BatchId != target.BatchId)
           {
               _factory.DestroyObject(target);
               if (null != _logger) _logger.InfoFormat("此对象不属于该连接池，该连接的生成批次为{0},现生成对象的批次为{1}！", target.BatchId, FastDFSService.BatchId);
               return true;
           }
            lock (locker)
            {
                if (_busy.Contains(target))
                {
                    if (null != _logger) _logger.Info("连接对象使用完毕，准备放回连接池！");
                    _busy.Remove(target);
                    _factory.PassivateObject(target);
                    _free.Add(target);
                    if (null != _logger) _logger.Info("连接对象使用完毕，放回连接池！");
                    return true;
                }
                return false;
            }
        }

        /// <summary>
        /// 关闭对象池
        /// </summary>
        private void DoClose()
        {
            _free = new List<T>();
            _closed = true;
        }

        /// <summary>
        /// 强行创建一个对象
        /// </summary>
        /// <returns></returns>
        public T RescueObject(string ipAddress, int port)
        {
            return DoRescueObject(ipAddress, port);
        }

        protected T DoRescueObject(string ipAddress, int port)
        {
            T obj = _factory.CreateObject();
            _factory.ActivateObject(obj, ipAddress, port);
            obj.IsFromPool = false;
            return obj;
        }
    }
}