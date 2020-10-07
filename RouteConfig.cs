using DotNetNuke.Web.Mvc.Routing;

namespace Dnn.Modules.MVCPartialTest
{
    public class RouteConfig : IMvcRouteMapper
    {
        public void RegisterRoutes(IMapRoute mapRouteManager)
        {
            mapRouteManager.MapRoute("MVCPartialTest", "MVCPartialTest", "{controller}/{action}", new[]
            {"Dnn.Modules.MVCPartialTest.Controllers"});
        }
    }
}
