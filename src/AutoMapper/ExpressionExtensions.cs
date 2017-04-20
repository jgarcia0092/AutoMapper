﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.Internal;

namespace AutoMapper
{
    internal static class ExpressionExtensions
    {
        public static IEnumerable<MemberExpression> GetMembers(this Expression expression)
        {
            return ((MemberExpression)expression).GetMembers();
        }

        public static IEnumerable<MemberExpression> GetMembers(this MemberExpression expression)
        {
            while(expression != null)
            {
                yield return expression;
                expression = expression.Expression as MemberExpression;
            }
        }

        public static bool IsMemberPath(this LambdaExpression exp)
        {
            return (exp.Body as MemberExpression).GetMembers().Last().Expression == exp.Parameters.First();
        }

        public static Expression ReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ReplaceParameters(exp, replace);

        public static Expression ConvertReplaceParameters(this LambdaExpression exp, params Expression[] replace)
            => ExpressionFactory.ConvertReplaceParameters(exp, replace);

        public static Expression Replace(this Expression exp, Expression old, Expression replace)
            => ExpressionFactory.Replace(exp, old, replace);

        public static LambdaExpression Concat(this LambdaExpression expr, LambdaExpression concat)
            => ExpressionFactory.Concat(expr, concat);

        public static Expression IfNotNull(this Expression expression, Type destinationType)
            => ExpressionFactory.IfNotNull(expression, destinationType);

        public static Expression RemoveIfNotNull(this Expression expression, params Expression[] expressions)
            => ExpressionFactory.RemoveIfNotNull(expression, expressions);

        public static Expression IfNullElse(this Expression expression, Expression then, Expression @else = null)
            => ExpressionFactory.IfNullElse(expression, then, @else);
    }
}