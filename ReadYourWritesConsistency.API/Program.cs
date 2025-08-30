using ReadYourWritesConsistency.API.Endpoints.V1;
using ReadYourWritesConsistency.API.Endpoints.V2;
using ReadYourWritesConsistency.API.Middlewares;
using ReadYourWritesConsistency.API.Models;
using ReadYourWritesConsistency.API.Services;

var builder = WebApplication.CreateBuilder(args);

var readCnn = builder.Configuration.GetConnectionString("Read");
var readWriteCnn = builder.Configuration.GetConnectionString("ReadWrite");

builder.Services.AddSingleton<IReadDbContext>(_ => new ReadDbContext(readCnn!));
builder.Services.AddSingleton<IReadWriteDbContext>(_ => new ReadWriteDbContext(readWriteCnn!));
builder.Services.AddScoped<IDbIntentAccessor, DbIntentAccessor>();
builder.Services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseMiddleware<ExceptionHandlingMiddleware>();
app.UseMiddleware<DbIntentMiddleware>();

app.MapGet("/health", () => Results.Ok(new { status = "ok" }));

app.MapV1Endpoints();
app.MapV2Endpoints();

app.Run();