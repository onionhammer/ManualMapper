using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Wivuu.ManualMapper
{
    public sealed class Mapper
    {
        public static readonly Mapper Instance = new Mapper();

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
            var dest = new TDest();
            var expr = Mappings[typeof(TDest)] as MapExpression<TDest>;
            expr.CopyParametersAction(source, dest);
            return dest;
        }

        /// <summary>
        /// Map to a new destination
        /// </summary>
        public TDest Map<TDest>(object source, TDest dest)
            where TDest : class, new()
        {
            var expr = Mappings[typeof(TDest)] as MapExpression<TDest>;
            expr.CopyParametersAction(source, dest);
            return dest;
        }
    }

    public static class MapperExtensions
    {
        /// <summary>
        /// Project enumerable with projection
        /// </summary>
        public static IEnumerable<TDest> ProjectTo<TDest>(this IEnumerable source)
            where TDest : class, new() =>
            ProjectTo<TDest>(source, Mapper.Instance);

        /// <summary>
        /// Project enumerable with projection
        /// </summary>
        public static IEnumerable<TDest> ProjectTo<TDest>(this IEnumerable source, Mapper map)
            where TDest : class, new()
        {
            var expr    = map.Mappings[typeof(TDest)] as MapExpression<TDest>;
            var exprMap = expr.CopyParametersAction;

            var xx = source.Cast<object>();
            return xx.Select(s =>
            {
                var d = new TDest();
                exprMap(s, d);
                return d;
            });
        }

        /// <summary>
        /// Project query with projection
        /// </summary>
        public static IQueryable<TDest> ProjectTo<TDest, TEntity>(this IQueryable<TEntity> source) =>
            ProjectTo<TDest, TEntity>(source, Mapper.Instance);

        /// <summary>
        /// Project query with projection
        /// </summary>
        public static IQueryable<TDest> ProjectTo<TDest, TEntity>(this IQueryable<TEntity> source, Mapper map)
        {
            var expr    = map.Mappings[typeof(TDest)] as MapExpression<TDest>;
            var exprMap = expr.CopyParametersExpr as Expression<Func<TEntity, TDest>>;

            return source.Select(exprMap);
        }
    }
}