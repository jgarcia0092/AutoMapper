using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Reflection;

namespace AutoMapper.Configuration.Conventions
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class MemberConfiguration : IMemberConfiguration
    {
        public IParentSourceToDestinationNameMapper NameMapper { get; set; }

        public List<IChildMemberConfiguration> MemberMappers { get; } = new List<IChildMemberConfiguration>();

        public IMemberConfiguration AddMember<TMemberMapper>(Action<TMemberMapper> setupAction = null)
            where TMemberMapper : IChildMemberConfiguration, new()
        {
            GetOrAdd(_ => (IList)_.MemberMappers, setupAction);
            return this;
        }

        public IMemberConfiguration AddName<TNameMapper>(Action<TNameMapper> setupAction = null)
            where TNameMapper : ISourceToDestinationNameMapper, new()
        {
            GetOrAdd(_ => (IList)_.NameMapper.NamedMappers, setupAction);
            return this;
        }

        private TMemberMapper GetOrAdd<TMemberMapper>(Func<IMemberConfiguration, IList> getList, Action<TMemberMapper> setupAction = null)
            where TMemberMapper : new()
        {
            var child = getList(this).OfType<TMemberMapper>().FirstOrDefault();
            if (child == null)
            {
                child = new TMemberMapper();
                getList(this).Add(child);
            }
            setupAction?.Invoke(child);
            return child;
        }

        public MemberConfiguration()
        {
            NameMapper = new ParentSourceToDestinationNameMapper();
            MemberMappers.Add(new DefaultMember { NameMapper = NameMapper });
        }

        public bool MapDestinationPropertyToSource(ProfileMap options, TypeDetails sourceType, Type destType, Type destMemberType, string nameToSearch, LinkedList<MemberInfo> resolvers, bool isReverseMap)
        {
            foreach (var memberMapper in MemberMappers)
            {
                if (memberMapper.MapDestinationPropertyToSource(options, sourceType, destType, destMemberType, nameToSearch, resolvers, this, isReverseMap))
                {
                    return true;
                }
            }
            return false;
        }
    }
}