namespace AutomationLetterWriting.DTOs
{

    public class ForwardMessageDto
    {
        public int ParentMessageId { get; set; }
        public List<string> ReceiverIds { get; set; } = new();
    }
}
