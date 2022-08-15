using DFTelegram.Helper;
using DFTelegram.BackgroupTaskService;
using DFTelegram.BackgroupTaskService.QueueService;
using Starksoft.Net.Proxy;
using SqlSugar;
using DFTelegram.Services;
using NLog;
using NLog.Web;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Text;
using Microsoft.IdentityModel.Tokens;

var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Logging.ClearProviders();
    builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
    builder.Host.UseNLog();
    // Add services to the container.
    builder.Services.AddCors(setupAction =>
    {
        setupAction.AddPolicy("cors", configurePolicy =>
        {
            configurePolicy.WithOrigins("http://localhost:8080")
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
        });
    });
    builder.Services.AddControllers();
    // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
    builder.Services.AddEndpointsApiExplorer();
    builder.Services.AddSwaggerGen(setupAction =>
            {
                setupAction.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
                {
                    Description = "JWT Authorization header using the Bearer scheme.\r\nEnter 'Bearer' [space] and then your token in the text input below.\r\nExample: 'Bearer 12345abcdef'",
                    Name = "Authorization",
                    In = ParameterLocation.Header,
                    Type = SecuritySchemeType.ApiKey
                });
                setupAction.OperationFilter<AddResponseHeadersFilter>();
                setupAction.OperationFilter<AppendAuthorizeToSummaryOperationFilter>();
                setupAction.OperationFilter<SecurityRequirementsOperationFilter>();
            });
    builder.Services.AddSingleton<TLConfigService>();
    builder.Services.AddSingleton(new AppsettingsHelper(builder.Configuration));
    builder.Services.AddSingleton(new HashHelper());
    builder.Services.AddSingleton<IQueueBase<TL.Document>, QueueBase<TL.Document>>();
    builder.Services.AddSingleton<IQueueBase<TL.Photo>, QueueBase<TL.Photo>>();
    builder.Services.AddSingleton<JwtTokenHelper>();
    builder.Services.AddSingleton<UserService>();
    builder.Services.AddSingleton<WTelegram.Client>(m =>
    {
#nullable disable
        TLConfigService tlConfigService = m.GetService<TLConfigService>();
        WTelegram.Client client = new WTelegram.Client(tlConfigService.Config);
#nullable restore
        if (bool.Parse(AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "EnableProxy" })))
        {
#pragma warning disable CS1998
            client.TcpHandler = async (address, port) =>
            {
                var proxy = new Socks5ProxyClient(
                    AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "ProxyHost" }),
                int.Parse(AppsettingsHelper.app(new string[] { "RunConfig", "Proxy", "ProxyPort" })));
                return proxy.CreateConnection(address, port);
            };
#pragma warning restore CS1998
        }
        return client;
    });

    SqlSugarScope sqlSugar = new SqlSugarScope(new ConnectionConfig()
    {
        DbType = SqlSugar.DbType.Sqlite,
        ConnectionString = "DataSource=" + Path.Combine(
            AppsettingsHelper.app(new string[] { "RunConfig", "SQLitePath" }),
            "DFTelegram.sqlite"),
        IsAutoCloseConnection = true,
    });
    builder.Services.AddSingleton<DownloadsInfoService>();
    builder.Services.AddSingleton<ISqlSugarClient>(sqlSugar);
    builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Bearer";
                options.DefaultChallengeScheme = "Bearer";
            }).AddJwtBearer(options =>
            {
                byte[] bytes = Encoding.UTF8.GetBytes(AppsettingsHelper.app("JWT", "key"));
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = AppsettingsHelper.app("JWT", "Issuer"),
                    ValidAudience = AppsettingsHelper.app("JWT", "Audience"),
                    IssuerSigningKey = new SymmetricSecurityKey(bytes)
                };
            });

    // builder.Services.AddHostedService<ListenTelegramService>();

    var app = builder.Build();

    // Configure the HTTP request pipeline.
    // if (app.Environment.IsDevelopment())
    // {
    app.UseSwagger();
    app.UseSwaggerUI();
    // }

    app.UseCors("cors");

    // app.UseHttpsRedirection();

    app.UseAuthentication();

    app.UseAuthorization();

    app.MapControllers();

    app.Run();
}
catch (Exception exception)
{
    logger.Error(exception, "Stopped program because of exception");
}
finally
{
    NLog.LogManager.Shutdown();
}
