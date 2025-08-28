using ReadYourWritesConsistency.API.Services;
using ReadYourWritesConsistency.API.Endpoints.V1;
using ReadYourWritesConsistency.API.Endpoints.V2;
using ReadYourWritesConsistency.API.Middlewares;
using ReadYourWritesConsistency.API.Models;

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

// V1 endpoints
app.MapV1Dashboard();
app.MapV1Projects();
app.MapV1ProjectWrites();
app.MapV1Tasks();
app.MapV1Users();

// V2 scaffold
app.MapV2Scaffold();

app.Run();