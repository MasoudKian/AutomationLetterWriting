using AutomationLetterWriting.Context;
using AutomationLetterWriting.DTOs;
using AutomationLetterWriting.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

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


            // ارسال ایمیل امکان ارسال به چند نفر رو دارد
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
    }
}
