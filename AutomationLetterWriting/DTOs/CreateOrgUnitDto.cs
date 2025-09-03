namespace AutomationLetterWriting.DTOs
{
    public class CreateOrgUnitDto
    {
        public string Name { get; set; } = string.Empty;
        public int? ParentId { get; set; }
    }
}
