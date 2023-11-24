using Recore.Service.Helpers;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace Recore.Shared.Extensions;

public static class HttpContextExtensions
{
    public static void InitAccessor(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();

        HttpContextHelper.Accessor = scope.ServiceProvider.GetRequiredService<IHttpContextAccessor>();
    }
}
