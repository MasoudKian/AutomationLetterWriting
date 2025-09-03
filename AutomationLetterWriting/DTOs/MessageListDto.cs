namespace AutomationLetterWriting.DTOs
{
    public class MessageListDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }

        public string SenderName { get; set; }
        public string? LetterType { get; set; }

        public bool IsRead { get; set; }
    }
}
