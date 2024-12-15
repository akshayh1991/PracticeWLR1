using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using SecMan.BL;
using SecMan.BL.Common;
using SecMan.Data;
using SecMan.Data.DAL;
using SecMan.Data.Repository;
using SecMan.Data.SQLCipher;
using SecMan.Interfaces.BL;
using SecMan.Interfaces.DAL;
using SecMan.Model;
using Serilog;
using Swashbuckle.AspNetCore.SwaggerUI;
using System.IO.Abstractions;
using System.Net;
using System.Reflection;
using UserAccessManagement.Filters;
using UserAccessManagement.Handler;
using UserAccessManagement.Middleware;
using UserAccessManagement.Swagger;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration) // Read settings from appsettings.json
    .Enrich.FromLogContext()
    //.WriteTo.Console()
    // .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

// Add Serilog to the logging pipeline
builder.Host.UseSerilog();


builder.Services.AddDbContext<Db>(options =>
{
    var databaseFile = Path.Combine(builder.Configuration.GetConnectionString("DefaultConnection"), "SecMan.db");
    var connectionString = new SqliteConnectionStringBuilder
    {
        DataSource = databaseFile,
        Password = builder.Configuration.GetSection("DBPassword").Value,
    }.ToString();

    options.UseSqlite(connectionString);

#if DEBUG
    options.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddDebug()));
    options.EnableSensitiveDataLogging(); // only for debugging builds
#endif
});

_ = new SecManDb();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll",
        builder =>
        {
            builder.AllowAnyOrigin()
                   .AllowAnyHeader()
                   .AllowAnyMethod()
                   .WithExposedHeaders(ResponseHeaders.TotalCount, ResponseHeaders.FieldRevert, ResponseHeaders.ObjectRevert);
        });
});

builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));

builder.Services.Configure<JwtTokenOptions>(builder.Configuration.GetSection(JwtTokenOptions.JWTTokenValue));

builder.Services.AddHttpContextAccessor();

builder.Services.AddControllers(options =>
{
    options.Filters.Add(new ModelValidationActionFilter());
})
    .AddNewtonsoftJson(options =>
    {
        options.SerializerSettings.DateFormatString = "yyyy'-'MM'-'dd'T'HH':'mm':'ssZ";
        options.SerializerSettings.NullValueHandling = Newtonsoft.Json.NullValueHandling.Ignore;
    });
builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.SuppressModelStateInvalidFilter = true;
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("1.0.0", new OpenApiInfo
    {
        Title = "User Access Manager API",
        Version = "1.0.0",
        Description = "Watlow NextGen EPM Suite will onboard its first default application User Access Manager (UAM). UAM will manage Zones, Users, Roles, Devices and System Policies."
    });
    c.AddSecurityDefinition("Cookie", new OpenApiSecurityScheme
    {
        In = ParameterLocation.Cookie,
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey
    });
    c.OperationFilter<AuthOperationFilter>();

    List<OpenApiServer> servers = new List<OpenApiServer>();
    builder.Configuration.GetSection("Servers").Bind(servers);

    servers.ForEach(x => c.AddServer(x));

    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
    c.MapType<Languages>(() => new OpenApiSchema
    {
        Type = "string",
        Enum = Enum.GetNames(typeof(Languages))
                    .Select(n => new OpenApiString(n))
                    .Cast<IOpenApiAny>()
                    .ToList()
    });
});

IRsaKeysBL RSAKeysBL = new RsaKeysBL();

builder.Services.AddSingleton<IRsaKeysBL>(RSAKeysBL);

builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = "Auth-Cookie";
        options.Cookie.HttpOnly = false;
        options.Cookie.SameSite = SameSiteMode.None;
        options.Cookie.SecurePolicy = CookieSecurePolicy.Always;
        options.Events = new CookieAuthenticationEvents
        {
            OnRedirectToLogin = context =>
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                Unauthorized jsonResponse = new Unauthorized
                {
                    Type = ResponseConstants.GetTypeUrl(context.HttpContext, nameof(HttpStatusCode.Unauthorized)),
                    Title = nameof(HttpStatusCode.Unauthorized),
                    Status = HttpStatusCode.Unauthorized,
                    Detail = ResponseConstants.MissingSessionCookie
                };
                context.Response.WriteAsJsonAsync(jsonResponse).Wait();
                return Task.CompletedTask;
            },
            OnRedirectToAccessDenied = context =>
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                Unauthorized jsonResponse = new Unauthorized
                {
                    Type = ResponseConstants.GetTypeUrl(context.HttpContext, nameof(HttpStatusCode.Forbidden)),
                    Title = nameof(HttpStatusCode.Forbidden),
                    Status = HttpStatusCode.Forbidden,
                    Detail = ResponseConstants.PermissionDenied
                };
                context.Response.WriteAsJsonAsync(jsonResponse).Wait();
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddSingleton<IFileSystem, FileSystem>();

builder.Services.AddScoped<IRoleBL, RoleBL>();
builder.Services.AddScoped<IDeviceBL, DeviceBL>();
builder.Services.AddScoped<ISystemFeatureBL, SystemFeatureBL>();
builder.Services.AddScoped<IUserBL, UserBL>();
builder.Services.AddScoped<IPasswordBl, PasswordBL>();
builder.Services.AddScoped<ISendingEmail, SendingEmail>();
builder.Services.AddScoped<IAuthBL, UserBL>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IDashboardBL, DashboardBL>();
builder.Services.AddScoped<ModelValidationActionFilter>();
builder.Services.AddScoped<IEncryptionDecryption, EncryptionDecryption>();
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IApplicationLauncherBL, ApplicationLauncherBL>();
builder.Services.AddScoped<IReviewerBl, ReviewerBl>();
builder.Services.AddScoped<IPendingChangesManager, PendingChangesManager>();
builder.Services.AddScoped<ISignatureBL, SignatureBL>();

builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(InfoLogCommandHandler).Assembly));
builder.Services.AddSwaggerGen(c =>
{
    string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});
WebApplication app = builder.Build();
app.UseCors("AllowAll");

app.UseMiddleware<ExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/1.0.0/swagger.json", "User Access Manager API 1.0.0");
        c.DocExpansion(DocExpansion.List);
        c.DefaultModelsExpandDepth(0);
    });
}

app.UseAuthentication();

app.UseMiddleware<ValidateSessionMiddleware>();

app.UseAuthorization();

app.MapControllers();

await app.RunAsync();
