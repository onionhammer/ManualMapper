using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;

namespace Wivuu.ManualMapper
{
    public sealed class Mapper
    {
        /// <summary>
        /// Mappings stored in the mapper
        /// </summary>
        internal readonly Dictionary<Type, MapExpression> Mappings
            = new Dictionary<Type, MapExpression>();

        /// <summary>
        /// Create a mapping between TSource and TDest
        /// </summary>
        public MapBuilder<TSource, TDest> CreateMap<TSource, TDest>()
            where TDest : class
        {
            var expression = new MapExpression<TDest>();
            Mappings[typeof(TDest)] = expression;
            return new MapBuilder<TSource, TDest>(expression);
        }

        /// <summary>
        /// Map to a new destination
        /// </summary>
        public TDest Map<TDest>(object source)
            where TDest : class, new()
        {
            Contract.Assert(source != null);

            var expr = Mappings[typeof(TDest)] as MapExpression<TDest>;
            if (expr == null)
                throw new DestinationNotMapped(dest: typeof(TDest).Name);

            var dest = expr.CtorFunc();
            expr.CopyParametersAction(source, dest);
            return dest;
        }

        /// <summary>
        /// Map to a new destination
        /// </summary>
        public TDest Map<TDest>(object source, TDest dest)
            where TDest : class, new()
        {
            Contract.Assert(source != null);
            Contract.Assert(dest != null);

            var expr = Mappings[typeof(TDest)] as MapExpression<TDest>;
            if (expr == null)
                throw new DestinationNotMapped(dest: typeof(TDest).Name);

            expr.CopyParametersAction(source, dest);
            return dest;
        }
    }

    public static class MapperExtensions
    {
        /// <summary>
        /// Project enumerable with projection
        /// </summary>
        public static IEnumerable<TDest> ProjectTo<TDest>(this IEnumerable source, Mapper map)
            where TDest : class, new()
        {
            Contract.Assert(source != null);
            Contract.Assert(map != null);

            var expr    = map.Mappings[typeof(TDest)] as MapExpression<TDest>;
            var exprMap = expr.CopyParametersAction;

            if (exprMap == null)
                throw new ArgumentException($"{source.GetType().Name} has no mapping to {typeof(TDest).Name}");

            return source.Cast<object>().Select(s =>
            {
                var d = new TDest();
                exprMap(s, d);
                return d;
            });
        }

        /// <summary>
        /// Project query with projection
        /// </summary>
        public static IQueryable<TDest> ProjectTo<TDest>(this IQueryable source, Mapper map)
            where TDest : new()
        {
            Contract.Assert(source != null);
            Contract.Assert(map != null);

            var expr    = map.Mappings[typeof(TDest)] as MapExpression<TDest>;
            var exprMap = expr?.CopyParametersExpr;

            if (exprMap == null)
                throw new ArgumentException($"{source.ElementType.Name} has no mapping to {typeof(TDest).Name}");

            return source.Provider.CreateQuery<TDest>(
                Expression.Call(
                typeof(Queryable), nameof(Queryable.Select),
                new[] { source.ElementType, typeof(TDest) },
                source.Expression, exprMap
            ));
        }
    }
}