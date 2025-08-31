using ReadYourWritesConsistency.API.Endpoints.V1;
using ReadYourWritesConsistency.API.Endpoints.V2;
using ReadYourWritesConsistency.API.Middlewares;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;

var builder = WebApplication.CreateSlimBuilder(args);

builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();
builder.Services.AddScoped<IDbIntentAccessor, DbIntentAccessor>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<DbIntentMiddleware>();

app.MapV1Endpoints();
app.MapV2Endpoints();

await app.RunAsync();