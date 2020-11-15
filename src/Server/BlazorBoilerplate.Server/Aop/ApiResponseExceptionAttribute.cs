using AspectInjector.Broker;
using System;

namespace BlazorBoilerplate.Server.Aop
{
    [Injection(typeof(ApiResponseExceptionAspect))]
    public class ApiResponseExceptionAttribute : Attribute
    {
    }
}
