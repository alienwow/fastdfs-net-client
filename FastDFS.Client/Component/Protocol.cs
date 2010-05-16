/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System;

namespace FastDFS.Client.Component
{
    /// <summary>
    /// FastDFS通讯协议
    /// </summary>
    public sealed class Protocol
    {
        public const string FDFS_FIELD_SEPERATOR = "\u0002";
        public const byte FDFS_FILE_EXT_NAME_MAX_LEN = 5;
        public const byte FDFS_GROUP_NAME_MAX_LEN = 16;
        public const byte FDFS_IPADDR_SIZE = 16;
        public const byte FDFS_PROTO_CMD_QUIT = 82;
        public const string FDFS_RECORD_SEPERATOR = "\u0001";
        public const int PROTO_HEADER_CMD_INDEX = TRACKER_PROTO_PKG_LEN_SIZE;
        public const int PROTO_HEADER_STATUS_INDEX = TRACKER_PROTO_PKG_LEN_SIZE + 1;
        public const byte STORAGE_PROTO_CMD_DELETE_FILE = 12;
        public const byte STORAGE_PROTO_CMD_DOWNLOAD_FILE = 14;
        public const byte STORAGE_PROTO_CMD_GET_METADATA = 15;
        public const byte STORAGE_PROTO_CMD_RESP = 10;
        public const byte STORAGE_PROTO_CMD_SET_METADATA = 13;
        public const byte STORAGE_PROTO_CMD_UPLOAD_FILE = 11;
        public const byte STORAGE_SET_METADATA_FLAG_MERGE =(byte) 'M';
        public const byte STORAGE_SET_METADATA_FLAG_OVERWRITE =(byte) 'O';
        public const byte TRACKER_PROTO_CMD_SERVER_LIST_GROUP = 91;
        public const byte TRACKER_PROTO_CMD_SERVER_LIST_STORAGE = 92;
        public const byte TRACKER_PROTO_CMD_SERVER_RESP = 90;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ALL = 105;

        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_FETCH_ONE = 102;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITH_GROUP = 104;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_STORE_WITHOUT_GROUP = 101;
        public const byte TRACKER_PROTO_CMD_SERVICE_QUERY_UPDATE = 103;
        public const byte TRACKER_PROTO_CMD_SERVICE_RESP = 100;

        public const byte TRACKER_PROTO_CMD_SIZE = 1;
        public const byte TRACKER_PROTO_PKG_LEN_SIZE = 8;

        public const int TRACKER_QUERY_STORAGE_FETCH_BODY_LEN = FDFS_GROUP_NAME_MAX_LEN
                                                                + FDFS_IPADDR_SIZE - 1 + TRACKER_PROTO_PKG_LEN_SIZE;

        public const int TRACKER_QUERY_STORAGE_STORE_BODY_LEN = FDFS_GROUP_NAME_MAX_LEN
                                                                + FDFS_IPADDR_SIZE + TRACKER_PROTO_PKG_LEN_SIZE;


        /// <summary>
        /// 批量上传文件协议
        /// </summary>
        public const int STORAGE_PROTO_CMD_Batch_UPLOAD = 126;
    }
}