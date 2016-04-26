using System;
using System.Linq.Expressions;

namespace Wivuu.ManualMapper
{
    public sealed class MapBuilder<TSource, TDest>
    {
        private readonly MapExpression<TDest> Expr;

        internal MapBuilder(MapExpression<TDest> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Map input source property to input destination property
        /// </summary>
        public MapBuilder<TSource, TDest> ForMember<TProp>(
            Expression<Func<TSource, TProp>> source,
            Expression<Func<TDest, TProp>> dest)
        {
            // TODO : Add expression

            return this;
        }

        public void Compile()
        {
            // TODO : Build member expression
            Expr.CopyParametersExpr = null;
            Expr.CopyParametersFunc = null;
        }
    }

    internal abstract class MapExpression { }

    internal sealed class MapExpression<TDest> : MapExpression
    {
        public Action<object, TDest> CopyParametersFunc;
        public Expression<Action<object, TDest>> CopyParametersExpr;
    }
}
