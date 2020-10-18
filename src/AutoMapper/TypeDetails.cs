using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    /// <summary>
    /// Contains cached reflection information for easy retrieval
    /// </summary>
    [DebuggerDisplay("{Type}")]
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class TypeDetails
    {
        private readonly Dictionary<string, MemberInfo> _nameToMember;

        public TypeDetails(Type type, ProfileMap config)
        {
            Type = type;
            var membersToMap = MembersToMap(config.ShouldMapProperty, config.ShouldMapField);
            var publicReadableMembers = GetAllPublicReadableMembers(membersToMap);
            var publicWritableMembers = GetAllPublicWritableMembers(membersToMap);
            PublicReadAccessors = BuildPublicReadAccessors(publicReadableMembers);
            PublicWriteAccessors = BuildPublicAccessors(publicWritableMembers);
            Constructors = GetAllConstructors(config.ShouldUseConstructor);
            _nameToMember = new Dictionary<string, MemberInfo>(PublicReadAccessors.Length, StringComparer.OrdinalIgnoreCase);
            PossibleNames(config);
        }
        public MemberInfo GetMember(string name) => _nameToMember.GetOrDefault(name);
        private void PossibleNames(ProfileMap config)
        {
            var publicNoArgMethods = GetPublicNoArgMethods(config.ShouldMapMethod);
            var publicNoArgExtensionMethods = GetPublicNoArgExtensionMethods(config.SourceExtensionMethods.Where(config.ShouldMapMethod));
            foreach (var member in PublicReadAccessors.Concat(publicNoArgMethods).Concat(publicNoArgExtensionMethods))
            {
                foreach (var memberName in PossibleNames(member.Name, config.Prefixes, config.Postfixes))
                {
                    if (!_nameToMember.ContainsKey(memberName))
                    {
                        _nameToMember.Add(memberName, member);
                    }
                }
            }
        }
        public static IEnumerable<string> PossibleNames(string memberName, List<string> prefixes, List<string> postfixes)
        {
            yield return memberName;
            foreach (var prefix in prefixes)
            {
                if (!memberName.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                var withoutPrefix = memberName.Substring(prefix.Length);
                yield return withoutPrefix;
                foreach (var s in PostFixes(postfixes, withoutPrefix))
                {
                    yield return s;
                }
            }
            foreach (var s in PostFixes(postfixes, memberName))
            {
                yield return s;
            }
        }

        private static IEnumerable<string> PostFixes(List<string> postfixes, string name)
        {
            foreach (var postfix in postfixes)
            {
                if (!name.EndsWith(postfix, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                yield return name.Remove(name.Length - postfix.Length);
            }
        }

        private static Func<MemberInfo, bool> MembersToMap(
            Func<PropertyInfo, bool> shouldMapProperty,
            Func<FieldInfo, bool> shouldMapField)
        {
            return m =>
            {
                switch (m)
                {
                    case PropertyInfo property:
                        return !property.IsStatic() && shouldMapProperty(property);
                    case FieldInfo field:
                        return !field.IsStatic && shouldMapField(field);
                    default:
                        throw new ArgumentException("Should be a field or a property.");
                }
            };
        }

        public Type Type { get; }

        public ConstructorInfo[] Constructors { get; }

        public MemberInfo[] PublicReadAccessors { get; }

        public MemberInfo[] PublicWriteAccessors { get; }

        private IEnumerable<MethodInfo> GetPublicNoArgExtensionMethods(IEnumerable<MethodInfo> sourceExtensionMethodSearch)
        {
            var explicitExtensionMethods = sourceExtensionMethodSearch.Where(method => method.GetParameters()[0].ParameterType.IsAssignableFrom(Type));

            var genericInterfaces = Type.GetTypeInfo().ImplementedInterfaces.Where(t => t.IsGenericType);

            if (Type.IsInterface && Type.IsGenericType)
            {
                genericInterfaces = genericInterfaces.Union(new[] { Type });
            }

            return explicitExtensionMethods.Union
            (
                from genericInterface in genericInterfaces
                let genericInterfaceArguments = genericInterface.GenericTypeArguments
                let matchedMethods = (
                    from extensionMethod in sourceExtensionMethodSearch
                    where !extensionMethod.IsGenericMethodDefinition
                    select extensionMethod
                ).Concat(
                    from extensionMethod in sourceExtensionMethodSearch
                    where extensionMethod.IsGenericMethodDefinition
                        && extensionMethod.GetGenericArguments().Length == genericInterfaceArguments.Length
                    select extensionMethod.MakeGenericMethod(genericInterfaceArguments)
                )
                from methodMatch in matchedMethods
                where methodMatch.GetParameters()[0].ParameterType.IsAssignableFrom(genericInterface)
                select methodMatch
            );
        }

        private static MemberInfo[] BuildPublicReadAccessors(IEnumerable<MemberInfo> allMembers) =>
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.First())
                .Concat(allMembers.Where(x => x is FieldInfo)) // add FieldInfo objects back
                .ToArray();

        private static MemberInfo[] BuildPublicAccessors(IEnumerable<MemberInfo> allMembers) =>
            // Multiple types may define the same property (e.g. the class and multiple interfaces) - filter this to one of those properties
            allMembers
                .OfType<PropertyInfo>()
                .GroupBy(x => x.Name) // group properties of the same name together
                .Select(x => x.FirstOrDefault(y => y.CanWrite && y.CanRead) ?? x.First()) // favor the first property that can both read & write - otherwise pick the first one
                .Concat(allMembers.Where(x => x is FieldInfo)) // add FieldInfo objects back
                .ToArray();

        private IEnumerable<MemberInfo> GetAllPublicReadableMembers(Func<MemberInfo, bool> membersToMap)
            => GetAllPublicMembers(PropertyReadable, FieldReadable, membersToMap);

        private IEnumerable<MemberInfo> GetAllPublicWritableMembers(Func<MemberInfo, bool> membersToMap)
            => GetAllPublicMembers(PropertyWritable, FieldWritable, membersToMap);

        private ConstructorInfo[] GetAllConstructors(Func<ConstructorInfo, bool> shouldUseConstructor)
            => Type.GetDeclaredConstructors().Where(shouldUseConstructor).ToArray();

        private static bool PropertyReadable(PropertyInfo propertyInfo) => propertyInfo.CanRead;

        private static bool FieldReadable(FieldInfo fieldInfo) => true;

        private static bool PropertyWritable(PropertyInfo propertyInfo) => propertyInfo.CanWrite || propertyInfo.PropertyType.IsNonStringEnumerable();

        private static bool FieldWritable(FieldInfo fieldInfo) => !fieldInfo.IsInitOnly;

        private IEnumerable<MemberInfo> GetAllPublicMembers(
            Func<PropertyInfo, bool> propertyAvailableFor,
            Func<FieldInfo, bool> fieldAvailableFor,
            Func<MemberInfo, bool> memberAvailableFor)
        {
            var typesToScan = new List<Type>();
            for (var t = Type; t != null; t = t.BaseType)
                typesToScan.Add(t);

            if (Type.IsInterface)
                typesToScan.AddRange(Type.GetTypeInfo().ImplementedInterfaces);

            // Scan all types for public properties and fields
            return typesToScan
                .Where(x => x != null) // filter out null types (e.g. type.BaseType == null)
                .SelectMany(x => x.GetDeclaredMembers()
                    .Where(mi => mi.DeclaringType != null && mi.DeclaringType == x)
                    .Where(
                        m =>
                            m is FieldInfo && fieldAvailableFor((FieldInfo)m) ||
                            m is PropertyInfo && propertyAvailableFor((PropertyInfo)m) &&
                            !((PropertyInfo)m).GetIndexParameters().Any())
                    .Where(memberAvailableFor)
                );
        }

        private IEnumerable<MethodInfo> GetPublicNoArgMethods(Func<MethodInfo, bool> shouldMapMethod) =>
            Type.GetRuntimeMethods()
                .Where(shouldMapMethod)
                .Where(mi => mi.IsPublic && !mi.IsStatic && mi.DeclaringType != typeof(object))
                .Where(m => (m.ReturnType != typeof(void)) && (m.GetParameters().Length == 0));
    }
}