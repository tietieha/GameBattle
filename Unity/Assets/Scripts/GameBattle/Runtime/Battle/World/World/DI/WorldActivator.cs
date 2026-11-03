using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using AbilityKit.Ability.World.Services.Attributes;

namespace AbilityKit.Ability.World.DI
{
    internal static class WorldActivator
    {
        private sealed class CtorPlan
        {
            public ConstructorInfo Ctor;
            public Type[] ParamTypes;
            public string Signature;
        }

        private sealed class InjectMemberPlan
        {
            public FieldInfo Field;
            public PropertyInfo Property;
            public Type ServiceType;
            public bool Required;
            public string MemberName;
        }

        private sealed class TypePlan
        {
            public Type ImplType;
            public CtorPlan[] Ctors;
            public InjectMemberPlan[] InjectMembers;
        }

        private static readonly ConcurrentDictionary<Type, TypePlan> s_planCache = new ConcurrentDictionary<Type, TypePlan>();

        public static object Create(Type implType, IWorldResolver resolver)
        {
            if (implType == null) throw new ArgumentNullException(nameof(implType));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            var plan = s_planCache.GetOrAdd(implType, BuildPlan);
            if (plan.Ctors == null || plan.Ctors.Length == 0)
            {
                throw new InvalidOperationException($"No public constructor found for type: {implType.FullName}");
            }

            CtorPlan best = null;
            object[] bestArgs = null;
            var bestScore = -1;

            StringBuilder diag = null;

            for (int i = 0; i < plan.Ctors.Length; i++)
            {
                var cp = plan.Ctors[i];
                var paramTypes = cp.ParamTypes;
                var args = new object[paramTypes.Length];

                var ok = true;
                int missingAt = -1;
                Type missingType = null;

                for (int p = 0; p < paramTypes.Length; p++)
                {
                    var pt = paramTypes[p];
                    if (resolver.TryResolve(pt, out var arg))
                    {
                        args[p] = arg;
                    }
                    else
                    {
                        ok = false;
                        missingAt = p;
                        missingType = pt;
                        break;
                    }
                }

                if (!ok)
                {
                    diag ??= new StringBuilder(256);
                    diag.Append("  ");
                    diag.Append(cp.Signature);
                    diag.Append(" missing: ");
                    diag.Append(missingType?.FullName ?? (missingType?.Name ?? "unknown"));
                    diag.Append(" @index=");
                    diag.Append(missingAt);
                    diag.AppendLine();
                    continue;
                }

                if (paramTypes.Length > bestScore)
                {
                    best = cp;
                    bestArgs = args;
                    bestScore = paramTypes.Length;
                }
            }

            if (best == null)
            {
                var msg = $"No suitable constructor found for type: {implType.FullName}. Make sure dependencies are registered.";
                if (diag != null)
                {
                    msg += "\nMissing dependencies by constructor:\n" + diag;
                }
                throw new InvalidOperationException(msg);
            }

            var instance = best.Ctor.Invoke(bestArgs);
            InjectMembers(instance, plan.InjectMembers, resolver);
            return instance;
        }

        /// <summary>
        /// 解析并写入已创建实例上的所有 <c>[WorldInject]</c> 字段/属性。
        /// 主要用于测试：手动构造实例（例如通过无参构造函数），再通过容器运行时使用的同一注入路径注入 mock。
        /// </summary>
        internal static void InjectMembersInto(object instance, IWorldResolver resolver)
        {
            if (instance == null) throw new ArgumentNullException(nameof(instance));
            if (resolver == null) throw new ArgumentNullException(nameof(resolver));

            var plan = s_planCache.GetOrAdd(instance.GetType(), BuildPlan);
            InjectMembers(instance, plan.InjectMembers, resolver);
        }

        private static void InjectMembers(object instance, InjectMemberPlan[] members, IWorldResolver resolver)
        {
            if (instance == null) return;
            if (members == null || members.Length == 0) return;

            for (int i = 0; i < members.Length; i++)
            {
                var member = members[i];
                if (member == null) continue;

                if (!resolver.TryResolve(member.ServiceType, out var value))
                {
                    if (member.Required)
                    {
                        throw new InvalidOperationException($"Required world service injection failed. type={instance.GetType().FullName} member={member.MemberName} service={member.ServiceType.FullName}");
                    }

                    continue;
                }

                member.Field?.SetValue(instance, value);
                member.Property?.SetValue(instance, value, null);
            }
        }

        private static TypePlan BuildPlan(Type implType)
        {
            var ctors = implType.GetConstructors(BindingFlags.Public | BindingFlags.Instance);
            if (ctors == null || ctors.Length == 0)
            {
                return new TypePlan { ImplType = implType, Ctors = Array.Empty<CtorPlan>() };
            }

            var plans = new List<CtorPlan>(ctors.Length);
            for (int i = 0; i < ctors.Length; i++)
            {
                var ctor = ctors[i];
                var ps = ctor.GetParameters();
                var paramTypes = new Type[ps.Length];
                for (int p = 0; p < ps.Length; p++)
                {
                    paramTypes[p] = ps[p].ParameterType;
                }

                var sb = new StringBuilder(64);
                sb.Append("ctor(");
                for (int p = 0; p < paramTypes.Length; p++)
                {
                    if (p > 0) sb.Append(", ");
                    sb.Append(paramTypes[p].Name);
                }
                sb.Append(")");

                plans.Add(new CtorPlan
                {
                    Ctor = ctor,
                    ParamTypes = paramTypes,
                    Signature = sb.ToString(),
                });
            }

            // 优先尝试更具体的构造函数（参数更多）。
            plans.Sort((a, b) => b.ParamTypes.Length.CompareTo(a.ParamTypes.Length));

            return new TypePlan
            {
                ImplType = implType,
                Ctors = plans.ToArray(),
                InjectMembers = BuildInjectMemberPlans(implType),
            };
        }

        private static InjectMemberPlan[] BuildInjectMemberPlans(Type implType)
        {
            var members = new List<InjectMemberPlan>(4);
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            for (var t = implType; t != null && t != typeof(object); t = t.BaseType)
            {
                var fields = t.GetFields(flags | BindingFlags.DeclaredOnly);
                for (int i = 0; i < fields.Length; i++)
                {
                    var field = fields[i];
                    if (field.IsStatic) continue;
                    if (field.IsInitOnly) continue;

                    var attr = (WorldInjectAttribute)Attribute.GetCustomAttribute(field, typeof(WorldInjectAttribute), true);
                    if (attr == null) continue;

                    var serviceType = attr.ServiceType ?? field.FieldType;
                    if (serviceType == null) continue;
                    if (!field.FieldType.IsAssignableFrom(serviceType))
                    {
                        throw new InvalidOperationException($"WorldInject service type is not assignable to field. type={implType.FullName} member={field.Name} fieldType={field.FieldType.FullName} service={serviceType.FullName}");
                    }

                    members.Add(new InjectMemberPlan
                    {
                        Field = field,
                        ServiceType = serviceType,
                        Required = attr.Required,
                        MemberName = field.Name,
                    });
                }

                var properties = t.GetProperties(flags | BindingFlags.DeclaredOnly);
                for (int i = 0; i < properties.Length; i++)
                {
                    var property = properties[i];
                    var attr = (WorldInjectAttribute)Attribute.GetCustomAttribute(property, typeof(WorldInjectAttribute), true);
                    if (attr == null) continue;

                    var setter = property.GetSetMethod(true);
                    if (setter == null || setter.IsStatic)
                    {
                        throw new InvalidOperationException($"WorldInject property must have an instance setter. type={implType.FullName} member={property.Name}");
                    }

                    var serviceType = attr.ServiceType ?? property.PropertyType;
                    if (serviceType == null) continue;
                    if (!property.PropertyType.IsAssignableFrom(serviceType))
                    {
                        throw new InvalidOperationException($"WorldInject service type is not assignable to property. type={implType.FullName} member={property.Name} propertyType={property.PropertyType.FullName} service={serviceType.FullName}");
                    }

                    members.Add(new InjectMemberPlan
                    {
                        Property = property,
                        ServiceType = serviceType,
                        Required = attr.Required,
                        MemberName = property.Name,
                    });
                }
            }

            return members.Count == 0 ? Array.Empty<InjectMemberPlan>() : members.ToArray();
        }
    }
}
