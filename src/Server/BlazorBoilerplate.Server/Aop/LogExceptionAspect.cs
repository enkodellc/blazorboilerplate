using AspectInjector.Broker;
using BlazorBoilerplate.Infrastructure.Server.Models;
using BlazorBoilerplate.Server.Factories;
using System.Reflection;

namespace BlazorBoilerplate.Server.Aop
{
    [Aspect(Scope.PerInstance, Factory = typeof(AopServicesFactory))]
    public class LogExceptionAspect
    {
        private readonly ILogger<LogExceptionAspect> _logger;
        private static readonly MethodInfo _asyncHandler = typeof(LogExceptionAspect).GetMethod(nameof(WrapAsync), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly MethodInfo _syncHandler = typeof(LogExceptionAspect).GetMethod(nameof(WrapSync), BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly Type _voidTaskResult = Type.GetType("System.Threading.Tasks.VoidTaskResult");

        public LogExceptionAspect(ILogger<LogExceptionAspect> logger)
        {
            _logger = logger;
        }


        [Advice(Kind.Around, Targets = Target.Public | Target.Method)]
        public object Handle(
            [Argument(Source.Target)] Func<object[], object> target,
            [Argument(Source.Arguments)] object[] args,
            [Argument(Source.Name)] string name,
            [Argument(Source.ReturnType)] Type retType
            )
        {
            if (typeof(Task).IsAssignableFrom(retType))
            {
                var syncResultType = retType.IsConstructedGenericType ? retType.GenericTypeArguments[0] : _voidTaskResult;
                var tgt = target;
                return _asyncHandler.MakeGenericMethod(syncResultType).Invoke(this, new object[] { tgt, args, name });
            }
            else
            {
                retType = retType == typeof(void) ? typeof(object) : retType;
                return _syncHandler.MakeGenericMethod(retType).Invoke(this, new object[] { target, args, name });
            }
        }

        private T WrapSync<T>(Func<object[], object> target, object[] args, string name)
        {
            try
            {
                var result = (T)target(args);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{name}: {ex.GetBaseException().Message}");

                if (ex is DomainException)
                    throw;

                return default;
            }
        }

        private async Task<T> WrapAsync<T>(Func<object[], object> target, object[] args, string name)
        {
            try
            {
                var result = await ((Task<T>)target(args)).ConfigureAwait(false);

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError($"{name}: {ex.GetBaseException().Message}");

                if (ex is DomainException)
                    throw;

                return default;
            }
        }
    }
}
