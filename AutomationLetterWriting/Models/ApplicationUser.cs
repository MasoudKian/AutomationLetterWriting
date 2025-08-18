using Microsoft.AspNetCore.Identity;

namespace AutomationLetterWriting.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }

        // Navigation
        public ICollection<MessageRecipient> ReceivedMessages { get; set; }
        public ICollection<Message> SentMessages { get; set; }
    }
}
