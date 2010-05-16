/****************************************************************************************************************
*                                                                                                               *
* Copyright (C) 2010 Seapeak.Xu / xvhfeng                                                                       *
* FastDFS .Net Client may be copied only under the terms of the GNU General Public License V3,                  *
* which may be found in the FastDFS .Net Client source kit.                                                     *
* Please visit the FastDFS .Net Client Home Page https://code.google.com/p/fastdfs-net-client/ for more detail. *
*                                                                                                               *
****************************************************************************************************************/

using System;
using System.Xml;

namespace FastDFS.Client.Core
{
    /// <summary>
    /// 配置文件读取类
    /// </summary>
    public static class ConfigReader
    {
        /// <summary>
        /// 加载xml文件
        /// </summary>
        /// <param name="path">解析指定文件的路径.</param>
        /// <returns></returns>
        /// <remarks>该路径为对于应用程序根目录的虚拟路径，但是不包括开头的路径符号</remarks>
        /// <example>
        /// XmlDocument doc = ConfigReader.LoadXml("config\\FastDFS.config");
        /// </example>
        public static XmlDocument LoadXml(string path)
        {
            XmlDocument doc = new XmlDocument();
            doc.Load(GetFullPath(path));
            return doc;
        }

        /// <summary>
        /// 得到配置文件的物理路径
        /// </summary>
        /// <param name="path">The path.</param>
        /// <returns></returns>
        private static string GetFullPath(string path)
        {
            string basePath = AppDomain.CurrentDomain.BaseDirectory;
            if (basePath.ToLower().IndexOf("\\bin") > 0)
            {
                basePath = basePath.Substring(0, basePath.ToLower().IndexOf("\\bin"));
                basePath = string.Format("{0}\\", basePath);
            }
            return string.Format("{0}{1}", basePath, path);
        }

        /// <summary>
        /// 解析指定文件得到指定标签列表
        /// </summary>
        /// <param _name="doc">配置文件</param>
        /// <param _name="tagName">标签名称</param>
        /// <returns></returns>
        public static XmlNodeList Analyze(XmlDocument doc, string tagName)
        {
            //doc.DocumentElement.SelectNodes("")
            return null == doc ? null : doc.GetElementsByTagName(tagName);
        }

        /// <summary>
        /// Analyzes the single.
        /// </summary>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <returns></returns>
        public static XmlNode AnalyzeSingle(XmlDocument doc, string tagName)
        {
            XmlNodeList nodes = Analyze(doc, tagName);
            if (null == nodes || 0 == nodes.Count) return null;
            return nodes[0];
        }

        /// <summary>
        /// 得到标签的制定属性值
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="node">The node.</param>
        /// <param _name="attributeName">Name of the attribute.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetAttributeValue(XmlNode node, string attributeName, out object value)
        {
            if (null == node || string.IsNullOrEmpty(attributeName))
            {
                value = null;
                return false;
            }
            if (!HasAttribute(node, attributeName))
            {
                value = null;
                return false;
            }

            XmlNode attribute = node.Attributes.GetNamedItem(attributeName);
            if (null == attribute)
            {
                value = null;
                return false;
            }
            try
            {
                value = attribute.Value.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the get attribute value.
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <param _name="attributeName">Name of the attribute.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetAttributeValue(XmlDocument doc, string tagName, string attributeName, out object value)
        {
            if (null == doc)
            {
                value = null;
                return false;
            }

            XmlNodeList nodes = Analyze(doc, tagName);
            if (null == nodes || 0 == nodes.Count)
            {
                value = null;
                return false;
            }

            if (!TryGetAttributeValue(nodes[0], attributeName, out value))
            {
                value = null;
                return false;
            }
            return true;
        }

        /// <summary>
        /// 节点是否存在值
        /// </summary>
        /// <param _name="node"></param>
        /// <returns></returns>
        public static bool HasValue(XmlNode node)
        {
            return null != node && !string.IsNullOrEmpty(node.InnerText);
        }

        /// <summary>
        /// Determines whether the specified doc has value.
        /// </summary>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <returns>
        /// 	<c>true</c> if the specified doc has value; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasValue(XmlDocument doc, string tagName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasValue(node);
        }

        /// <summary>
        /// 节点是否存在属性
        /// </summary>
        /// <param _name="node"></param>
        /// <returns></returns>
        public static bool HasAttributes(XmlNode node)
        {
            return null == node ? false : null != node.Attributes && 0 != node.Attributes.Count;
        }

        public static bool HasAttributes(XmlDocument doc, string tagName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasAttributes(node);
        }

        /// <summary>
        /// 节点是否存在指定名称的属性
        /// </summary>
        /// <param _name="node"></param>
        /// <param _name="attributeName"></param>
        /// <returns></returns>
        public static bool HasAttribute(XmlNode node, string attributeName)
        {
            if (!HasAttributes(node)) return false;
            return null != node.Attributes.GetNamedItem(attributeName);
        }

        public static bool HasAttribute(XmlDocument doc, string tagName, string attributeName)
        {
            XmlNode node = AnalyzeSingle(doc, tagName);
            return HasAttribute(node, attributeName);
        }

        /// <summary>
        /// 试图得到节点的值
        /// </summary>
        /// <param _name="node"></param>
        /// <param _name="value"></param>
        /// <returns></returns>
        public static bool TryGetNodeValue(XmlNode node, out object value)
        {
            if (!HasValue(node))
            {
                value = null;
                return false;
            }

            try
            {
                value =  node.InnerText.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }

        /// <summary>
        /// Tries the get node value.
        /// </summary>
        /// <typeparam _name="T"></typeparam>
        /// <param _name="doc">The doc.</param>
        /// <param _name="tagName">Name of the tag.</param>
        /// <param _name="value">The value.</param>
        /// <returns></returns>
        public static bool TryGetNodeValue(XmlDocument doc, string tagName, out object value)
        {
            if (!HasValue(doc, tagName))
            {
                value = null;
                return false;
            }

            try
            {
                value = AnalyzeSingle(doc, tagName).InnerText.Trim();
                return true;
            }
            catch
            {
                value = null;
                return false;
            }
        }
    }
}