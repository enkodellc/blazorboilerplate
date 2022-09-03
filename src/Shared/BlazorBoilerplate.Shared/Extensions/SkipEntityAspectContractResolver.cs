using Breeze.Sharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace BlazorBoilerplate.Shared.Extensions
{
    public class SkipEntityAspectContractResolver : DefaultContractResolver
    {
        protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
        {
            return base.CreateProperties(type, memberSerialization).Where(i => i.PropertyName != nameof(EntityAspect)).ToList();
        }
    }
}
