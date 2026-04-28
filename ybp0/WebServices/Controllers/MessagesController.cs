using DataBase.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Models;
using ViewModels.Api;

namespace WebServices.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagesController : ControllerBase
{
    private readonly IMessageRepository _messageRepository;
    private readonly IUserRepository _userRepository;

    public MessagesController(IMessageRepository messageRepository, IUserRepository userRepository)
    {
        _messageRepository = messageRepository;
        _userRepository = userRepository;
    }

    [HttpGet("conversation/{userIdA:int}/{userIdB:int}")]
    public async Task<ActionResult<IEnumerable<MessageViewModel>>> GetConversation(int userIdA, int userIdB)
    {
        IEnumerable<Message> messages = await _messageRepository.GetConversationAsync(userIdA, userIdB);
        return Ok(messages.Select(ApiMappings.ToMessageViewModel));
    }

    [HttpGet("contacts/{userId:int}")]
    public async Task<ActionResult<IEnumerable<int>>> GetContacts(int userId)
    {
        IEnumerable<int> contactIds = await _messageRepository.GetChatContactIdsAsync(userId);
        return Ok(contactIds);
    }

    [HttpPost]
    public async Task<ActionResult<MessageViewModel>> Post([FromBody] CreateMessageRequest request)
    {
        if (await _userRepository.GetByIdAsync(request.SenderId) is null || await _userRepository.GetByIdAsync(request.RecipientId) is null)
        {
            return BadRequest("Sender or recipient does not exist.");
        }

        var message = new Message
        {
            SenderId = request.SenderId,
            RecipientId = request.RecipientId,
            MessageText = request.MessageText,
            SentAt = DateTime.UtcNow
        };

        await _messageRepository.AddAsync(message);
        Message? created = await _messageRepository.GetByIdAsync(message.Id);
        return CreatedAtAction(nameof(GetConversation), new { userIdA = request.SenderId, userIdB = request.RecipientId }, ApiMappings.ToMessageViewModel(created ?? message));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        Message? message = await _messageRepository.GetByIdAsync(id);
        if (message is null)
        {
            return NotFound();
        }

        await _messageRepository.DeleteAsync(id);
        return NoContent();
    }
}
