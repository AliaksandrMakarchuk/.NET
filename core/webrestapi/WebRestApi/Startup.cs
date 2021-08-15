using System.IO;
using System.Reflection;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using WebRestApi.DataAccess;
using WebRestApi.DataAccess.Repository;
using WebRestApi.Logger;
using WebRestApi.Models;
using WebRestApi.Service;
using WebRestApi.Service.Repository;

#pragma warning disable 1591
namespace WebRestApi
{
    public class Startup
    {
        public IConfigurationRoot Configuration { get; set; }

        public Startup(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(Path.Combine(env.ContentRootPath))
                .AddJsonFile("appsettings.json", optional : true, reloadOnChange : true);

            Configuration = builder.Build();
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvcCore(options => options.EnableEndpointRouting = false)
                .AddAuthorization() // Note - this is on the IMvcBuilder, not the service collection
                .AddNewtonsoftJson(options => options.SerializerSettings.ReferenceLoopHandling = Newtonsoft.Json.ReferenceLoopHandling.Ignore)
                .AddJsonOptions(options => options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase)
                .SetCompatibilityVersion(CompatibilityVersion.Version_3_0)
                .AddApiExplorer();

            services.AddScoped<AbstractDbContext, WebRestApiContext>();
            services.AddDbContext<AbstractDbContext>();
            services.AddScoped<UserRepositoryBase, UserRepository>();
            services.AddScoped<RoleRepositoryBase, RoleRepository>();
            services.AddScoped<MessageRepositoryBase, MessageRepository>();
            services.AddScoped<CommentRepositoryBase, CommentRepository>();
            services.AddScoped<IDataService, DataService>();

            services.AddSwaggerGen();

            services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.RequireHttpsMetadata = false;
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        // укзывает, будет ли валидироваться издатель при валидации токена
                        ValidateIssuer = true,
                        // строка, представляющая издателя
                        ValidIssuer = AuthOptions.ISSUER,

                        // будет ли валидироваться потребитель токена
                        ValidateAudience = true,
                        // установка потребителя токена
                        ValidAudience = AuthOptions.AUDIENCE,
                        // будет ли валидироваться время существования
                        ValidateLifetime = true,

                        // установка ключа безопасности
                        IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                        // валидация ключа безопасности
                        ValidateIssuerSigningKey = true,
                    };
                });
            services.AddAuthorization();

            services.ConfigureSwaggerGen(setup =>
            {
                var basePath = System.AppDomain.CurrentDomain.BaseDirectory;
                var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                var xmlPath = Path.Combine(basePath, xmlFile);
                setup.IncludeXmlComments(xmlPath);
            });
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env, ILoggerFactory loggerFactory)
        {
            if (env.EnvironmentName == Environments.Development)
            {
                app.UseDeveloperExceptionPage();
            }

            loggerFactory.AddProvider(
                new FileLoggerProvider(
                    Configuration.GetSection("Logger")
                    .Get<FileLoggerOptions>()
                )
            );

            app.UseStaticFiles();

            app.UseAuthentication();
            app.UseAuthorization();

            app.UseSwagger();

            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("swagger/v1/swagger.json", "Web Rest API v1");
                c.RoutePrefix = string.Empty;
            });

            app.UseMvc();
        }
    }
}