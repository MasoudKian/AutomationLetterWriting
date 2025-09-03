namespace AutomationLetterWriting.Models
{
    public class LetterType
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;

        // Navigation
        public ICollection<Message> Messages { get; set; } = new List<Message>();
    }
}
