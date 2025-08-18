namespace AutomationLetterWriting.Models
{
    public class Message
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }

        // برای اینکه بدونیم چه کسی ارسال کرده
        public string SenderId { get; set; }
        public ApplicationUser Sender { get; set; }

        // در صورت reply یا forward می‌توانیم پیام والد را داشته باشیم
        public int? ParentMessageId { get; set; }
        public Message? ParentMessage { get; set; }

        // Navigation
        public ICollection<MessageRecipient> Recipients { get; set; }
        public ICollection<Attachment> Attachments { get; set; }
    }
}
