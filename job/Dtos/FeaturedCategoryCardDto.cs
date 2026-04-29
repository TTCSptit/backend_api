namespace job.Dtos
{
    public class FeaturedCategoryCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int TotalJobs { get; set; }
        public int Growth { get; set; }
    }
}
