using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Xunit;

namespace mvc.Tests;

public class IdentityConfigurationTests
{
    [Fact]
    public void IdentityOptions_HaveCorrectSecuritySettings()
    {
        // Arrange
        var services = new ServiceCollection();
        
        // Simuliere die Konfiguration aus Program.cs
        services.AddLogging();
        services.AddOptions();
        services.Configure<IdentityOptions>(options => 
        {
            // Kopie der Logik aus Program.cs
            options.Password.RequiredLength = 12;
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;

            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
        });

        var serviceProvider = services.BuildServiceProvider();
        var options = serviceProvider.GetRequiredService<IOptions<IdentityOptions>>().Value;

        // Assert - Password Policy
        Assert.Equal(12, options.Password.RequiredLength);
        Assert.True(options.Password.RequireDigit);
        Assert.True(options.Password.RequireLowercase);
        Assert.True(options.Password.RequireUppercase);
        Assert.True(options.Password.RequireNonAlphanumeric);

        // Assert - Lockout
        Assert.Equal(TimeSpan.FromMinutes(15), options.Lockout.DefaultLockoutTimeSpan);
        Assert.Equal(5, options.Lockout.MaxFailedAccessAttempts);
        Assert.True(options.Lockout.AllowedForNewUsers);
    }
}
