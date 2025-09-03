namespace AutomationLetterWriting.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }

        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        public int? ParentMessageId { get; set; }
        public Message? ParentMessage { get; set; }

        public int? LetterTypeId { get; set; }
        public LetterType? LetterType { get; set; }

        // Navigation
        public ICollection<MessageRecipient> Recipients { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }
}
