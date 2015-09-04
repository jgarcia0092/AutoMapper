using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    /// <summary>
    /// Provides an interceptable IQueryable
    /// Copied from http://blogs.msdn.com/b/alexj/archive/2010/03/01/tip-55-how-to-extend-an-iqueryable-by-wrapping-it.aspx
    /// </summary>
    public class MappedQueryProvider<TDestination, TSource> : IQueryProvider
    {
        private readonly IQueryable _underlyingQuery;
        private readonly IMappingEngine _engine;
        private readonly Action<Exception> _onException;
        private readonly Action<IEnumerable> _onMaterialized;
        private IQueryProvider _underlyingProvider;

        private MappedQueryProvider(IQueryable underlyingQuery, IMappingEngine engine, Action<Exception> onException, Action<IEnumerable> onMaterialized)
        {
            if (underlyingQuery == null) throw new ArgumentNullException(nameof(underlyingQuery));
            if (engine == null) throw new ArgumentNullException(nameof(engine));
            _underlyingQuery = underlyingQuery;
            _engine = engine;
            _underlyingProvider = underlyingQuery.Provider;
            
            _onException = onException ?? ((y) => { });
            _onMaterialized = onMaterialized ?? ((y) => { });
        }

        public static IQueryable<T> Map<T>(IQueryable<TSource> underlyingQuery, IMappingEngine engine, Action<Exception> onException = null, Action<IEnumerable<T>> onMaterialized = null)
        {
            return new MappedQueryable<T, TSource>(new MappedQueryProvider<T, TSource>(underlyingQuery, engine, onException, (x) =>
            {
                if (onMaterialized != null) onMaterialized((IEnumerable<T>) x);
            }), 

            new T[0].AsQueryable().Expression);
        }

        public IEnumerator<TDestination> ExecuteQuery(Expression expression)
        {
            try
            {
                var visitor = new QueryMapperVisitor(typeof(TDestination), typeof (TSource), _underlyingQuery, _engine);
                var expr = visitor.Visit(expression);

                var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);

                return newDestQuery.ProjectTo<TDestination>().GetEnumerator();
            }
            catch (Exception x)
            {
                _onException(x);
                throw;
            }
        }

        public IQueryable<TElement> CreateQuery<TElement>(Expression expression)
        {
            try
            { 
                Type et = typeof (TElement);
                Type dt = typeof(TSource);
                Type qt = typeof(MappedQueryable<,>).MakeGenericType(et, dt);
                Type qpt = typeof(MappedQueryProvider<,>).MakeGenericType(et, dt);
                object[] args = new object[] { this, expression };

                ConstructorInfo ci = qt.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[] {
                        qpt,
                        typeof(Expression)
                    },
                    null);

                return (IQueryable<TElement>)ci.Invoke(args);
            }
            catch (Exception x)
            {
                _onException(x);
                throw;
            }
        }

        public IQueryable CreateQuery(Expression expression)
        {
            try
            {

                Type et = TypeHelper.FindIEnumerable(expression.Type).GetGenericArguments().First();
                Type dt = typeof (TSource);
                Type qt = typeof (MappedQueryable<,>).MakeGenericType(et, dt);
                Type qpt = typeof (MappedQueryProvider<,>).MakeGenericType(et, dt);
                object[] args = new object[] {this, expression};

                ConstructorInfo ci = qt.GetConstructor(
                    BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance,
                    null,
                    new Type[]
                    {
                        qpt,
                        typeof (Expression)
                    },
                    null);

                return (IQueryable) ci.Invoke(args);

            }
            catch (Exception x)
            {
                _onException(x);
                throw;
            }
        }
        public TResult Execute<TResult>(Expression expression)
        {
            try
            {
                var visitor = new QueryMapperVisitor(typeof (TSource), typeof (TResult), _underlyingQuery, _engine);
                var expr = visitor.Visit(expression);

                var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);

                return newDestQuery.ProjectTo<TResult>().SingleOrDefault();
            }
            catch (Exception x)
            {
                _onException(x);
                throw;
            }
        }

        public object Execute(Expression expression)
        {
            try
            {
                var visitor = new QueryMapperVisitor(typeof(TDestination), typeof(TSource), _underlyingQuery, _engine);
                var expr = visitor.Visit(expression);

                var newDestQuery = _underlyingQuery.Provider.CreateQuery<TSource>(expr);

                return newDestQuery.ProjectTo<TSource>().SingleOrDefault();
            }
            catch (Exception x)
            {
                _onException(x);
                throw;
            }
        }
    }
}
