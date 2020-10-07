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
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.IO;
using System.Web;
using System.Web.Mvc;
using System.Web.Routing;
using System.Web.UI;
using DotNetNuke.Collections;
using DotNetNuke.ComponentModel;
using DotNetNuke.Entities.Modules;
using DotNetNuke.Entities.Modules.Actions;
using DotNetNuke.Services.Exceptions;
using DotNetNuke.UI.Modules;
using DotNetNuke.Web.Mvc.Common;
using DotNetNuke.Web.Mvc.Framework.Modules;
using DotNetNuke.Web.Mvc.Routing;
using DotNetNuke.Web.Mvc.Framework.Controllers;
using DotNetNuke.Web.Mvc.Framework.ActionResults;

namespace DotNetNuke.Web.Mvc
{
    public class MvcHostControl : ModuleControlBase, IActionable
    {
        private ModuleRequestResult _result;
        private string _controlKey;

        public MvcHostControl()
        {
            _controlKey = String.Empty;
        }

        protected MvcHostControl(string controlKey)
        {
            _controlKey = controlKey;
        }

        private IModuleExecutionEngine GetModuleExecutionEngine()
        {
            var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

            if (moduleExecutionEngine == null)
            {
                moduleExecutionEngine = new ModuleExecutionEngine();
                ComponentFactory.RegisterComponentInstance<IModuleExecutionEngine>(moduleExecutionEngine);
            }

            return moduleExecutionEngine;
        }

        private ModuleRequestContext GetModuleRequestContext(HttpContextBase httpContext)
        {
            var module = ModuleContext.Configuration;

            //TODO DesktopModuleControllerAdapter usage is temporary in order to make method testable
            var desktopModule = DesktopModuleControllerAdapter.Instance.GetDesktopModule(module.DesktopModuleID, module.PortalID);
            var defaultControl = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey("", module.ModuleDefID);
            var defaultSegments = defaultControl.ControlSrc.Replace(".mvc", "").Split('/');

            var moduleApplication = new ModuleApplication
                                            {
                                                DefaultActionName = defaultSegments[1],
                                                DefaultControllerName = defaultSegments[0],
                                                ModuleName = desktopModule.ModuleName,
                                                FolderPath = desktopModule.FolderName
                                            };

            RouteData routeData;

            if (String.IsNullOrEmpty(_controlKey))
            {
                _controlKey = httpContext.Request.QueryString.GetValueOrDefault("ctl", String.Empty);
            }

            var moduleId = httpContext.Request.QueryString.GetValueOrDefault("moduleId", -1);
            if (moduleId != ModuleContext.ModuleId && String.IsNullOrEmpty(_controlKey))
            {
                //Set default routeData for module that is not the "selected" module
                routeData = new RouteData();
                routeData.Values.Add("controller", defaultSegments[0]);
                routeData.Values.Add("action", defaultSegments[1]);
            }
            else
            {
                var control = ModuleControlControllerAdapter.Instance.GetModuleControlByControlKey(_controlKey, module.ModuleDefID);
                routeData = ModuleRoutingProvider.Instance().GetRouteData(httpContext, control);
            }

            var moduleRequestContext = new ModuleRequestContext
                                            {
                                                HttpContext = httpContext,
                                                ModuleContext = ModuleContext, 
                                                ModuleApplication = moduleApplication,
                                                RouteData = routeData
                                            };

            return moduleRequestContext;
        }

        private ModuleActionCollection LoadActions(ModuleRequestResult result)
        {
            var actions = new ModuleActionCollection();

            if (result.ModuleActions != null)
            {
                foreach (ModuleAction action in result.ModuleActions)
                {
                    action.ID = ModuleContext.GetNextActionID();
                    actions.Add(action);
                }
            }

            return actions;
        }

        public ModuleActionCollection ModuleActions { get; private set; }

        protected override void OnInit(EventArgs e)
        {
            base.OnInit(e);

            try
            {
                HttpContextBase httpContext = new HttpContextWrapper(HttpContext.Current);

                var moduleExecutionEngine = GetModuleExecutionEngine();

                _result = moduleExecutionEngine.ExecuteModule(GetModuleRequestContext(httpContext));

                ModuleActions = LoadActions(_result);

                httpContext.SetModuleRequestResult(_result);
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }

        }

		private LiteralControl _content;
		private string _html;

        protected override void OnPreRender(EventArgs e)
        {
            base.OnPreRender(e);

            try
            {
                if (_result != null)					
                {
					// Write the result to html field first so we can access it if we shall render partial
					_html = RenderModule(_result).ToString();
                    _content = new LiteralControl(_html);
                    Controls.Add(_content);
                }
            }
            catch (Exception exc)
            {
                Exceptions.ProcessModuleLoadException(this, exc);
            }
        }

		protected override void Render(HtmlTextWriter writer)
		{
			if (_result != null)
			{
                // Check if we shall render partial
                IDnnController controller = _result.ControllerContext.Controller as IDnnController;
                var result = _result.ActionResult as IDnnViewResult;

                if (controller != null && result != null && result.IsExclusive)
                {
                    RenderPartial(controller.ControllerContext.HttpContext.Response, _html);
                }
            }

            base.Render(writer);
		}

		private void RenderPartial(HttpResponseBase response, string output)
		{
			// Clear all results
			base.Page.Response.Clear();

			// Set all properties to that of the ActionResult
			base.Page.Response.Charset = response.Charset;
			base.Page.Response.ContentType = response.ContentType;
			base.Page.Response.ContentEncoding = response.ContentEncoding;
			base.Page.Response.Expires = response.Expires;
			base.Page.Response.ExpiresAbsolute = response.ExpiresAbsolute;
			base.Page.Response.Filter = response.Filter;
			base.Page.Response.HeaderEncoding = response.HeaderEncoding;
			base.Page.Response.RedirectLocation = response.RedirectLocation;
			base.Page.Response.Status = response.Status;
			base.Page.Response.StatusCode = response.StatusCode;
			base.Page.Response.StatusDescription = response.StatusDescription;
			base.Page.Response.SubStatusCode = response.SubStatusCode;
			base.Page.Response.SuppressContent = response.SuppressContent;
			base.Page.Response.SuppressFormsAuthenticationRedirect = response.SuppressFormsAuthenticationRedirect;
			base.Page.Response.TrySkipIisCustomErrors = response.TrySkipIisCustomErrors;

			// Set header informations
			// May be I forgot something specially related to DNN, so please don't blame me
			var headers = response.Headers.AllKeys.SelectMany(response.Headers.GetValues, (key, value) => new KeyValuePair<string, string>(key, value));

			foreach (KeyValuePair<string, string> header in headers)
			{
				base.Page.Response.Headers[header.Key] = header.Value;
			}

			// Write response to stream and send it back to the client
			base.Page.Response.Write(output);
			base.Page.Response.Flush();
			base.Page.Response.Close();
			base.Page.Response.End();
		}

		private MvcHtmlString RenderModule(ModuleRequestResult moduleResult)
        {
            MvcHtmlString moduleOutput;

            using (var writer = new StringWriter(CultureInfo.CurrentCulture))
            {
                var moduleExecutionEngine = ComponentFactory.GetComponent<IModuleExecutionEngine>();

                moduleExecutionEngine.ExecuteModuleResult(moduleResult, writer);

                moduleOutput = MvcHtmlString.Create(writer.ToString());
            }

            return moduleOutput;
        }
    }
}
