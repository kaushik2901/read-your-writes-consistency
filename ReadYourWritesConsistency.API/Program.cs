using ReadYourWritesConsistency.API.Endpoints.V1;
using ReadYourWritesConsistency.API.Endpoints.V2;
using ReadYourWritesConsistency.API.Middlewares;
using ReadYourWritesConsistency.API.Persistence;
using ReadYourWritesConsistency.API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerGen();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IReadDbContext, ReadDbContext>();
builder.Services.AddScoped<IReadWriteDbContext, ReadWriteDbContext>();
builder.Services.AddScoped<IAppDbContextFactory, AppDbContextFactory>();
builder.Services.AddScoped<IDbIntentAccessor, DbIntentAccessor>();
builder.Services.AddScoped<ICurrentUserAccessor, CurrentUserAccessor>();

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