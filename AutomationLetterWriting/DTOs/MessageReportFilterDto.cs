namespace AutomationLetterWriting.DTOs
{
    public class MessageReportFilterDto
    {
        public int? MessageId { get; set; }        
        public string? Subject { get; set; }       
        public DateTime? FromDate { get; set; }    
        public DateTime? ToDate { get; set; }      
        public string? SenderId { get; set; }      
        public string? ReceiverId { get; set; }    
    }
}
