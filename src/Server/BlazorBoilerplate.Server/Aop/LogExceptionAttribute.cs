using AspectInjector.Broker;

namespace BlazorBoilerplate.Server.Aop
{
    [Injection(typeof(LogExceptionAspect))]
    public class LogExceptionAttribute : Attribute
    {
    }
}
