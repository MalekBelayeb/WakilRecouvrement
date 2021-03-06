﻿using System.Web.Mvc;
using System.Web.Routing;

namespace WakilRecouvrement.Web
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Authentification", action = "Login", id = UrlParameter.Optional }
            );

            routes.MapRoute(
               name: "AffecterAgents",
               url: "{controller}/{action}/{id}",
               defaults: new { controller = "Affectation", action = "AffecterAgents", id = UrlParameter.Optional }
           );
        }
    }
}
