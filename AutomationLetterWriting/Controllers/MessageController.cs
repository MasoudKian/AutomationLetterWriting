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


        #region Forward , Reply , Attach

        [HttpPost("reply")]
        public async Task<IActionResult> Reply([FromForm] ReplyMessageDto model)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null) return Unauthorized();

            var parent = await _context.Messages.FindAsync(model.ParentMessageId);
            if (parent == null) return NotFound("Parent message not found.");

            var reply = new Message
            {
                Subject = "Re: " + parent.Subject,
                Body = model.Body,
                CreatedAt = DateTime.UtcNow,
                SenderId = sender.Id,
                ParentMessageId = parent.Id,
                Recipients = new List<MessageRecipient>
        {
            new MessageRecipient
            {
                ReceiverId = parent.SenderId
            }
        },
                Attachments = new List<Attachment>()
            };

            _context.Messages.Add(reply);
            await _context.SaveChangesAsync();

            return Ok("Reply sent successfully");
        }

        [HttpPost("forward")]
        public async Task<IActionResult> Forward([FromForm] ForwardMessageDto model)
        {
            var sender = await _userManager.GetUserAsync(User);
            if (sender == null) return Unauthorized();

            var parent = await _context.Messages
                .Include(m => m.Attachments)
                .FirstOrDefaultAsync(m => m.Id == model.ParentMessageId);

            if (parent == null) return NotFound("Parent message not found.");

            var forward = new Message
            {
                Subject = "Fwd: " + parent.Subject,
                Body = parent.Body,
                CreatedAt = DateTime.UtcNow,
                SenderId = sender.Id,
                ParentMessageId = parent.Id,
                Recipients = model.ReceiverIds.Select(r => new MessageRecipient
                {
                    ReceiverId = r
                }).ToList(),
                Attachments = parent.Attachments.Select(a => new Attachment
                {
                    FileName = a.FileName,
                    FilePath = a.FilePath,
                    FileSize = a.FileSize
                }).ToList()
            };

            _context.Messages.Add(forward);
            await _context.SaveChangesAsync();

            return Ok("Message forwarded successfully");
        }



        #endregion


        #region Report

        [HttpPost("report")]
        public async Task<IActionResult> GetReport([FromBody] MessageReportFilterDto filter)
        {
            var query = _context.Messages
                .Include(m => m.Sender)
                .Include(m => m.Recipients).ThenInclude(r => r.Receiver)
                .Include(m => m.LetterType)
                .AsQueryable();

            // فیلتر شماره نامه
            if (filter.MessageId.HasValue)
                query = query.Where(m => m.Id == filter.MessageId.Value);

            // فیلتر موضوع
            if (!string.IsNullOrEmpty(filter.Subject))
                query = query.Where(m => m.Subject.Contains(filter.Subject));

            // فیلتر تاریخ
            if (filter.FromDate.HasValue)
                query = query.Where(m => m.CreatedAt >= filter.FromDate.Value);

            if (filter.ToDate.HasValue)
                query = query.Where(m => m.CreatedAt <= filter.ToDate.Value);

            // فیلتر فرستنده
            if (!string.IsNullOrEmpty(filter.SenderId))
                query = query.Where(m => m.SenderId == filter.SenderId);

            // فیلتر گیرنده
            if (!string.IsNullOrEmpty(filter.ReceiverId))
                query = query.Where(m => m.Recipients.Any(r => r.ReceiverId == filter.ReceiverId));

            var result = await query
                .OrderByDescending(m => m.CreatedAt)
                .Select(m => new MessageReportDto
                {
                    Id = m.Id,
                    Subject = m.Subject,
                    Body = m.Body,
                    CreatedAt = m.CreatedAt,
                    SenderName = m.Sender.DisplayName ?? m.Sender.UserName,
                    Recipients = m.Recipients.Select(r => r.Receiver.DisplayName ?? r.Receiver.UserName).ToList(),
                    LetterType = m.LetterType != null ? m.LetterType.Name : null
                })
                .ToListAsync();

            return Ok(result);
        }


        #endregion


    }
}