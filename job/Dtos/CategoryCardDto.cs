namespace job.Dtos
{
    public class CategoryCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int TotalJobs { get; set; }
    }
}
