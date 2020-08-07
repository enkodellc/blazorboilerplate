using System.Runtime.Serialization;

namespace BlazorBoilerplate.Shared.Dto
{
    [DataContract]
    public class ApiResponseDto<T>
    {
        [DataMember]
        public string Version { get; set; }

        [DataMember]
        public int StatusCode { get; set; }

        public bool IsSuccessStatusCode => StatusCode / 200 == 1;

        [DataMember]
        public string Message { get; set; }

        [DataMember(EmitDefaultValue = false)]
        public T Result { get; set; }
    }

    [DataContract]
    public class ApiResponseDto : ApiResponseDto<object>
    {
    }
}
