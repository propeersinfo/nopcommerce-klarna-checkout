using Nop.Web.Framework.Mvc.Routes;
using System.Web.Mvc;
using System.Web.Routing;

namespace Motillo.Nop.Plugin.KlarnaCheckout
{
    public class RouteProvider : IRouteProvider
    {
        public void RegisterRoutes(RouteCollection routes)
        {
            routes.MapRoute("Motillo.Nop.Plugin.KlarnaCheckout",
                 "Plugins/Motillo.KlarnaCheckout/{action}",
                 new { controller = "KlarnaCheckout" },
                 new[] { "Motillo.Nop.Plugin.KlarnaCheckout.Controllers" }
            );
        }

        public int Priority
        {
            get { return 0; }
        }
    }
}
