using System;
using System.Collections.Generic;

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
            expr.CopyParametersFunc(source, dest);
            return dest;
        }

        /// <summary>
        /// Map to a new destination
        /// </summary>
        public TDest Map<TDest>(object source, TDest dest)
            where TDest : class, new()
        {
            var expr = Mappings[typeof(TDest)] as MapExpression<TDest>;
            expr.CopyParametersFunc(source, dest);
            return dest;
        }
    }
}