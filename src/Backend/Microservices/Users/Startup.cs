using IdentityDatabase;

using IdentityDatabase.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using System.IO;
using Company.Common.Models.Identity;

namespace Company.Users
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var identityConfig = Common.GlobalConfig.GetConfigByName("dbsettings.json");

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<IdentityContext>(options => options.UseNpgsql(identityConfig.GetConnectionString("identityContext")))
                .AddTransient<UserManager<User>>();
               // .AddTransient<IPermissionDbContext>();

            // configure identity options
            var builder = services.AddIdentityCore<User>(o => o.Password = JwtOptions.PasswordOptions);
            builder = new IdentityBuilder(builder.UserType, typeof(Role), builder.Services);
            builder.AddEntityFrameworkStores<IdentityContext>().AddDefaultTokenProviders();

            services.AddAuthentication(options => options.DefaultChallengeScheme = options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(configureOptions =>
            {
                configureOptions.TokenValidationParameters = JwtOptions.TokenValidationParameters;
                configureOptions.SaveToken = true;
            });

            services.AddAuthorization(options =>
            {
                foreach (var policy in JwtOptions.Policies)
                    options.AddPolicy(policy.Key, pol =>
                    {
                        foreach (var claim in policy.Value)
                            pol.RequireClaim(claim.Key, claim.Value);
                    });
            });

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiddleware<UserMiddleware>();

            app.UseMvc();
        }
    }
}
