using Microsoft.Extensions.DependencyInjection;
using YJCabin.Application.Abstractions;
using YJCabin.Application.Services;

namespace YJCabin.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IProjectService, ProjectService>();
        services.AddScoped<IArticleService, ArticleService>();
        services.AddScoped<IAboutService, AboutService>();
        services.AddScoped<IContactService, ContactService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMediaService, MediaService>();
        services.AddScoped<ITagService, TagService>();
        return services;
    }
}
