using AspectInjector.Broker;
using System;

namespace BlazorBoilerplate.Server.Aop
{
    [Injection(typeof(LogExceptionAspect))]
    public class LogExceptionAttribute : Attribute
    {
    }
}
