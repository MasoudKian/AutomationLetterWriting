using Microsoft.AspNetCore.Identity;

namespace AutomationLetterWriting.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string? DisplayName { get; set; }

        public string? OrganizationEmail { get; set; }

        public string? JobTitle { get; set; }



        // Navigation
        public ICollection<MessageRecipient> ReceivedMessages { get; set; }
        public ICollection<Message> SentMessages { get; set; }

        public int? OrganizationUnitId { get; set; }
        public OrganizationUnit? OrganizationUnit { get; set; }
    }
}
