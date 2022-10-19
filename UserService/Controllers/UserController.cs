using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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
        return NoContent();
    }

    [HttpPost]
    public async Task<ActionResult> PostUser(User user)
    {

        _context.User.Add(user);
        await _context.SaveChangesAsync();
        return CreatedAtAction("GetUsers", new { id = user.ID }, user);
    }

}
