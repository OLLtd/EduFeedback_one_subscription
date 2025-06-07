using Microsoft.Owin;
using Owin;
using SimpleInjector.Integration.Web.Mvc;
using SimpleInjector.Integration.Web;
using SimpleInjector;
using System.Reflection;
using System.Web.Mvc;
using System.Web.Services.Description;
using EduFeedback.Web.Services;
using Hangfire;
using SimpleInjector.Lifestyles;
using System.Configuration;

[assembly: OwinStartupAttribute(typeof(EduFeedback.Web.Startup))]
namespace EduFeedback.Web
{
    public partial class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            ConfigureAuth(app);
            //ConfigureDependencyInjection();

            // Get the connection string from web.config
            var connectionString = ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString;

            // Configure Hangfire to use SQL Server storage
            //GlobalConfiguration.Configuration                .UseSqlServerStorage("Data Source=213.136.68.3;Initial Catalog=EduFeedback_Uat;Integrated Security=false; User id=usergcse02; password=gcse@db#002#");

            GlobalConfiguration.Configuration
                .UseSqlServerStorage(connectionString);
            //, new SqlServerStorageOptions
            //{
            //    QueuePollInterval = TimeSpan.FromSeconds(15),
            //    SlidingInvisibilityTimeout = TimeSpan.FromMinutes(5),
            //    CommandBatchMaxTimeout = TimeSpan.FromMinutes(5),
            //    UseRecommendedIsolationLevel = true,
            //    UsePageLocksOnDequeue = true,
            //    DisableGlobalLocks = true
            //});

            // Set the global retry attempts
            GlobalJobFilters.Filters.Add(new AutomaticRetryAttribute { Attempts = 3 });
            // Register the custom filter
            GlobalJobFilters.Filters.Add(new LogJobFailureAttribute());
            // Register the notification filter
            GlobalJobFilters.Filters.Add(new NotifyJobFailureAttribute());


            // Start Hangfire server
            app.UseHangfireServer();
            app.UseHangfireDashboard();
            // Register services
            var container = new Container();
            container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();
            // Register your types, for instance:
            container.Register<BackgroundJobService>(Lifestyle.Scoped);
            app.Use(async (context, next) =>
            {
                using (AsyncScopedLifestyle.BeginScope(container))
                {
                    await next.Invoke();
                }
            });
        }

    }
}
