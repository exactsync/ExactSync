using System.Web;
using System.Web.Optimization;

namespace ExactSync
{
    public class BundleConfig
    {
        // For more information on bundling, visit http://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {
            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Use the development version of Modernizr to develop with and learn from. Then, when you're
            // ready for production, use the build tool at http://modernizr.com to pick only the tests you need.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                      "~/Scripts/bootstrap.js",
                      "~/Scripts/respond.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/Content/site.css"));

            // AdminLTE Themes - Begin
            // Styles
            bundles.Add(new StyleBundle("~/Style/bootstrap").Include(
                "~/Themes/bootstrap/css/bootstrap.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Style/fontawesome").Include(
                "~/Themes/font-awesome/css/font-awesome.min.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Style/adminlte").Include(
                "~/Themes/css/AdminLTE.min.css"));

            bundles.Add(new StyleBundle("~/Style/plugin/icheck").Include(
                "~/Themes/plugins/iCheck/flat/blue.css", new CssRewriteUrlTransform()));

            bundles.Add(new StyleBundle("~/Style/skins").Include(
                "~/Themes/css/skins/_all-skins.min.css"));

            bundles.Add(new StyleBundle("~/Style/style").Include(
                "~/Themes/css/style.css"));

            // Scripts
            bundles.Add(new ScriptBundle("~/Script/jquery").Include(
                "~/Themes/plugins/jQuery/jQuery-2.1.4.min.js"));

            bundles.Add(new ScriptBundle("~/Script/bootstrap").Include(
                "~/Themes/bootstrap/js/bootstrap.min.js"));

            bundles.Add(new ScriptBundle("~/Script/plugin/icheck").Include(
                "~/Themes/plugins/iCheck/icheck.min.js"));

            bundles.Add(new ScriptBundle("~/Script/plugin/slimscroll").Include(
                "~/Themes/plugins/slimScroll/jquery.slimscroll.min.js"));

            bundles.Add(new ScriptBundle("~/Script/plugin/fastclick").Include(
                "~/Themes/plugins/fastclick/fastclick.min.js"));

            bundles.Add(new ScriptBundle("~/Script/app").Include(
                "~/Themes/js/app.min.js"));
            // AdminLTE Themes - End
        }
    }
}