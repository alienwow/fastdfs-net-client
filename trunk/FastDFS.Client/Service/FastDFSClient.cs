/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System.Collections.Generic;
using FastDFS.Client.Component;

namespace FastDFS.Client.Service
{
    /// <summary>
    /// DFS的客户端
    /// </summary>
    public class FastDFSClient : StorageClient
    {
        private const string SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR = "/";

        /// <summary>
        ///上传文件.
        /// </summary>
        /// <param name="buffer">上传文件的二进制流.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(byte[] buffer, string extension)
        {
            return Upload(string.Empty, buffer, extension, null);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="buffer">上传文件的二进制流.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <param name="metadatas">文件的扩展属性.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(byte[] buffer, string extension, NameValuePair[] metadatas)
        {
            return Upload(string.Empty, buffer, extension, metadatas);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="groupName">上传服务器的组名.</param>
        /// <param name="buffer">上传文件的二进制流.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string groupName, byte[] buffer, string extension)
        {
            return Upload(groupName, buffer, extension, null);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="groupName">上传服务器的组名.</param>
        /// <param name="buffer">上传文件的二进制流.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <param name="metadatas">文件的扩展属性.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string groupName, byte[] buffer, string extension, NameValuePair[] metadatas)
        {
            string[] uploadPath = DoUpload(groupName, string.Empty, buffer, extension, metadatas);
            return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}{2}", uploadPath[0], SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="localFileName">上传文件的本地路径.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string localFileName, string extension)
        {
            return Upload(string.Empty, localFileName, extension, null);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="localFileName">上传文件的本地路径.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string localFileName, string extension, NameValuePair[] metadatas)
        {
            return Upload(string.Empty, localFileName, extension, metadatas);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="groupName">上传服务器的组名.</param>
        /// <param name="localFileName">上传文件的本地路径.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string groupName, string localFileName, string extension)
        {
            return Upload(groupName, localFileName, extension, null);
        }

        /// <summary>
        /// 上传文件.
        /// </summary>
        /// <param name="groupName">上传服务器的组名.</param>
        /// <param name="localFileName">上传文件的本地路径.</param>
        /// <param name="extension">上传文件的扩展名.</param>
        /// <param name="metadatas">上传文件的扩展属性.</param>
        /// <returns>文件所在的路径</returns>
        public static string Upload(string groupName, string localFileName, string extension, NameValuePair[] metadatas)
        {
            string[] uploadPath = DoUpload(groupName, localFileName, null, extension, metadatas);
            return uploadPath != null ? Util.GetFilePath(string.Format("{0}{1}{2}", uploadPath[0], SPLIT_GROUP_NAME_AND_FILENAME_SEPERATOR, uploadPath[1])) : null;
        }

        /// <summary>
        /// 批量上传文件
        /// </summary>
        /// <remarks>
        /// 此功能仅为FastDFS扩展版本提供
        /// </remarks>
        /// <param name="groupName"></param>
        /// <param name="filesBuffer"></param>
        /// <param name="filesExtension"></param>
        /// <returns></returns>
        public static string[] BatchUpload(string groupName, IList<byte[]> filesBuffer,
                                                   string[] filesExtension)
        {
            return DoBatchUpload(groupName, filesBuffer, filesExtension);
        }


    }
}
