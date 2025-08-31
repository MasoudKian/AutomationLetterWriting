namespace AutomationLetterWriting.DTOs
{
    public class SendMessageDto
    {
        public string Subject { get; set; } = string.Empty;
        public string Body { get; set; } = string.Empty;
        public List<string> ReceiverIds { get; set; } = new();
        public List<IFormFile>? Attachments { get; set; }
        public int? ParentMessageId { get; set; }   
    }
}
