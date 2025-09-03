namespace AutomationLetterWriting.Models
{
    public class OrganizationUnit
    {
        public int Id { get; set; } 
        public string Name { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        public OrganizationUnit? Parent { get; set; }
        public ICollection<OrganizationUnit> Children { get; set; } = new List<OrganizationUnit>();

        public ICollection<ApplicationUser> Users { get; set; } = new List<ApplicationUser>();
    }
}
