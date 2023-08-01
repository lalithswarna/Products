using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Products.Data;
using Products.Interface;
using Products.Middleware;
using OpenQA.Selenium.DevTools;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddDbContext<ProductsContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("ProductsContext") ?? throw new InvalidOperationException("Connection string 'ProductsContext' not found.")));

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddControllers(options =>
{
    options.Filters.Add(typeof(NotFoundExceptionFilter));
});
builder.Services.AddLogging();
Serilog.Log.Logger = new LoggerConfiguration()
    .WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

builder.Services.AddApplicationInsightsTelemetry(builder.Configuration["ApplicationInsights:InstrumentationKey"]);
builder.Services.AddCors();
var app = builder.Build();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseApplicationInsightsRequestTelemetry();
app.UseCors(builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader());
app.UseHttpsRedirection();
app.UseExceptionMiddleware();
app.UseAuthorization();

app.MapControllers();

app.Run();

public class NotFoundExceptionFilter : IExceptionFilter
{
    public void OnException(ExceptionContext context)
    {
        if (context.Exception is NotFoundException)
        {
            context.Result = new NotFoundResult();
            context.ExceptionHandled = true;
        }
    }
}