using System;
using System.Collections.Generic;

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
                    parameters.Add(prop.Name, dt == null ? null : dt.Value.ToString("s"));
                }
                else
                    parameters.Add(prop.Name, prop.GetValue(this));
            }

            return parameters;
        }
    }
}
