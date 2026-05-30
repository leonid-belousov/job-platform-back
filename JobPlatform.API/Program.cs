using System.Reflection;
using JobPlatform.API.Extensions;
using JobPlatform.API.Middleware;
using JobPlatform.API.Security;
using JobPlatform.BLL;
using JobPlatform.DAL.Seeds;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.OpenApi;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.WebHost.ConfigureKestrel(serverOptions => { serverOptions.Limits.MaxRequestBodySize = long.MaxValue; });

builder.Services.AddRouting(p =>
{
    p.LowercaseUrls = true;
    p.LowercaseQueryStrings = true;
});

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor
                               | ForwardedHeaders.XForwardedProto
                               | ForwardedHeaders.XForwardedHost;
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddRecruitmentSwagger();
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();
builder.Services.AddCurrentUser();
builder.Services.AddBLLServiceCollections(builder.Configuration);
builder.Services.AddSingleton<IAuthorizationHandler, PermissionAuthorizationHandler>();
builder.Services.AddRecruitmentCors(builder.Configuration);
builder.Services.AddRecruitmentRateLimiting(builder.Configuration);
builder.Services.AddRecruitmentHealthChecks();
builder.Services.AddAuthorization(options => options.AddRecruitmentPolicies());

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}
app.UseExceptionHandling();
app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.SwaggerEndpoint("/swagger/v1/swagger.json", "Recruitment Platform API v1");
    options.DisplayRequestDuration();
});

app.UseHttpsRedirection();
app.UseCors(CorsExtensions.PolicyName);
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapRecruitmentHealthChecks();

if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();

    var roleSeeder = scope.ServiceProvider.GetRequiredService<RoleSeeder>();
    var dictionarySeeder = scope.ServiceProvider.GetRequiredService<DictionarySeeder>();

    await roleSeeder.SeedAsync();
    await dictionarySeeder.SeedAsync();
}

app.Run();

public partial class Program { }
