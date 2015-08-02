using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Web.Mvc.Html;

namespace ExactSync.Helpers
{
    public static class HTMLHelperExtensions
    {
        public static string IsSelected(this HtmlHelper html, string controller = null, string action = null)
        {
            string cssClass = "active";
            string currentAction = (string)html.ViewContext.RouteData.Values["action"];
            string currentController = (string)html.ViewContext.RouteData.Values["controller"];

            if (String.IsNullOrEmpty(controller))
            {
                controller = currentController;
            }

            if (String.IsNullOrEmpty(action))
            {
                action = currentAction;
            }

            return controller.Contains(currentController) && action.Contains(currentAction) ? cssClass : String.Empty;
        }
    }
}
