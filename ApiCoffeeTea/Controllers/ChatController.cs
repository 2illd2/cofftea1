using ApiCoffeeTea.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace ApiCoffeeTea.Controllers;

[ApiController]
[Route("api/chat")]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly AppDbContext _db;
    public ChatController(AppDbContext db) => _db = db;
    private static DateTime NowDb()
    => DateTime.SpecifyKind(DateTime.UtcNow, DateTimeKind.Unspecified);
    private int? GetUserId()
    {
        var c = User.FindFirst("UserId") ?? User.FindFirst(ClaimTypes.NameIdentifier);
        return int.TryParse(c?.Value, out var id) ? id : (int?)null;
    }

    private string GetRole()
    {
        var r = User.FindFirst(ClaimTypes.Role)?.Value
                ?? User.FindFirst("role")?.Value
                ?? User.FindFirst("Role")?.Value;
        return string.IsNullOrWhiteSpace(r) ? "client" : r.ToLowerInvariant();
    }

    public class SendMessageDto
    {
        public int ThreadId { get; set; }
        public string Text { get; set; } = "";
    }

    [HttpGet("thread")]
    public async Task<ActionResult<object>> GetThread()
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();

        var role = GetRole();
        chat_thread? thread = null;

        if (role == "consultant")
        {
            // уже закреплённый
            thread = await _db.chat_threads
                .Include(t => t.chat_messages).ThenInclude(m => m.sender)
                .OrderBy(t => t.created_at)
                .FirstOrDefaultAsync(t => t.consultant_id == uid);

            // если нет — берём первый свободный и закрепляем
            if (thread == null)
            {
                thread = await _db.chat_threads
                    .Include(t => t.chat_messages).ThenInclude(m => m.sender)
                    .Where(t => t.status == "open" && t.consultant_id == null)
                    .OrderBy(t => t.created_at)
                    .FirstOrDefaultAsync();

                if (thread != null)
                {
                    thread.consultant_id = uid.Value;
                    await _db.SaveChangesAsync();
                }
            }

            if (thread == null)
                return Ok(new { threadId = 0, messages = Array.Empty<object>() });
        }
        else
        {
            // клиент: свой тред, создать если нет
            thread = await _db.chat_threads
                .Include(t => t.chat_messages).ThenInclude(m => m.sender)
                .FirstOrDefaultAsync(t => t.client_id == uid);

            if (thread == null)
            {
                thread = new chat_thread
                {
                    client_id = uid.Value,
                    status = "open",
                    created_at = NowDb()
                };
                _db.chat_threads.Add(thread);
                await _db.SaveChangesAsync();
                thread.chat_messages = new List<chat_message>();
            }
        }

        var msgs = thread.chat_messages
            .OrderBy(m => m.created_at)
            .Select(m => new
            {
                id = m.id,
                text = m.text,
                senderId = m.sender_id,
                senderName = m.sender?.first_name ?? "(без имени)",
                createdAt = m.created_at
            })
            .ToList();

        return Ok(new { threadId = thread.id, messages = msgs });
    }

    [HttpPost("send")]
    public async Task<IActionResult> SendMessage([FromBody] SendMessageDto dto)
    {
        var uid = GetUserId();
        if (uid is null) return Unauthorized();
        if (dto == null || dto.ThreadId <= 0 || string.IsNullOrWhiteSpace(dto.Text))
            return BadRequest("threadId и text обязательны.");

        var role = GetRole();

        var thread = await _db.chat_threads.FirstOrDefaultAsync(t => t.id == dto.ThreadId);
        if (thread == null) return NotFound("Thread not found.");

        // Авторизация + автозакрепление
        if (role == "consultant")
        {
            if (thread.consultant_id == null)
                thread.consultant_id = uid.Value;
            else if (thread.consultant_id != uid)
                return Forbid();
        }
        else
        {
            if (thread.client_id != uid) return Forbid();
        }

        var msg = new chat_message
        {
            thread_id = thread.id,
            sender_id = uid.Value,
            text = dto.Text.Trim(),
            created_at = NowDb()
        };

        _db.chat_messages.Add(msg);
        await _db.SaveChangesAsync();

        return Ok(new { id = msg.id, text = msg.text, createdAt = msg.created_at });
    }
}
