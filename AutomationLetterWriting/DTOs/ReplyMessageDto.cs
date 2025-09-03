namespace AutomationLetterWriting.DTOs
{
    public class ReplyMessageDto
    {
        public int ParentMessageId { get; set; }
        public string Body { get; set; } = string.Empty;
    }
}
