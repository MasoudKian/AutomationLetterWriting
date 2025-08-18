namespace AutomationLetterWriting.Models
{
    public class Attachment
    {
        public int Id { get; set; }

        public string FileName { get; set; }
        public string FilePath { get; set; }
        public long FileSize { get; set; }

        public int MessageId { get; set; }
        public Message Message { get; set; }
    }
}
