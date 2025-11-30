using nhl_service_dotnet.Services;
using nhl_service_dotnet.Integrations;
using Microsoft.OpenApi.Models;
using nhl_service_dotnet;
using Microsoft.Extensions.Options;
using nhl_service_dotnet.Data;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);
var httpsPort = builder.Configuration.GetValue<int?>("ASPNETCORE_HTTPS_PORT");

// Add services to the container.
builder.Services.AddMemoryCache();
builder.Services.AddControllers();
builder.Services.AddOptions();
builder.Services.Configure<AppSettings>(builder.Configuration.GetSection("AppSettings"));
builder.Services.AddDbContext<NhlDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));
if (httpsPort.HasValue)
{
    builder.Services.AddHttpsRedirection(options => { options.HttpsPort = httpsPort.Value; });
}

builder.Services.AddScoped<ITeamService, TeamService>();
builder.Services.AddScoped<IPlayerService, PlayerService>();
builder.Services.AddScoped<IGameService, GameService>();
builder.Services.AddScoped<ITeamRosterService, TeamRosterService>();
builder.Services.AddSingleton<INhlClient, NhlClient>();
builder.Services.AddHttpClient("INhlClient");

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(s =>
{
    s.EnableAnnotations();
    s.AddSecurityDefinition("ApiKey", new Microsoft.OpenApi.Models.OpenApiSecurityScheme
    {
        Description = "API Key needed to access the endpoints. Enter the value for the configured header.",
        Name = builder.Configuration.GetSection("AppSettings")["ApiKeyName"] ?? "X-API-Key",
        In = Microsoft.OpenApi.Models.ParameterLocation.Header,
        Type = Microsoft.OpenApi.Models.SecuritySchemeType.ApiKey,
        Scheme = "ApiKeyScheme"
    });

    s.AddSecurityRequirement(new Microsoft.OpenApi.Models.OpenApiSecurityRequirement
    {
        {
            new Microsoft.OpenApi.Models.OpenApiSecurityScheme
            {
                Reference = new Microsoft.OpenApi.Models.OpenApiReference
                {
                    Type = Microsoft.OpenApi.Models.ReferenceType.SecurityScheme,
                    Id = "ApiKey"
                },
                In = Microsoft.OpenApi.Models.ParameterLocation.Header,
            },
            new List<string>()
        }
    });

    s.SwaggerDoc(
        "v1",
        new OpenApiInfo
        {
            Version = "v1",
            Title = "NHL service",
            Description = "Business service between free NHL api and NHL clients (235)"
        }
    );
});

var app = builder.Build();

// Apply migrations at startup
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<NhlDbContext>();
    dbContext.Database.Migrate();
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (httpsPort.HasValue)
{
    app.UseHttpsRedirection();
}

app.UseExceptionHandler("/error");

app.Use(async (context, next) =>
{
    var options = context.RequestServices.GetRequiredService<IOptions<AppSettings>>().Value;

    if (!string.IsNullOrWhiteSpace(options.ApiKeyValue))
    {
        if (!context.Request.Headers.TryGetValue(options.ApiKeyName, out var provided) || provided != options.ApiKeyValue)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }
    }

    await next();
});

app.UseAuthorization();

app.MapControllers();

app.Run();
