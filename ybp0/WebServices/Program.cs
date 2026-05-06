using ViewModels.Api;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddYbp0ApiDatabase(builder.Environment.ContentRootPath);

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
        policy.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
});

var app = builder.Build();

app.Services.EnsureYbp0ApiDatabaseCreated();

app.UseSwagger();
app.UseSwaggerUI(options =>
{
    options.RoutePrefix = string.Empty;
});
app.UseCors("AllowAll");
app.MapControllers();
app.Run();
