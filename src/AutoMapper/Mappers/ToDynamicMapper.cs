using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.CompilerServices;
using AutoMapper.Execution;
using AutoMapper.Internal;
using Microsoft.CSharp.RuntimeBinder;
using Binder = Microsoft.CSharp.RuntimeBinder.Binder;

namespace AutoMapper.Mappers
{
    using static Expression;
    using static ExpressionFactory;

    public class ToDynamicMapper : IObjectMapper
    {
        private static TDestination Map<TSource, TDestination>(TSource source, TDestination destination, ResolutionContext context, ProfileMap profileMap)
        {
            var sourceTypeDetails = profileMap.CreateTypeDetails(typeof(TSource));
            foreach (var member in sourceTypeDetails.ReadAccessors)
            {
                object sourceMemberValue;
                try
                {
                    sourceMemberValue = member.GetMemberValue(source);
                }
                catch (RuntimeBinderException)
                {
                    continue;
                }
                var destinationMemberValue = context.MapMember(member, sourceMemberValue);
                SetDynamically(member.Name, destination, destinationMemberValue);
            }
            return destination;
        }

        private static void SetDynamically(string memberName, object target, object value)
        {
            var binder = Binder.SetMember(CSharpBinderFlags.None, memberName, null,
                new[]{
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null),
                    CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null)
                });
            var callsite = CallSite<Func<CallSite, object, object, object>>.Create(binder);
            callsite.Target(callsite, target, value);
        }

        private static readonly MethodInfo MapMethodInfo = typeof(ToDynamicMapper).GetStaticMethod(nameof(Map));

        public bool IsMatch(in TypePair context) => context.DestinationType.IsDynamic() && !context.SourceType.IsDynamic();

        public Expression MapExpression(IGlobalConfiguration configurationProvider, ProfileMap profileMap,
            MemberMap memberMap, Expression sourceExpression, Expression destExpression) =>
            Call(null,
                MapMethodInfo.MakeGenericMethod(sourceExpression.Type, destExpression.Type),
                sourceExpression,
                ToType(
                    Coalesce(destExpression.ToObject(),
                        ObjectFactory.GenerateConstructorExpression(destExpression.Type)), destExpression.Type),
                ContextParameter,
                Constant(profileMap));
    }
}
