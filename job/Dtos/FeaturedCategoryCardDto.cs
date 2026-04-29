namespace job.Dtos
{
    public class FeaturedCategoryCardDto
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public int TotalJobs { get; set; }
        public int Growth { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public List<string> TopSkills { get; set; } = new();
        public double CompetitionRatio { get; set; }
    }
}
