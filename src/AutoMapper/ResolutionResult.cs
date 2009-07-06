using System;

namespace AutoMapper
{
	public class ResolutionResult
	{
		private readonly object _value;
		private readonly Type _type;
		private readonly Type _memberType;

		public ResolutionResult(object value, Type memberType)
		{
			_value = value;
			_type = value == null
			       	? memberType
			       	: value.GetType();
			_memberType = memberType;
		}

        public ResolutionResult(object value, Type memberType, Type type)
		{
			_value = value;
			_type = type;
			_memberType = memberType;
		}

		public ResolutionResult(object value)
			: this(value, typeof(object))
		{
		}

		public object Value { get { return _value; } }
		public Type Type { get { return _type; } }
		public Type MemberType { get { return _memberType; } }
	}
}