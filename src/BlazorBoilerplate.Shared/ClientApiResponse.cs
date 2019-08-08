using System.Runtime.Serialization;

namespace BlazorBoilerplate.Shared
{
    [DataContract]
    public class ClientApiResponse
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public int StatusCode { get; set; }

        [DataMember]
        public string Message { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public string ResponseException { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public object Result { get; set; }
    }
}
