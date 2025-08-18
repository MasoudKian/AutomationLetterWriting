namespace AutomationLetterWriting.Models
{
    public class MessageRecipient
    {
        public int Id { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }

        public string ReceiverId { get; set; }
        public ApplicationUser Receiver { get; set; }

        // وضعیت کاربر نسبت به پیام
        public bool IsRead { get; set; }
        public bool IsDeleted { get; set; }
    }
}
