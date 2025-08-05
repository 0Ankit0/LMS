using System;

using Microsoft.AspNetCore.Routing;

namespace LMS.Web.Infrastructure;

public interface IEndpoint
{
    void MapEndpoint(IEndpointRouteBuilder app);
}