using System.Text;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using PostService.Data;
using PostService.Entities;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

static void ListenForIntegrationEvents()
{
    var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672, UserName = "guest", Password = "guest" };
    var connection = factory.CreateConnection();
    var channel = connection.CreateModel();
    var consumer = new EventingBasicConsumer(channel);

    channel.ExchangeDeclare(exchange: "user.postservice", type: ExchangeType.Fanout);

    var queueName = channel.QueueDeclare().QueueName;
    channel.QueueBind(queue: queueName,
                                  exchange: "user.postservice",
                                  routingKey: "user.add");
    channel.QueueBind(queue: queueName,
                                  exchange: "user.postservice",
                                  routingKey: "user.update");
    consumer.Received += (model, ea) =>
    {
        var contextOptions = new DbContextOptionsBuilder<PostServiceContext>()
            .UseSqlite(@"Data Source=post.db")
            .Options;
        var dbContext = new PostServiceContext(contextOptions);

        var body = ea.Body.ToArray();
        var message = Encoding.UTF8.GetString(body);
        Console.WriteLine(" [x] Received {0}, RouterKey {1}", message, ea.RoutingKey);

        var data = JsonSerializer.Deserialize<User>(message);

        var type = ea.RoutingKey;
        if (type == "user.add")
        {
            dbContext.User.Add(new User()
            {
                ID = data.ID,
                Name = data.Name
            });
            dbContext.SaveChanges();
        }
        else if (type == "user.update")
        {

            var user = dbContext.User.First(a => a.ID == data.ID);
            user.Name = data.Name;
            dbContext.SaveChanges();
        }
    };
    channel.BasicConsume(queue: queueName,
                             autoAck: true,
                             consumer: consumer);
}
ListenForIntegrationEvents();

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

