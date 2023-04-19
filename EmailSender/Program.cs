using MacroMail.DbAccess;
using MacroMail.Extensions;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddTemplateService();

var connectionString = builder.Configuration.GetConnectionString("DataContext");
builder.Services.AddDbContext<DataContext>(option =>
{
    option.UseNpgsql(connectionString);
    option.UseSnakeCaseNamingConvention();
    option.EnableSensitiveDataLogging();
});
builder.Services.AddDbContextFactory<DataContext>(option =>
{
    option.UseNpgsql(connectionString);
    option.UseSnakeCaseNamingConvention();
    option.EnableSensitiveDataLogging();
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();