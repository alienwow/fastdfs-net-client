/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

namespace FastDFS.Client.Core.Pool
{
    /// <summary>
    /// 对象池接口
    /// </summary>
    public interface IObjectPool<T>
        where T : new()
    {
        /// <summary>
        /// 得到对象.
        /// </summary>
        /// <param name="ipAddress">The ip address.</param>
        /// <param name="port">The port.</param>
        /// <returns></returns>
        T GetObject(string ipAddress, int port);

        /// <summary>
        /// 将使用完毕的对象返回到对象池.
        /// </summary>
        void ReturnObject(T target);

        /// <summary>
        /// 关闭对象池并释放池中所有的资源
        /// </summary>
        void Close();

        /// <summary>
        /// 得到当前对象池中正在使用的对象数. 
        /// </summary>
        int NumActive { get; }        
        
        /// <summary>
        /// 得到当前对象池中可用的对象数
        /// </summary>
        int NumIdle { get; }

        /// <summary>
        /// 强行创建一个对象
        /// </summary>
        /// <returns></returns>
        T RescueObject(string ipAddress,int port);
    }
}