using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

namespace Inventory.Controllers.Conventions
{
    // https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing?view=aspnetcore-9.0#ar6

    /// <summary>
    /// Applies a global route prefix to all controllers in the application.
    /// </summary>
    /// <remarks>
    /// The <see cref="DefaultEndpointPrefixConvention"/> class implements <see cref="IApplicationModelConvention"/> to prepend a specified route prefix to all controller routes.
    /// This is useful for enforcing a consistent API route structure across the application.
    /// </remarks>
    internal class DefaultEndpointPrefixConvention(string routePrefix) : IApplicationModelConvention
    {
        private readonly AttributeRouteModel _routePrefix = new(new RouteAttribute(routePrefix));

        /// <summary>
        /// Applies the global route prefix to all controllers in the application model.
        /// </summary>
        /// <param name="application">The <see cref="ApplicationModel"/> representing the application's controllers and actions.</param>
        /// <remarks>
        /// Prepends the global prefix to existing routes or sets it as the route if none is defined.
        /// </remarks>
        public void Apply(ApplicationModel application)
        {
            foreach (var controller in application.Controllers)
            {
                foreach (var selector in controller.Selectors)
                {
                    if (selector.AttributeRouteModel != null)
                    {
                        // Prepend the global prefix to existing route
                        selector.AttributeRouteModel = AttributeRouteModel.CombineAttributeRouteModel(
                            _routePrefix,
                            selector.AttributeRouteModel
                        );
                    }
                    else
                    {
                        // Add the global prefix if no route is set
                        selector.AttributeRouteModel = _routePrefix;
                    }
                }
            }
        }
    }
}
