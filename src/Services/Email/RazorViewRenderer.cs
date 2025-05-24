using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewEngines;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace light_quiz_api.Services.Email
{
    public class RazorViewToStringRenderer : IRazorViewToStringRenderer
    {
        private readonly IRazorViewEngine _razorViewEngine;
        private readonly ITempDataProvider _tempDataProvider;
        private readonly IServiceProvider _serviceProvider;

        public RazorViewToStringRenderer(
            IRazorViewEngine razorViewEngine,
            ITempDataProvider tempDataProvider,
            IServiceProvider serviceProvider)
        {
            _razorViewEngine = razorViewEngine ?? throw new ArgumentNullException(nameof(razorViewEngine));
            _tempDataProvider = tempDataProvider ?? throw new ArgumentNullException(nameof(tempDataProvider));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public async Task<string> RenderViewToStringAsync<TModel>(string viewName, TModel model)
        {
            if (string.IsNullOrEmpty(viewName))
            {
                throw new ArgumentNullException(nameof(viewName));
            }

            var httpContext = new DefaultHttpContext { RequestServices = _serviceProvider };
            var actionContext = new ActionContext(httpContext, new RouteData(), new ActionDescriptor());

            ViewEngineResult viewResult = _razorViewEngine.FindView(actionContext, viewName, isMainPage: false);

            if (viewResult.View == null || !viewResult.Success)
            {
                viewResult = _razorViewEngine.GetView(executingFilePath: null, viewPath: viewName, isMainPage: false);
            }

            if (viewResult.View == null || !viewResult.Success)
            {
                var searchedLocationsMessage = viewResult.SearchedLocations.Any() ?
                                               $" Searched locations: {string.Join(", ", viewResult.SearchedLocations)}." :
                                               " No locations were searched (this can happen if the view path is absolute or not found by FindView).";

                throw new InvalidOperationException(
                    $"Unable to find view '{viewName}'.{searchedLocationsMessage} " +
                    "Ensure the .cshtml file's 'Build Action' is 'Content' and 'Copy to Output Directory' is 'Copy if newer' or 'Copy always'. " +
                    "Also, verify RazorViewEngineOptions in Program.cs if using custom template paths and not providing the full path here.");
            }

            var viewDataDictionary = new ViewDataDictionary<TModel>(
                metadataProvider: new EmptyModelMetadataProvider(),
                modelState: new ModelStateDictionary())
            {
                Model = model
            };

            var viewContext = new ViewContext(
                actionContext,
                viewResult.View,
                viewDataDictionary,
                new TempDataDictionary(actionContext.HttpContext, _tempDataProvider),
                TextWriter.Null,
                new HtmlHelperOptions()
            );

            using (var stringWriter = new StringWriter())
            {
                viewContext.Writer = stringWriter;
                await viewResult.View.RenderAsync(viewContext);
                return stringWriter.ToString();
            }
        }
    }
}