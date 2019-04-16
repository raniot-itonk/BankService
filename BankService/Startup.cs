using BankService.Authorization;
using BankService.DB;
using BankService.OptionModels;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Swashbuckle.AspNetCore.Filters;
using Swashbuckle.AspNetCore.Swagger;

namespace BankService
{
    public class Startup
    {
        private readonly IHostingEnvironment _env;
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
            _env = env;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            // Disables auth when in Development
            //if (_env.IsDevelopment()) 
            //    services.AddMvc(opts =>{opts.Filters.Add(new AllowAnonymousFilter());})
            //        .SetCompatibilityVersion(CompatibilityVersion.Version_2_2);
            //else
                services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_2);


            services.Configure<Services>(Configuration.GetSection(nameof(Services)));
            

            // Register the Swagger generator, defining 1 or more Swagger documents
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new Info {Title = "My API", Version = "v1"});

                // Disable Swagger auth when in Development
                //if (_env.IsDevelopment()) return;
                c.AddSecurityDefinition("oauth2", new ApiKeyScheme
                {
                    Description =
                        "Standard Authorization header using the Bearer scheme. Example: \"bearer {token}\"",
                    In = "header",
                    Name = "Authorization",
                    Type = "apiKey"
                });
                c.OperationFilter<SecurityRequirementsOperationFilter>();
            });

            SetupDatabase(services);
            var authorizationService = services.BuildServiceProvider().GetService<IOptionsMonitor<Services>>()
                .CurrentValue.AuthorizationService;
            AddAuthenticationAndAuthorization(services, authorizationService);

            services.AddHealthChecks().AddDbContextCheck<BankingContext>(tags: new[] {"ready"});
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            app.UseAuthentication();

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            SetupReadyAndLiveHealthChecks(app);
            InitializeDatabase(app, env);

            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "My API V1");
            });

            app.UseHttpsRedirection();
            app.UseMvc();
        }

        private static void SetupReadyAndLiveHealthChecks(IApplicationBuilder app)
        {
            // The readiness check uses all registered checks with the 'ready' tag.
            app.UseHealthChecks("/health/ready", new HealthCheckOptions()
            {
                Predicate = (check) => check.Tags.Contains("ready"),
            });
            app.UseHealthChecks("/health/live", new HealthCheckOptions()
            {
                // Exclude all checks and return a 200-Ok.
                Predicate = (_) => false
            });
        }

        private static void InitializeDatabase(IApplicationBuilder app, IHostingEnvironment env)
        {
            using (var scope = app.ApplicationServices.GetService<IServiceScopeFactory>().CreateScope())
            {
                if (env.IsDevelopment())
                    scope.ServiceProvider.GetRequiredService<BankingContext>().Database.EnsureCreated();
                else
                    scope.ServiceProvider.GetRequiredService<BankingContext>().Database.Migrate();
            }
        }
        private void SetupDatabase(IServiceCollection services)
        {
            services.AddDbContext<BankingContext>
                (options => options.UseSqlServer(Configuration.GetConnectionString("BankingDatabase")));
        }

        private void AddAuthenticationAndAuthorization(IServiceCollection services,
            AuthorizationService authorizationService)
        {
            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.Authority = authorizationService.BaseAddress;
                    options.Audience = "BankingService";
                });

            services.AddAuthorization(options =>
            {
                options.AddPolicy("BankingService.UserActions", policy =>
                    policy.Requirements.Add(new HasScopeRequirement("BankingService.UserActions", authorizationService.BaseAddress)));
                options.AddPolicy("BankingService.broker&taxer", policy =>
                    policy.Requirements.Add(new HasScopeRequirement("BankingService.broker&taxer", authorizationService.BaseAddress)));
            });
            services.AddSingleton<IAuthorizationHandler, HasScopeHandler>();
            
        }
    }
}
