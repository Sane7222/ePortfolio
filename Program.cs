using Microsoft.Extensions.Configuration.AzureAppConfiguration;

namespace ePortfolio {
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddRazorPages();
            builder.Services.AddMemoryCache();
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddSession();

            if (!builder.Environment.IsDevelopment()) {
                string? connectionString = Environment.GetEnvironmentVariable("AZ_APP_CONFIG");

                builder.Configuration.AddAzureAppConfiguration(options => {
                    options.Connect(connectionString)
                        .Select("ePortfolio:*", LabelFilter.Null)
                        .ConfigureRefresh(refreshOptions => {
                            refreshOptions.Register("ePortfolio:Settings:Sentinel", refreshAll: true)
                                .SetCacheExpiration(TimeSpan.FromDays(1));
                        });
                });

                builder.Services.AddAzureAppConfiguration();
            }

            builder.Services.Configure<Settings>(builder.Configuration.GetSection("ePortfolio:Settings"));

            var app = builder.Build();

            if (!app.Environment.IsDevelopment()) {
                app.UseExceptionHandler("/Error");
                app.UseHsts(); // The default HSTS value is 30 days. https://aka.ms/aspnetcore-hsts.
                app.UseAzureAppConfiguration();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseSession();

            app.Use(async (context, next) => {
                context.Session.SetString("Theme", context.Request.Cookies["Theme"] ?? "light");
                await next(context);
            });

            app.MapRazorPages();

            app.Run();
        }
    }
}