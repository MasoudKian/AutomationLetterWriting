using AutomationLetterWriting.Context;
using AutomationLetterWriting.DTOs;
using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace AutomationLetterWriting.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class MessageController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IWebHostEnvironment _env;

        public MessageController(
            ApplicationDbContext context,
            UserManager<ApplicationUser> userManager,
            IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }


        [HttpGet("ping")]
        public IActionResult Ping()
        {
            return Ok("pong");
        }


        [HttpPost("send")]
        public async Task<IActionResult> Send([FromForm] SendMessageDto model)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null) return Unauthorized();

            var message = new Message
            {
                Subject = model.Subject,
                Body = model.Body,
                CreatedAt = DateTime.UtcNow,
                SenderId = sender.Id,
                ParentMessageId = model.ParentMessageId,
                Recipients = new List<MessageRecipient>(),
                Attachments = new List<Attachment>()
            };


            foreach (var receiverId in model.ReceiverIds)
            {
                message.Recipients.Add(new MessageRecipient
                {
                    MessageId = message.Id,
                    ReceiverId = receiverId
                });
            }

            // Attachments
            if (model.Attachments != null)
            {
                foreach (var file in model.Attachments)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
                    var path = Path.Combine(_env.WebRootPath ?? "wwwroot", "uploads", fileName);

                    Directory.CreateDirectory(Path.GetDirectoryName(path)!);

                    using var stream = new FileStream(path, FileMode.Create);
                    await file.CopyToAsync(stream);

                    message.Attachments.Add(new Attachment
                    {
                        FileName = file.FileName,
                        FilePath = $"/uploads/{fileName}",
                        FileSize = file.Length
                    });
                }
            }

            _context.Messages.Add(message);
            await _context.SaveChangesAsync();

            return Ok("Message sent successfully");
        }


        #region InBox Out Box 

        [HttpGet("inbox")]
        public async Task<IActionResult> Inbox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var messages = await _context.MessageRecipients
                .Where(r => r.ReceiverId == user.Id && !r.IsDeleted)
                .Select(r => new MessageListDto
                {
                    Id = r.Message.Id,
                    Subject = r.Message.Subject,
                    Body = r.Message.Body,
                    CreatedAt = r.Message.CreatedAt,
                    SenderName = r.Message.Sender.DisplayName ?? r.Message.Sender.UserName,
                    LetterType = r.Message.LetterType != null ? r.Message.LetterType.Name : null,
                    IsRead = r.IsRead
                })
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        [HttpGet("outbox")]
        public async Task<IActionResult> Outbox()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var messages = await _context.Messages
                .Where(m => m.SenderId == user.Id)
                .Select(m => new MessageListDto
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    SenderName = m.Sender.DisplayName ?? m.Sender.UserName,
                    LetterType = m.LetterType != null ? m.LetterType.Name : null,
                    IsRead = true // چون فرستنده خودش همیشه دیده
                })
                .OrderByDescending(m => m.CreatedAt)
                .ToListAsync();

            return Ok(messages);
        }

        #endregion

        [HttpGet("{id}")]
        public async Task<IActionResult> GetMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            // نامه باید یا فرستاده‌شده توسط کاربر باشه یا دریافت‌شده
            var recipient = await _context.MessageRecipients
                .Include(r => r.Message)
                    .ThenInclude(m => m.Sender)
                .Include(r => r.Message.LetterType)
                .Include(r => r.Message.Attachments)
                .Where(r => r.MessageId == id && (r.ReceiverId == user.Id || r.Message.SenderId == user.Id))
                .FirstOrDefaultAsync();

            if (recipient == null) return NotFound();

            var message = recipient.Message;

            var dto = new MessageDetailDto
            {
                Id = message.Id,
                Subject = message.Subject,
                Body = message.Body,
                CreatedAt = message.CreatedAt,
                SenderName = message.Sender.DisplayName ?? message.Sender.UserName,
                LetterType = message.LetterType?.Name,
                Recipients = message.Recipients.Select(x => x.Receiver.DisplayName ?? x.Receiver.UserName).ToList(),
                Attachments = message.Attachments.Select(a => new AttachmentDto
                {
                    Id = a.Id,
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileSize = a.FileSize
                }).ToList()
            };

            return Ok(dto);
        }

        [HttpPost("{id}/read")]
        public async Task<IActionResult> MarkAsRead(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var recipient = await _context.MessageRecipients
                .FirstOrDefaultAsync(r => r.MessageId == id && r.ReceiverId == user.Id);

            if (recipient == null) return NotFound();

            recipient.IsRead = true;
            await _context.SaveChangesAsync();

            return Ok("Marked as read");
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteMessage(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            var recipient = await _context.MessageRecipients
                .FirstOrDefaultAsync(r => r.MessageId == id && r.ReceiverId == user.Id);

            if (recipient == null) return NotFound();

            recipient.IsDeleted = true;
            await _context.SaveChangesAsync();

            return Ok("Message deleted from inbox");
        }



    }
}