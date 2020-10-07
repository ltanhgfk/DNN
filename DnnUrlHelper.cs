#region Copyright
// 
// DotNetNuke® - http://www.dnnsoftware.com
// Copyright (c) 2002-2014
// by DNN Corporation
// 
// Permission is hereby granted, free of charge, to any person obtaining a copy of this software and associated 
// documentation files (the "Software"), to deal in the Software without restriction, including without limitation 
// the rights to use, copy, modify, merge, publish, distribute, sublicense, and/or sell copies of the Software, and 
// to permit persons to whom the Software is furnished to do so, subject to the following conditions:
// 
// The above copyright notice and this permission notice shall be included in all copies or substantial portions 
// of the Software.
// 
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR IMPLIED, INCLUDING BUT NOT LIMITED 
// TO THE WARRANTIES OF MERCHANTABILITY, FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL 
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER LIABILITY, WHETHER IN AN ACTION OF 
// CONTRACT, TORT OR OTHERWISE, ARISING FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER 
// DEALINGS IN THE SOFTWARE.
#endregion

using System;
using System.Web.Mvc;
using System.Web.Routing;
using DotNetNuke.Common;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Routing;

namespace DotNetNuke.Web.Mvc.Helpers
{
    public class DnnUrlHelper
    {
        private readonly ViewContext _viewContext;

        public DnnUrlHelper(ViewContext viewContext)
        {
            Requires.NotNull("viewContext", viewContext); 
            
            _viewContext = viewContext;

            var controller = viewContext.Controller as IDnnController;

            if (controller == null)
            {
                throw new InvalidOperationException("The DnnUrlHelper class can only be used in Views that inherit from DnnWebViewPage");
            }

            ModuleContext = controller.ModuleContext;
        }

        public ModuleInstanceContext ModuleContext { get; set; }

        public virtual string Action()
        {
            return _viewContext.RequestContext.HttpContext.Request.RawUrl;
        }

        public virtual string Action(string actionName)
        {
            return GenerateUrl(actionName, null, new RouteValueDictionary(), false);
        }

        public virtual string Action(string actionName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(actionName, null, routeValues, false);
        }

        public virtual string Action(string actionName, object routeValues)
        {
            return GenerateUrl(actionName, null, TypeHelper.ObjectToDictionary(routeValues), false);
        }

        public virtual string Action(string actionName, string controllerName)
        {
            return GenerateUrl(actionName, controllerName, new RouteValueDictionary(), false);
        }

        public virtual string Action(string actionName, string controllerName, RouteValueDictionary routeValues)
        {
            return GenerateUrl(actionName, controllerName, routeValues, false);
        }

        public virtual string Action(string actionName, string controllerName, object routeValues)
        {
            return GenerateUrl(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), false);
		}

		public virtual string AsyncAction(string actionName)
		{
			return GenerateUrl(actionName, null, new RouteValueDictionary(), true);
		}

		public virtual string AsyncAction(string actionName, RouteValueDictionary routeValues)
		{
			return GenerateUrl(actionName, null, routeValues, true);
		}

		public virtual string AsyncAction(string actionName, object routeValues)
		{
			return GenerateUrl(actionName, null, TypeHelper.ObjectToDictionary(routeValues), true);
		}

		public virtual string AsyncAction(string actionName, string controllerName)
		{
			return GenerateUrl(actionName, controllerName, new RouteValueDictionary(), true);
		}

		public virtual string AsyncAction(string actionName, string controllerName, RouteValueDictionary routeValues)
		{
			return GenerateUrl(actionName, controllerName, routeValues, true);
		}

		public virtual string AsyncAction(string actionName, string controllerName, object routeValues)
		{
			return GenerateUrl(actionName, controllerName, TypeHelper.ObjectToDictionary(routeValues), true);
		}

		private string GenerateUrl(string actionName, string controllerName, RouteValueDictionary routeValues, Boolean isAsync)
        {
            routeValues["controller"] = controllerName;
            routeValues["action"] = actionName;

			if (isAsync)
			{
				routeValues["async"] = true;
			}

            return ModuleRoutingProvider.Instance().GenerateUrl(routeValues, ModuleContext);
        }
    }
}
