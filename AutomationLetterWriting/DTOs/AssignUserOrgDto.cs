namespace AutomationLetterWriting.DTOs
{
    public class AssignUserOrgDto
    {
        public string UserId { get; set; } = string.Empty;
        public int OrganizationUnitId { get; set; }
        public string? OrganizationEmail { get; set; }
        public string? JobTitle { get; set; }
    }
}
