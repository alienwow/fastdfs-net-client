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
using System.Reflection;
using System.Text;
using FastDFS.Client.Core.Pool;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Client.Component
{

    public class StorageClient
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        public static TcpConnection GetStorageConnection(string groupName)
        {
            StorageServerInfo storageServerInfo = TrackerClient.GetStoreStorage(groupName);
            IObjectPool<TcpConnection> pool = TcpConnectionPoolManager.GetPool(storageServerInfo.IpAddress, storageServerInfo.Port, false, true);
            try
            {
                TcpConnection storageConnection = pool.GetObject(storageServerInfo.IpAddress, storageServerInfo.Port);
                storageConnection.Index = storageServerInfo.StorePathIndex;
                if (null != _logger)
                    _logger.InfoFormat("Storage可用连接数为:{0}", pool.NumIdle);
                return storageConnection;
            }
            catch (Exception exc)
            {
                if (null != _logger)
                    _logger.WarnFormat("连接Storage服务器时发生异常,异常信息为:{0}", exc.Message);
                throw;
            }
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="localFileName">Name of the local file.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="extension">The extension.</param>
        /// <param name="metadatas">The metadatas.</param>
        /// <returns></returns>
        protected static string[] DoUpload(string groupName, string localFileName, byte[] buffer,
                                                     string extension, NameValuePair[] metadatas)
        {
            TcpConnection storageConnection = GetStorageConnection(groupName);
            if (null != _logger)
                _logger.InfoFormat("Storage服务器的IP是:{0}.端口为{1}", storageConnection.IpAddress, storageConnection.Port);
            long totalBytes = 0;
            try
            {
                long length;
                FileStream stream;

                byte[] metadatasBuffer = metadatas == null
                                              ? new byte[0]
                                              : Encoding.GetEncoding(FastDFSService.Charset).GetBytes(Util.PackMetadata(metadatas));

                byte[] bufferSize = new byte[1 + 2 * Protocol.TRACKER_PROTO_PKG_LEN_SIZE];

                if (!string.IsNullOrEmpty(localFileName))
                {
                    FileInfo fileInfo = new FileInfo(localFileName);
                    length = fileInfo.Exists ? fileInfo.Length : 0;
                    stream = new FileStream(fileInfo.FullName, FileMode.Open, FileAccess.Read);
                }
                else
                {
                    length = buffer.Length;
                    stream = null;
                }

                byte[] extensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN];
                Util.InitializeBuffer(extensionBuffer, 0);
                if (!string.IsNullOrEmpty(extension))
                {
                    byte[] bs = Encoding.GetEncoding(FastDFSService.Charset).GetBytes(extension);
                    int ext_name_len = bs.Length;
                    if (ext_name_len > Protocol.FDFS_FILE_EXT_NAME_MAX_LEN) ext_name_len = Protocol.FDFS_FILE_EXT_NAME_MAX_LEN;
                    Array.Copy(bs, 0, extensionBuffer, 0, ext_name_len);
                }

                Util.InitializeBuffer(bufferSize, 0);
                bufferSize[0] = (byte)storageConnection.Index;
                byte[] hexBuffer = Util.LongToBuffer(metadatasBuffer.Length);
                Array.Copy(hexBuffer, 0, bufferSize, 1, hexBuffer.Length);
                hexBuffer = Util.LongToBuffer(length);
                Array.Copy(hexBuffer, 0, bufferSize, 1 + Protocol.TRACKER_PROTO_PKG_LEN_SIZE, hexBuffer.Length);

                byte[] header = Util.PackHeader(Protocol.STORAGE_PROTO_CMD_UPLOAD_FILE,
                                                1 + 2 * Protocol.TRACKER_PROTO_PKG_LEN_SIZE +
                                                Protocol.FDFS_FILE_EXT_NAME_MAX_LEN + metadatasBuffer.Length + length,
                                                0);
                Stream outStream = storageConnection.GetStream();
                outStream.Write(header, 0, header.Length);
                outStream.Write(bufferSize, 0, bufferSize.Length);
                outStream.Write(extensionBuffer, 0, extensionBuffer.Length);
                outStream.Write(metadatasBuffer, 0, metadatasBuffer.Length);
                if (stream != null)
                {
                    int readBytes;
                    byte[] buff = new byte[128 * 1024];

                    while ((readBytes = Util.ReadInput(stream, buff, 0, buff.Length)) >= 0)
                    {
                        if (readBytes == 0) continue;
                        outStream.Write(buff, 0, readBytes);
                        totalBytes += readBytes;
                    }
                }
                else
                {
                    outStream.Write(buffer, 0, buffer.Length);
                }

                PackageInfo pkgInfo = Util.RecvPackage(storageConnection.GetStream(), Protocol.STORAGE_PROTO_CMD_RESP, -1);
                if (pkgInfo.ErrorNo != 0) return null;

                if (pkgInfo.Body.Length <= Protocol.FDFS_GROUP_NAME_MAX_LEN)
                    throw new Exception(string.Format("_body length: {0} <= {1}",
                                                      pkgInfo.Body.Length, Protocol.FDFS_GROUP_NAME_MAX_LEN));

                char[] chars = Util.ToCharArray(pkgInfo.Body);
                string newGroupName = new string(chars, 0, Protocol.FDFS_GROUP_NAME_MAX_LEN).Trim();
                string remoteFileName = new string(chars, Protocol.FDFS_GROUP_NAME_MAX_LEN, pkgInfo.Body.Length - Protocol.FDFS_GROUP_NAME_MAX_LEN);
                string[] results = new string[]
                                       {
                                           newGroupName, remoteFileName
                                       };
                return results;
            }
            finally
            {
                storageConnection.Close(false, true);
            }
        }



        /// <summary>
        /// 批量上传文件.
        /// </summary>
        /// <param name="groupName">Name of the group.</param>
        /// <param name="filesCount">The files count.</param>
        /// <param name="localFileName">Name of the local file.</param>
        /// <param name="buffer">The buffer.</param>
        /// <param name="extension">The extension.</param>
        /// <returns></returns>
        protected static string[] DoBatchUpload(string groupName, IList<byte[]> filesBuffer, string[] filesExtension)
        {
            int filesCount = filesBuffer.Count;
            if (255 < filesCount)
            {
                if (null != _logger)
                {
                    _logger.ErrorFormat("批量上传文件的数量为:{0}，超出了限定批上传文件数（限定批上传文件数为255），请分多次上传.", filesCount);
                    throw new Exception("上传文件数超出批处理限制范围！");
                }
            }
            if (null != _logger)
            {
                _logger.InfoFormat("开始批量上传文件，批量上传文件的总数为:{0}.", filesCount);
            }
            TcpConnection storageConnection = GetStorageConnection(groupName);
            if (null != _logger)
                _logger.InfoFormat("Storage服务器的IP是:{0}.端口为{1}", storageConnection.IpAddress, storageConnection.Port);

            if (filesBuffer.Count != filesExtension.Length)
            {
                if (null != _logger)
                    _logger.ErrorFormat("上传文件数组与上传文件扩展名，上传文件数组长度为{0},文件扩展名数组长度为{1}。",
                        filesBuffer.Count, filesExtension.Length);
                throw new Exception("上传文件数不匹配。");
            }

            //构建扩展名传输流
            byte[] filesExtensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN * filesCount];
            byte[] fileExtensionBuffer = new byte[Protocol.FDFS_FILE_EXT_NAME_MAX_LEN];
            byte[] fileTempExtensionBuffer;
            for (int i = 0; i < filesExtension.Length; i++)
            {
                if (string.IsNullOrEmpty(filesExtension[i]))
                {
                    if (null != _logger)
                        _logger.Error("文件扩展名为空，终止文件上传。");
                    throw new Exception("未获得文件扩展名。");
                }
                fileTempExtensionBuffer = Encoding.GetEncoding(FastDFSService.Charset).GetBytes(filesExtension[i]);
                int fileExtBufferLength = fileTempExtensionBuffer.Length;
                if (fileExtBufferLength > Protocol.FDFS_FILE_EXT_NAME_MAX_LEN) fileExtBufferLength = Protocol.FDFS_FILE_EXT_NAME_MAX_LEN;

                //协议归整
                Array.Copy(fileTempExtensionBuffer, 0, fileExtensionBuffer, 0, fileExtBufferLength);
                //加入网络传输
                Array.Copy(fileExtensionBuffer, 0, filesExtensionBuffer, i * Protocol.FDFS_FILE_EXT_NAME_MAX_LEN, fileExtBufferLength);
            }

            //构建文件传输流
            long filesBufferLength = 0L;
            foreach (byte[] fileBuffer in filesBuffer)
            {
                filesBufferLength += fileBuffer.LongLength;
            }
          
            //构建文件数量字节流
            byte[] filesCountBuffer = Util.LongToBuffer(filesCount);

            //构建头部协议块
            // Protocol.TRACKER_PROTO_PKG_LEN_SIZE * (filesCount + 2) 各个文件的长度+扩展名字节流的长度+文件字节流总共的长度
            byte[] headerBuffer = new byte[1 + Protocol.TRACKER_PROTO_PKG_LEN_SIZE * (filesCount + 2)];
            headerBuffer[0] = (byte)storageConnection.Index;//第一位为storage的index
            byte[] temp; 
            //每8位表示每个文件的字节流长度
            for (int i = 0; i < filesBuffer.Count; i++)
            {
                temp = Util.LongToBuffer(filesBuffer[i].LongLength);
                Array.Copy(temp, 0, headerBuffer, 1 + i * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);
            }

            temp = Util.LongToBuffer(filesExtensionBuffer.LongLength);//扩展名总共的长度
            Array.Copy(temp, 0, headerBuffer, 1 + filesBuffer.Count * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);
            temp = Util.LongToBuffer(filesBufferLength);//文件字节流长度
            Array.Copy(temp, 0, headerBuffer, 1 + (filesBuffer.Count + 1) * Protocol.TRACKER_PROTO_PKG_LEN_SIZE, temp.LongLength);

            //构建协议传输流
            byte[] protocalBuffer = Util.PackHeader(Protocol.STORAGE_PROTO_CMD_Batch_UPLOAD,
                //长度构成：一位storage的index+每个文件的字节长度+文件扩展名的总长度+文件字节的总长度
                                               headerBuffer.Length +
                                               filesCountBuffer.Length
                                              + filesExtensionBuffer.Length + filesBufferLength,
                                               0);

            _logger.InfoFormat("上传字节数为：{0}", headerBuffer.Length +
                                               filesCountBuffer.Length
                                              + filesExtensionBuffer.Length + filesBufferLength);

            Stream outStream = storageConnection.GetStream();
            outStream.Write(protocalBuffer, 0, protocalBuffer.Length);
            outStream.Write(filesCountBuffer, 0, filesCountBuffer.Length);
            outStream.Write(headerBuffer, 0, headerBuffer.Length);
            outStream.Write(filesExtensionBuffer, 0, filesExtensionBuffer.Length);
            foreach (byte[] buffer in filesBuffer)
            {
                outStream.Write(buffer,0,buffer.Length);
            }

            Stream readStream;
            int fileNameBufferLength = Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 128;//文件名长度+文件名内容
            byte[] tempBuffer;
            int tempReadSize = 0;
            int fileNameSize;
            byte[] tempFileNameBytes;
            char[] chars;
            int error;
            string[] filesName = new string[filesCount];
            for(int i = 0;i<filesCount;i++)
            {
                readStream = storageConnection.GetStream();
                tempBuffer = new byte[Protocol.TRACKER_PROTO_PKG_LEN_SIZE + 128];
                tempReadSize = readStream.Read(tempBuffer, 0, fileNameBufferLength);
                if(tempReadSize != fileNameBufferLength)
                {
                    if(tempReadSize == 10)//文件未传输完成 出现错误
                    {
                        error = tempBuffer[Protocol.PROTO_HEADER_STATUS_INDEX];
                        if (null != _logger)
                            _logger.ErrorFormat("上传文件中间发生异常，文件位置为:{0}.错误号为:{1}", i + 1, error);
                        throw new Exception("上传文件错误！");
                    }
                    if (null != _logger)
                        _logger.ErrorFormat("上传文件中间发生异常，文件位置为:{0}.未能返回错误号，错误header长度为:{1}", i + 1, tempReadSize);
                    throw new Exception("上传文件错误！");
                }

                fileNameSize = (int)Util.BufferToLong(tempBuffer, 0);
                tempFileNameBytes = new byte[fileNameSize];
                Array.Copy(tempBuffer, Protocol.TRACKER_PROTO_PKG_LEN_SIZE, tempFileNameBytes, 0, fileNameSize);
                chars = Util.ToCharArray(tempFileNameBytes);
                filesName[i] = new string(chars, 0, fileNameSize).Trim('\0').Trim();

            }
            readStream = storageConnection.GetStream();
            tempBuffer = new byte[10];
            tempReadSize = readStream.Read(tempBuffer, 0, 10);
            if (10 != tempReadSize)
            {
                if (null != _logger)
                    _logger.ErrorFormat("文件上传完毕，并且文件路径已经全部返回客户端。但是发生服务器传输信息头错误。错误header长度为:{0}", tempReadSize);
            }

            error = tempBuffer[Protocol.PROTO_HEADER_STATUS_INDEX];
            if (0 != error)
            {
                if (null != _logger)
                    _logger.ErrorFormat("文件上传完毕，并且文件路径已经全部返回客户端。但是服务器发生错误。错误号:{0}",error);
            }

            return filesName;
        }
    }
}
