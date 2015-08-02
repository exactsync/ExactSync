using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;

namespace ExactSync
{
    public class RouteConfig
    {
        public static void RegisterRoutes(RouteCollection routes)
        {
            routes.IgnoreRoute("{resource}.axd/{*pathInfo}");

            routes.MapRoute(
                name: "DropboxWebhook",
                url: "webhook",
                defaults: new { controller = "Dropbox", action = "Webhook" }
            );

            routes.MapRoute(
                name: "Default",
                url: "{controller}/{action}/{page}",
                defaults: new { controller = "Home", action = "Index", page = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "Detail",
                url: "{controller}/{action}/{id}",
                defaults: new { controller = "Home", action = "Detail", id = UrlParameter.Optional }
            );

            routes.MapRoute(
                name: "DropboxConnect",
                url: "{controller}/{action}",
                defaults: new { controller = "Dropbox", action = "Connect" }
            );

            routes.MapRoute(
                name: "DropboxDisconnect",
                url: "{controller}/{action}",
                defaults: new { controller = "Dropbox", action = "Disconnect" }
            );

            routes.MapRoute(
                name: "DropboxAuthorize",
                url: "{controller}/{action}",
                defaults: new { controller = "Dropbox", action = "Authorize" }
            );

            routes.MapRoute(
                name: "ExactOnlineConnect",
                url: "{controller}/{action}",
                defaults: new { controller = "ExactOnline", action = "Connect" }
            );

            routes.MapRoute(
                name: "ExactOnlineDisconnect",
                url: "{controller}/{action}",
                defaults: new { controller = "ExactOnline", action = "Disconnect" }
            );

            routes.MapRoute(
                name: "ExactOnlineAuthorize",
                url: "{controller}/{action}",
                defaults: new { controller = "ExactOnline", action = "Authorize" }
            );
        }
    }
}