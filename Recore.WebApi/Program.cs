using Serilog;
using Recore.Data.Contexts;
using Recore.Service.Helpers;
using Recore.WebApi.Extensions;
using Recore.WebApi.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.ApplicationModels;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add DbContext
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.Configure<EmailSettings>(builder.Configuration.GetSection("EmailSettings"));
// Add Custom services
builder.Services.AddServices();

builder.Services.ConfigureSwagger();

// JWT
builder.Services.AddJwt(builder.Configuration);

// Lowercase routing

builder.Services.AddControllers(options =>
{
    options.Conventions.Add(new RouteTokenTransformerConvention(new SlugifyParameterTransformer()));
});

// Logger
var logger = new LoggerConfiguration()
                    .ReadFrom.Configuration(builder.Configuration)
                    .Enrich.FromLogContext()
                    .CreateLogger();
builder.Logging.ClearProviders();
builder.Logging.AddSerilog(logger);

var app = builder.Build();

AppContext.SetSwitch("Npgsql.EnableLegacyTimestampBehavior", true);

PathHelper.WebRootPath = Path.GetFullPath("wwwroot");

PathHelper.CountryPath = Path.GetFullPath(builder.Configuration.GetValue<string>(("FilePath:CountryFilePaths")));
PathHelper.RegionPath = Path.GetFullPath(builder.Configuration.GetValue<string>(("FilePath:RegionFilePaths")));
PathHelper.DistrictPath = Path.GetFullPath(builder.Configuration.GetValue<string>(("FilePath:DictrictsFilePaths")));


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseAuthentication();

app.UseMiddleware<ExceptionHandlerMiddleware>();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
