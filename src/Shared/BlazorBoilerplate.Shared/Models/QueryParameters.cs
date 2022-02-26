namespace BlazorBoilerplate.Shared.Models
{
    public abstract class QueryParameters
    {
        public Dictionary<string, object> ToDictionary()
        {
            var parameters = new Dictionary<string, object>();

            foreach (var prop in GetType().GetProperties())
            {
                if (prop.PropertyType == typeof(DateTime?))
                {
                    var dt = (DateTime?)prop.GetValue(this);
                    parameters.Add(prop.Name, dt?.ToString("s"));
                }
                else
                    parameters.Add(prop.Name, prop.GetValue(this));
            }

            return parameters;
        }

        public string ToQuery()
        {
            var queryOption = new List<string>();

            foreach (var i in ToDictionary().Where(p => p.Value != null))
                queryOption.Add($"{i.Key}={i.Value}");

            return string.Join('&', queryOption);
        }
    }
}
