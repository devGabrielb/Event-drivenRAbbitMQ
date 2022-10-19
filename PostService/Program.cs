using Microsoft.EntityFrameworkCore;
using PostService.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddDbContext<PostServiceContext>(options =>
         options.UseSqlite(@"Data Source=post.db"));



var app = builder.Build();

var scope = app.Services.CreateScope();
var dbContext = scope.ServiceProvider.GetService<PostServiceContext>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    dbContext?.Database.EnsureCreated();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
