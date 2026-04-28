using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public UsersController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<UserViewModel>>> Get()
    {
        var users = await _userRepository.GetAllAsync();
        return Ok(users.Select(ApiMappings.ToUserViewModel));
    }

    [HttpGet("{id:int}")]
    public async Task<ActionResult<UserViewModel>> GetById(int id)
    {
        Models.User? user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(ApiMappings.ToUserViewModel(user));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Models.User? user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            return NotFound();
        }

        await _userRepository.DeleteAsync(id);
        return NoContent();
    }
}
