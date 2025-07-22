using DataLibrary.Classes;
using DataLibrary.Interfaces;
using Inventory.Authorization.Requirements;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.Routing;
using System.Reflection;
using System.Security.Claims;

namespace Inventory.Authorization.Handlers
{
    /// <summary>
    /// Handles authorization for endpoint access based on user roles and required actions.
    /// </summary>
    /// <remarks>
    /// The <see cref="EndpointAccessHandler"/> class implements <see cref="AuthorizationHandler{TRequirement}"/> to evaluate whether a user's roles grant access to a specific endpoint and action.
    /// It uses the <see cref="IAccountContextHelper"/> to retrieve role permissions and determines if the required action is permitted for the endpoint.
    /// </remarks>
    internal class EndpointAccessHandler(IAccountContextHelper accountContextHelper, ILogger<EndpointAccessHandler> logger) : AuthorizationHandler<EndpointAccessRequirement>
    {
        private readonly IAccountContextHelper _accountContextHelper = accountContextHelper;
        private readonly ILogger<EndpointAccessHandler> _logger = logger;

        /// <summary>
        /// Evaluates whether the current user's roles grant access to the required endpoint and action.
        /// </summary>
        /// <param name="context">The authorization context containing user claims.</param>
        /// <param name="requirement">The endpoint access requirement to evaluate.</param>
        /// <returns>A task that represents the asynchronous operation.</returns>
        /// <remarks>
        /// Succeeds if any of the user's roles have the required action for the specified endpoint; otherwise, fails the requirement.
        /// </remarks>
        protected override async Task HandleRequirementAsync(AuthorizationHandlerContext context, EndpointAccessRequirement requirement)
        {
            string message = string.Empty;

            var httpContext = (context.Resource as DefaultHttpContext)?.HttpContext ?? context.Resource as HttpContext;
            var endpoint = httpContext?.GetEndpoint();

            if (endpoint != null)
            {
                // You can now access metadata and route info

                // Controller/action info
                var actionDescriptor = endpoint.Metadata.GetMetadata<ControllerActionDescriptor>();
                var actionName = actionDescriptor!.MethodInfo.Name;
                var controller = actionDescriptor.ControllerTypeInfo;
                var controllerName = actionDescriptor?.ControllerName.Replace("Controller", "");

                if (controller.GetCustomAttributes(inherit: true).Where(x => x is AllowAnonymousAttribute).ToList().Count == 0)
                {

                    var method = controller.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.DeclaredOnly)
                        .Where(m => m.Name == actionName).FirstOrDefault();

                    if (method!.GetCustomAttributes(inherit: true).Where(x => x is AllowAnonymousAttribute).ToList().Count == 0)
                    {

                        var actionType = EndpointPermissionActionType.None;

                        var httpMethodAttr = method.GetCustomAttributes(inherit: true)
                            .FirstOrDefault(attr =>
                                attr is HttpGetAttribute ||
                                attr is HttpPostAttribute ||
                                attr is HttpPutAttribute ||
                                attr is HttpDeleteAttribute ||
                                attr is HttpOptionsAttribute ||
                                attr is HttpPatchAttribute);

                        HttpMethodAttribute? attribute = null;

                        if (httpMethodAttr is HttpPutAttribute PutAttribute)
                        {
                            attribute = PutAttribute;
                            actionType = EndpointPermissionActionType.Update;
                        }
                        else if (httpMethodAttr is HttpOptionsAttribute OptionsAttribute)
                        {
                            attribute = OptionsAttribute;
                            actionType = EndpointPermissionActionType.Read;
                        }
                        else if (httpMethodAttr is HttpGetAttribute GetAttribute)
                        {
                            attribute = GetAttribute;
                            actionType = EndpointPermissionActionType.Read;
                        }
                        else if (httpMethodAttr is HttpDeleteAttribute DeleteAttribute)
                        {
                            attribute = DeleteAttribute;
                            actionType = EndpointPermissionActionType.Delete;
                        }
                        else if (httpMethodAttr is HttpPatchAttribute PatchAttribute)
                        {
                            attribute = PatchAttribute;
                            actionType = EndpointPermissionActionType.Update;
                        }
                        else if (httpMethodAttr is HttpPostAttribute PostAttribute)
                        {
                            attribute = PostAttribute;
                            actionType = EndpointPermissionActionType.Create;
                        }

                        var endpointName = $"{controllerName}/{actionName}";
                        var requiredAction = new EndpointPermissionAction(actionType);

                        if (attribute?.Template is not null)
                        {
                            var template = attribute!.Template.Replace("{", "").Replace("}", "");
                            endpointName += $"/{template}";
                        }

                        // Get all role names from claims
                        var roleNames = context.User.Claims
                            .Where(c => c.Type == ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToList();

                        if (roleNames.Count == 0) message = "No claims found for the current user. Are you missing an Authorization header?";

                        foreach (var roleName in roleNames)
                        {
                            var role = await _accountContextHelper.GetRoleByNameAsync(roleName);

                            if (role?.EndpointPermissions != null)
                            {
                                var roleEndpointPermission = role.EndpointPermissions
                                    .FirstOrDefault(rep =>
                                  string.Equals(rep.Endpoint, endpointName, StringComparison.OrdinalIgnoreCase) ||
                                  endpointName.StartsWith(rep.Endpoint!, StringComparison.OrdinalIgnoreCase) ||
                                  string.Equals(rep.Endpoint, "All", StringComparison.OrdinalIgnoreCase)
                                );

                                if (roleEndpointPermission != null &&
                                    roleEndpointPermission.EndpointPermissionActions!.Any(a =>
                                        a.EndpointPermissionActionType == requiredAction.EndpointPermissionActionType ||
                                        a.EndpointPermissionActionType == EndpointPermissionActionType.Write
                                    ))
                                {
                                    context.Succeed(requirement);
                                    return;
                                }
                            }

                            message = $"Role '{roleName}' does not have access to endpoint '{endpointName}' with action '{requiredAction.EndpointPermissionActionType}'.";
                        }
                    }
                }
            }

            message = string.IsNullOrEmpty(message)
                ? "Something went wrong during authorization."
                : message;

            _logger.LogError("Authorization failed: {Message}", message);
            context.Fail(new AuthorizationFailureReason(this, message));
            return;
        }
    }
}