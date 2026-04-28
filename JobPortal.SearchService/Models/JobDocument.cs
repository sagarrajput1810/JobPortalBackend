namespace JobPortal.SearchService.Models
{
    public class JobDocument
    {
        public int Id { get; set; } // Yeh wahi ID hai jo JobService mein hai
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
    }
}
