namespace job.Dtos
{
    public class JobApplicantsDashboardDto
    {
        public int Total { get; set; } = 0;
        public int Pending { get; set; } = 0;
        public int Interested { get; set; } = 0;
        public int Rejected { get; set; } = 0;
        public IEnumerable<ApplicantCardDto> Applicants { get; set; } = new List<ApplicantCardDto>();
    }
}