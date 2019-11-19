using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Company.Common;
using Microsoft.AspNetCore.Http.Features;
using Company.Common.Options;
using Company.Pim.Client.v2;
using Company.Pim.Helpers.v2;
using Company.Pim.Options;

namespace Company.Pim
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("PimContext");

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<PimContext>(options => options.UseNpgsql(connectionString))
                .AddTransient<SkuGenerator>()
                .AddScoped<IFileStorageMsCommunicator, HttpFileStorageMsCommunicator>()
                .AddScoped<ISeasonsMsCommunicator, HttpSeasonsMsCommunicator>()
                .AddScoped<IWebApiCommunicator, HttpWebApiCommunicator>()
                .AddScoped<SearchHelper>()
                .AddSingleton<TransformModelHelpers>()
                .AddSingleton<TreeObjectHelper>();

            services.Configure<CurrentSeasonOptions>(o =>
            {
                o.Id = int.Parse(Configuration.GetSection("CurrentSeason:Id").Value);
            });


            services.Configure<ExportAttributesIdsOptions>(Configuration.GetSection("ExportAttributesIds"));
            services.Configure<AttributesIdsOptions>(Configuration.GetSection("AttributesIds"));
            services.Configure<List<ExportTemplate>>(etl => etl.AddRange(Configuration.GetSection("Templates").Get<List<ExportTemplate>>()));

            services.AddMvc().AddXmlSerializerFormatters().AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore);
            services.Configure<FormOptions>(x =>
            {
                x.ValueLengthLimit = int.MaxValue;
                x.MultipartBodyLengthLimit = int.MaxValue; // In case of multipart
            });
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            var t = Configuration.GetSection("Ips:1cIp").Value;

            app.MapWhen(context =>
                {
                    Console.WriteLine("==========================================");
                    Console.WriteLine($"Input Ip {context.Request.Host.Host}");
                    Console.WriteLine($"Remote Ip {context.Connection.RemoteIpAddress}");
                    Console.WriteLine("(==========================================");
                    return context.Request.Host.Host == Configuration.GetSection("Ips:1cIp").Value && context.Request.Path.StartsWithSegments("/trademanagement");
                },
                a => a.UseMvc());

            app.UseMvc();
        }
    }
}
