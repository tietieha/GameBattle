using System;
using AbilityKit.Ability.World.DI;

namespace AbilityKit.Demo.Moba.Services
{
    public static class MobaRuntimeGuard
    {
        public static InvalidOperationException CreateRequiredException(string owner, string operation, string requirement, string detail = null)
        {
            var message = string.IsNullOrEmpty(detail)
                ? $"{owner} requires {requirement} for {operation}."
                : $"{owner} requires {requirement} for {operation}. {detail}";
            return new InvalidOperationException(message);
        }

        public static void ThrowRequired(string owner, string operation, string requirement, string detail = null)
        {
            throw CreateRequiredException(owner, operation, requirement, detail);
        }

        public static T Require<T>(T service, string owner, string operation, string requirement = null)
            where T : class
        {
            if (service != null) return service;
            ThrowRequired(owner, operation, requirement ?? typeof(T).Name);
            return null;
        }

        public static T RequireResolved<T>(IWorldResolver services, string owner, string operation, string requirement = null)
            where T : class
        {
            if (services != null && services.TryResolve<T>(out var service) && service != null)
            {
                return service;
            }

            ThrowRequired(owner, operation, requirement ?? typeof(T).Name);
            return null;
        }

        public static void ReportAndThrow(
            IWorldResolver services,
            Exception exception,
            MobaBattleExceptionDomain domain,
            string operation,
            MobaBattleExceptionSeverity severity = MobaBattleExceptionSeverity.Critical,
            int actorId = 0,
            int skillId = 0,
            long runtimeId = 0L,
            string detail = null)
        {
            if (exception == null) exception = new InvalidOperationException("Unknown battle runtime failure.");

            if (services != null && services.TryResolve<IMobaBattleExceptionPolicy>(out var policy) && policy != null)
            {
                policy.Handle(
                    exception,
                    new MobaBattleExceptionContext(domain, operation, actorId, skillId, runtimeId, detail),
                    severity);
            }
            else
            {
                AbilityKit.Core.Logging.Log.Exception(exception, $"[MobaRuntimeGuard] domain={domain} operation={operation} severity={severity} {detail}");
            }

            throw exception;
        }

        public static void ThrowRequired(
            IWorldResolver services,
            string owner,
            string operation,
            string requirement,
            MobaBattleExceptionDomain domain,
            MobaBattleExceptionSeverity severity = MobaBattleExceptionSeverity.Critical,
            string detail = null)
        {
            var exception = CreateRequiredException(owner, operation, requirement, detail);
            ReportAndThrow(services, exception, domain, operation, severity, detail: detail);
        }
    }
}
