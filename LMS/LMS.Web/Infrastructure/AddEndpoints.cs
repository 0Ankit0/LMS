using System;
using System.Reflection;

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace LMS.Web.Infrastructure;

public static class EndpointExtensions
{
    public static IServiceCollection AddEndpoints(
        this IServiceCollection services,
        Assembly assembly)
    {
        ServiceDescriptor[] serviceDescriptors = assembly
            .DefinedTypes
            .Where(type => type is { IsAbstract: false, IsInterface: false } &&
                           type.IsAssignableTo(typeof(IEndpoint)))
            .Select(type => ServiceDescriptor.Transient(typeof(IEndpoint), type))
            .ToArray();

        services.TryAddEnumerable(serviceDescriptors);

        // Register all repository interfaces and their implementations
        var repoTypes = assembly.DefinedTypes
            .Where(t => t.IsClass && !t.IsAbstract && t.Name.EndsWith("Repository"));
        foreach (var repoType in repoTypes)
        {
            var iface = repoType.GetInterfaces().FirstOrDefault(i => i.Name == "I" + repoType.Name);
            if (iface != null)
            {
                services.TryAddScoped(iface, repoType);
            }
        }

        return services;
    }
}