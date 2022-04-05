namespace BlazorBoilerplate.Shared.Dto.Admin
{
    public class TenantDto : BaseDto
    {
        public string Id { get; set; }
        public string Identifier { get; set; }
        public string Name { get; set; }
        public IDictionary<string, object> Items { get; }
        public string ConnectionString { get; set; }
    }
}