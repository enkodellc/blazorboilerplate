namespace BlazorBoilerplate.Shared.Dto.Sample
{
    public class JobDto
    {
        public int Id { get; set; }
        public JobStatuses Status { get; set; }
        public string Description { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public enum JobStatuses
    {
        Todo,
        Started,
        Completed
    }
}