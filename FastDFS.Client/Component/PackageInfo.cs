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
    /// <summary>
    /// 传输包信息
    /// </summary>
    public class PackageInfo
    {
        private byte[] _body;

        /// <summary>
        /// 得到或者设置传输包体
        /// </summary>
        /// <value>The body.</value>
        public byte[] Body
        {
            get { return _body; }
            set { _body = value; }
        }

        private byte _errorNo;

        /// <summary>
        /// 得到或者设置错误号
        /// </summary>
        /// <value>The error no.</value>
        public byte ErrorNo
        {
            get { return _errorNo; }
            set { _errorNo = value; }
        }

        /// <summary>
        /// 初始化 <see cref="PackageInfo"/> 对象.
        /// </summary>
        /// <param name="errorNo">错误号.</param>
        /// <param name="body">传输包体.</param>
        public PackageInfo(byte errorNo, byte[] body)
        {
            _errorNo = errorNo;
            _body = body;
        }
    }
}