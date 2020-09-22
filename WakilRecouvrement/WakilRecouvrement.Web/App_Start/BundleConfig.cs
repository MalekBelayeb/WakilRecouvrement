using System.Web.Optimization;

namespace WakilRecouvrement.Web
{
    public class BundleConfig
    {
        // Pour plus d'informations sur le regroupement, visitez https://go.microsoft.com/fwlink/?LinkId=301862
        public static void RegisterBundles(BundleCollection bundles)
        {

            bundles.Add(new ScriptBundle("~/bundles/jquery").Include(
                        "~/Scripts/jquery-{version}.js"));
            
            bundles.Add(new ScriptBundle("~/bundles/jqueryui").Include(
            "~/Scripts/jquery-ui-{version}.js"));

            bundles.Add(new ScriptBundle("~/bundles/jqueryval").Include(
                        "~/Scripts/jquery.validate*"));

            // Utilisez la version de développement de Modernizr pour le développement et l'apprentissage. Puis, une fois
            // prêt pour la production, utilisez l'outil de génération à l'adresse https://modernizr.com pour sélectionner uniquement les tests dont vous avez besoin.
            bundles.Add(new ScriptBundle("~/bundles/modernizr").Include(
                        "~/Scripts/modernizr-*"));

            bundles.Add(new ScriptBundle("~/bundles/bootstrap").Include(
                "~/Scripts/bootstrap.bundle.min.js"));

            bundles.Add(new StyleBundle("~/Content/css").Include(
                      "~/Content/bootstrap.css",
                      "~/adminlte/plugins/fontawesome-free/css/all.min.css",
                      "~/adminlte/css/adminlte.min.css",
                      "~/Content/site.css"));
           
            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
              "~/Content/themes/base/jquery.ui.core.css",
              "~/Content/themes/base/jquery.ui.resizable.css",
              "~/Content/themes/base/jquery.ui.selectable.css",
              "~/Content/themes/base/jquery.ui.accordion.css",
              "~/Content/themes/base/jquery.ui.autocomplete.css",
              "~/Content/themes/base/jquery.ui.button.css",
              "~/Content/themes/base/jquery.ui.dialog.css",
              "~/Content/themes/base/jquery.ui.slider.css",
              "~/Content/themes/base/jquery.ui.tabs.css",
              "~/Content/themes/base/jquery.ui.datepicker.css",
              "~/Content/themes/base/jquery.ui.progressbar.css",
              "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new StyleBundle("~/Content/themes/base/css").Include(
              "~/Content/themes/base/jquery.ui.core.css",
              "~/Content/themes/base/jquery.ui.autocomplete.css",
              "~/Content/themes/base/jquery.ui.theme.css"));

            bundles.Add(new ScriptBundle("~/adminlte/js").Include(
                 "~/adminlte/js/adminlte.min.js"));
        }
    }
}
