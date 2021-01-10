namespace BlazorBoilerplate.Infrastructure.Storage.Permissions
{
    public class EntityPermission
    {
        public EntityPermission()
        { }

        public EntityPermission(string name, string value, string groupName, string description = null)
        {
            Name = name;
            Value = value;
            GroupName = groupName;
            Description = description;
        }

        public string Name { get; set; }
        public string Value { get; set; }
        public string GroupName { get; set; }
        public string Description { get; set; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(EntityPermission permission)
        {
            return permission.Value;
        }
    }
}
