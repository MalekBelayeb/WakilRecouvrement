using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Optimization;
using System.Web.Routing;

namespace WakilRecouvrement.Web
{
    public class MvcApplication : System.Web.HttpApplication
    {
        protected void Application_Start()
        {
            AreaRegistration.RegisterAllAreas();
            GlobalConfiguration.Configure(WebApiConfig.Register);
            FilterConfig.RegisterGlobalFilters(GlobalFilters.Filters);
            RouteConfig.RegisterRoutes(RouteTable.Routes);
            BundleTable.EnableOptimizations = true;

            BundleConfig.RegisterBundles(BundleTable.Bundles);
            log4net.Config.XmlConfigurator.Configure();


            //JobScheduler.StartAsync().GetAwaiter().GetResult();
            ConfigurationManager.AppSettings["ExecuteTaskServiceCallSchedulingStatus"] = "OFF";


            //SqlDependency.Start(ConfigurationManager.ConnectionStrings["WRConnectionStrings"].ConnectionString);
        


            
        }

        


        protected void Application_End()
        {
           //SqlDependency.Stop(ConfigurationManager.ConnectionStrings["WRConnectionStrings"].ConnectionString);

        }
    }
}
