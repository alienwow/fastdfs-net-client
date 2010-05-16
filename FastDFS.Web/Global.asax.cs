using System;
using System.Data;
using System.Configuration;
using System.Collections;
using System.IO;
using System.Reflection;
using System.Web;
using System.Web.Security;
using System.Web.SessionState;
using FastDFS.Client.Service;
using log4net;
using log4net.Config;

namespace FastDFS.Web
{
    public class Global : System.Web.HttpApplication
    {
        private static readonly ILog _logger = LogManager.GetLogger(MethodBase.GetCurrentMethod().DeclaringType);

        protected void Application_Start(object sender, EventArgs e)
        {
            try
            {
                XmlConfigurator.Configure(new FileInfo(Server.MapPath(@"~/Config/log4net.config")));
                FastDFSService.Start();

                if (null != _logger)
                    _logger.Info("应用程序启动！");
            }
            catch (Exception exc)
            {
                if (null != _logger)
                    _logger.Error(exc.Message);
            }
        }

        protected void Application_End(object sender, EventArgs e)
        {
            FastDFSService.Stop();
            if (null != _logger)
                _logger.Info("关闭应用程序！");
        }
    }
}