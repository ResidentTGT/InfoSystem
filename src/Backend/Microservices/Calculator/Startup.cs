using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Threading.Tasks;
using IdentityDatabase;


using IdentityDatabase.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Company.Common;
using Company.Calculator.Calculation;
using Microsoft.Extensions.Options;
using Company.Calculator;
using Company.Calculator.Options;
using Company.Common.Models.Identity;
using Company.Common.Options;

namespace Calculator
{
    public class Startup
    {
        public IConfiguration Configuration { get; }

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var connectionString = Configuration.GetConnectionString("CalculatorContext");

            services.AddEntityFrameworkNpgsql()
                .AddDbContext<CalculatorContext>(options => options.UseNpgsql(connectionString));      

            services.Configure<AttributesIdsOptions>(Configuration.GetSection("AttributesIds"));
            services.Configure<ListsIdsOption>(Configuration.GetSection("ListsIds"));
            services.Configure<CoefficientsOptions>(o =>
            {
                o.Bwp = double.Parse(Configuration.GetSection("Coefficients:Bwp").Value);
                o.Rrc = double.Parse(Configuration.GetSection("Coefficients:Rrc").Value);
            });

            services.AddMvc().AddJsonOptions(x => x.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore); ;
        }
        
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
