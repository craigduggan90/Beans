using Beans.Common.Services;
using Microsoft.Extensions.DependencyInjection;
using System.Diagnostics.CodeAnalysis;

namespace Beans.Common;

[ExcludeFromCodeCoverage]
public static class DependencyInjection
{
    public static IServiceCollection AddCommonServices(this IServiceCollection builder)
    {
        builder.AddSingleton<IEnvironmentVariableAccessor, EnvironmentVariableAccessor>();
        return builder;
    }
}