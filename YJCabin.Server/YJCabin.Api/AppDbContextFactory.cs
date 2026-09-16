using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using YJCabin.Infrastructure.Persistence;

namespace YJCabin.Api;

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: true)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        var options = new DbContextOptionsBuilder<AppDbContext>();
        options.UseNpgsql(configuration.GetConnectionString("Default")
                          ?? "Host=localhost;Port=5432;Database=yjcabin;Username=yjcabin;Password=yjcabin");
        return new AppDbContext(options.Options);
    }
}
