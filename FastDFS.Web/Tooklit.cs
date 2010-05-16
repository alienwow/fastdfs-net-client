using System;
using System.Data;
using System.Configuration;
using System.IO;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;

namespace FastDFS.Web
{
    public class Tooklit
    {
        /// <summary>
        /// 得到文件扩展名
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static string GetExtension(string path)
        {
            return Path.GetExtension(path);
        }

        public static void ShowInfo(Page page,string message)
        {
            page.ClientScript.RegisterClientScriptBlock(typeof (Tooklit), Guid.NewGuid().ToString(),
                                                        "<script language=\"javascript\" type=\"text/javascript\">window.alert(\"" +
                                                        message + "！\");</script>");
        }

        public static string GroupName
        {
            get
            {
                return ConfigurationManager.AppSettings["GroupName"];
            }
        }

        public static string DomainName
        {
            get
            {
                return ConfigurationManager.AppSettings["DomainName"];
            }
        }

        //public static string GetFilePath(string fileName)
        //{
        //    int index = fileName.IndexOf("/M00/");
        //    return index > -1 ? fileName.Remove(0, index + 4) : fileName;
        //}

        //public static void SaveData(string id, string path)
        //{
        //    DataTable dt;
        //    if (0 == Global.Dataset.Tables.Count)
        //    {
        //        dt = new DataTable("FastDFS");
        //        dt.Columns.Add("ID", typeof(string));
        //        dt.Columns.Add("Path", typeof(string));
        //    }
        //    else
        //    {
        //        dt = Global.Dataset.Tables["FastDFS"];
        //    }
        //    DataRow dr = dt.NewRow();
        //    dr["ID"] = id;
        //    dr["Path"] = path;
        //    dt.Rows.Add(dr);
        //    dt.AcceptChanges();
        //    if (0 == Global.Dataset.Tables.Count)
        //    {
        //        Global.Dataset.Tables.Add(dt);
        //    }
        //    Global.Dataset.AcceptChanges();
        //}

        public static string GetPath(string id)
        {
            DataSet ds = new DataSet("FastDFSData");
            string physicsPath = HttpContext.Current.Server.MapPath(@"~/data/FastDFS.xml");
            ds.ReadXml(physicsPath);
            ds.AcceptChanges();

            DataTable dt =  ds.Tables["FastDFS"];
            DataRow[] dr = dt.Select("ID = '" + id.Replace("'","''") + "'");
            return 0 == dr.Length ? string.Empty : dr[0]["Path"].ToString();
        }

        public static int Interval
        {
            get
            {
                return int.Parse(ConfigurationManager.AppSettings["Interval"]);
            }
        }

    }
}