using Microsoft.AspNetCore.Authorization;

namespace Inventory.Authorization.Requirements
{
    /// <summary>
    /// Represents an authorization requirement for accessing a specific API endpoint with a required action.
    /// </summary>
    /// <remarks>
    /// The <see cref="EndpointAccessRequirement"/> class is used in policy-based authorization to ensure that a user has permission to perform a specific action on a given endpoint.
    /// It encapsulates the endpoint name and the required action for access control checks.
    /// </remarks>
    internal class EndpointAccessRequirement : IAuthorizationRequirement
    {
    }
}
