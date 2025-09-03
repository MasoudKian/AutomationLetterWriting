namespace AutomationLetterWriting.DTOs
{
    public class MessageReportDto
    {
        public int Id { get; set; }
        public string Subject { get; set; }
        public string Body { get; set; }
        public DateTime CreatedAt { get; set; }

        public string SenderName { get; set; }
        public List<string> Recipients { get; set; } = new();
        public string? LetterType { get; set; }
    }
}
