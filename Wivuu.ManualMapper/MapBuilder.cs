using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using static Wivuu.ManualMapper.Expr;

namespace Wivuu.ManualMapper
{
    internal class MemberReplaceVisitor : ExpressionVisitor
    {
        private readonly Expression With;

        public MemberReplaceVisitor(Expression with)
        {
            this.With = with;
        }

        protected override Expression VisitParameter(ParameterExpression node)
            => With;
    }

    public sealed class MapBuilder<TSource, TDest>
    {
        private readonly MapExpression<TDest> Expr;

        private readonly List<Tuple<LambdaExpression, MemberExpression>> Mappings 
            = new List<Tuple<LambdaExpression, MemberExpression>>();

        internal MapBuilder(MapExpression<TDest> expr)
        {
            this.Expr = expr;
        }

        /// <summary>
        /// Map input source property to input destination property
        /// </summary>
        public MapBuilder<TSource, TDest> ForMember<TProp>(
            Expression<Func<TDest, TProp>> dest,
            Expression<Func<TSource, TProp>> source)
        {
            // Add expression
            Mappings.Add(Tuple.Create<LambdaExpression, MemberExpression>(source, dest.Body as MemberExpression));

            return this;
        }

        public void Compile()
        {
            using (var scope = new Scope())
            {
                var srcObj  = scope.Param<object>("srcObj");
                var dest    = scope.Param<TDest>("dest");
                var src     = scope.Var<TSource>("src");
                var visitor = new MemberReplaceVisitor(src);

                var body = new List<Expression>(capacity: 1 + Mappings.Count)
                {
                    Expression.Assign(
                        src, 
                        Expression.Convert(srcObj, typeof(TSource))
                    )
                };

                // Add each mapping to the lambda
                body.AddRange(
                    Mappings.Select(map =>
                    {
                        var srcExpr  = visitor.Visit(map.Item1) as LambdaExpression;
                        var destExpr = map.Item2.Update(dest);

                        return Expression.Assign(destExpr, srcExpr.Body);
                    })
                );

                var expr = Lambda<Action<object, TDest>>(
                    new[] { srcObj, dest },
                    body: scope.Block(body)
                ) as Expression<Action<object, TDest>>;

                // Build member expression
                Expr.CopyParametersExpr = expr;
                Expr.CopyParametersFunc = expr.Compile();
            }
        }
    }

    internal abstract class MapExpression { }

    internal sealed class MapExpression<TDest> : MapExpression
    {
        public Action<object, TDest> CopyParametersFunc;
        public Expression<Action<object, TDest>> CopyParametersExpr;
    }
}
