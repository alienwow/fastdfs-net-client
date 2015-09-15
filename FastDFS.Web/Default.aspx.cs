using System;
using System.Collections.Generic;
using System.Data;
using System.Configuration;
using System.Collections;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.UI;
using System.Web.UI.WebControls;
using System.Web.UI.WebControls.WebParts;
using System.Web.UI.HtmlControls;
using FastDFS.Client.Component;
using FastDFS.Client.Service;
using log4net;

namespace FastDFS.Web
{
    public partial class _Default : System.Web.UI.Page
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);
        protected void Page_Load(object sender, EventArgs e)
        {
        }

        protected void Button1_Click(object sender, EventArgs e)
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                if (null != _logger)
                    _logger.InfoFormat("上传文件的大小为：{0}字节！", FileUpload1.PostedFile.ContentLength);
                DateTime begin = DateTime.Now;
                string filePath = FastDFSClient.Upload("g1",FileUpload1.FileBytes, Tooklit.GetExtension(FileUpload1.FileName));
                TimeSpan span = DateTime.Now - begin;
                if (null != _logger)
                    _logger.InfoFormat("上传文件Id为：{0}！路径为{1}!", guid, filePath);
                if (null != _logger)
                    _logger.InfoFormat("上传文件的时间为：{0}秒！", span.TotalSeconds);
                Response.Write("上传成功！");
            }
            catch (Exception exc)
            {
                Response.Write("上传失败！");
                if (null != _logger)
                    _logger.ErrorFormat("测试文件上传速度时发生异常：{0}" + exc.Message);
            }
        }

        protected void Button1_Click1(object sender, EventArgs e)
        {
            try
            {
                string guid = Guid.NewGuid().ToString();
                if (null != _logger)
                    _logger.InfoFormat("上传文件的大小为：{0}字节！", FileUpload1.PostedFile.ContentLength);
                DateTime begin = DateTime.Now;

                IList<Byte[]> list = new List<byte[]>();
                list.Add(FileUpload1.FileBytes);
                list.Add(FileUpload2.FileBytes);
                list.Add(FileUpload3.FileBytes);
                list.Add(FileUpload4.FileBytes);
                list.Add(FileUpload5.FileBytes);

                string[] filename = { Tooklit.GetExtension(FileUpload1.FileName), Tooklit.GetExtension(FileUpload2.FileName) ,
                Tooklit.GetExtension(FileUpload3.FileName),Tooklit.GetExtension(FileUpload4.FileName),Tooklit.GetExtension(FileUpload5.FileName)};

                FastDFSClient.BatchUpload("test", list, filename);


            
                TimeSpan span = DateTime.Now - begin;
                //if (null != _logger)
                //    _logger.InfoFormat("上传文件Id为：{0}！路径为{1}!", guid, filePath);
                if (null != _logger)
                    _logger.InfoFormat("上传文件的时间为：{0}秒！", span.TotalSeconds);
                Response.Write("上传成功！");
            }
            catch (Exception exc)
            {
                Response.Write("上传失败！");
                if (null != _logger)
                    _logger.ErrorFormat("测试文件上传速度时发生异常：{0}" + exc.Message);
            }

        }
    }
}
