﻿using AutoMapper.Internal;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq.Expressions;

namespace AutoMapper.QueryableExtensions.Impl
{
    using static Expression;
    [EditorBrowsable(EditorBrowsableState.Never)]
    public abstract class EnumBinder : IExpressionBinder
    {
        public Expression Build(IGlobalConfiguration configuration, IMemberMap memberMap, TypeMap memberTypeMap, ExpressionRequest request, ExpressionResolutionResult result, IDictionary<ExpressionRequest, int> typePairCount, LetPropertyMaps letPropertyMaps)
            => Convert(result.ResolutionExpression, memberMap.DestinationType);
        public abstract bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult result);
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToUnderlyingTypeBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult result) => memberMap.Types.IsEnumToUnderlyingType();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class UnderlyingTypeToEnumBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult result) => memberMap.Types.IsUnderlyingTypeToEnum();
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class EnumToEnumBinder : EnumBinder
    {
        public override bool IsMatch(IMemberMap memberMap, TypeMap memberTypeMap, ExpressionResolutionResult result) => memberMap.Types.IsEnumToEnum();
    }
}