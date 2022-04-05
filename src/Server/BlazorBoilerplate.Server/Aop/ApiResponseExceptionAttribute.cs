using AspectInjector.Broker;

namespace BlazorBoilerplate.Server.Aop
{
    [Injection(typeof(ApiResponseExceptionAspect))]
    public class ApiResponseExceptionAttribute : Attribute
    {
    }
}
