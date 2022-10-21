using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RabbitMQ.Client;
using UserService.Data;
using UserService.Entities;

namespace UserService.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController : ControllerBase
{
    private readonly UserServiceContext _context;

    public UserController(UserServiceContext context)
    {
        _context = context;
    }



    [HttpGet]
    public async Task<ActionResult<IEnumerable<User>>> GetUsers()
    {
        var result = await _context.User.ToListAsync();
        return Ok(result);
    }

    [HttpPut("{id:int}")]
    public async Task<ActionResult> PutUser(int id, User user)
    {
        if (id != user.ID)
        {
            return BadRequest();
        }
        _context.Entry(user).State = EntityState.Modified;
        await _context.SaveChangesAsync();

        var data = JsonSerializer.Serialize(new
        {
            user.ID,
            user.Name
        });

        PublishToMessageQueue("user.update", data);

        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> PostUser(User user)
    {

        _context.User.Add(user);
        await _context.SaveChangesAsync();

        var data = JsonSerializer.Serialize(new
        {
            id = user.ID,
            newname = user.Name
        });

        PublishToMessageQueue("user.add", data);

        return CreatedAtAction("GetUsers", new { id = user.ID }, user);
    }


    private static void PublishToMessageQueue(string integrationEvent, string eventData)
    {
        // TOOO: Reuse and close connections and channel, etc, 
        var factory = new ConnectionFactory() { HostName = "rabbitmq", Port = 5672, UserName = "guest", Password = "guest" };
        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        channel.ExchangeDeclare(exchange: "user.postservice", type: ExchangeType.Fanout);

        var body = Encoding.UTF8.GetBytes(eventData);
        var properties = channel.CreateBasicProperties();
        channel.BasicPublish(exchange: "user.postservice",
                             routingKey: integrationEvent,
                             basicProperties: properties,
                             body: body);
        Console.WriteLine(" [x] Sent {0}", body);

    }

}
