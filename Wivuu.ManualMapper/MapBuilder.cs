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

        /// <summary>
        /// Compile mapping expressions
        /// </summary>
        public void Compile()
        {
            Expr.CopyParametersExpr = BuildQueryFunc();
            Expr.CopyParametersFunc = BuildAction();
        }

        private void AddProperties(List<Expression> body, MemberReplaceVisitor visitor, Expression dest) =>
            body.AddRange(
                Mappings.Select(map =>
                {
                    var srcExpr  = visitor.Visit(map.Item1) as LambdaExpression;
                    var destExpr = map.Item2.Update(dest);

                    return Expression.Assign(destExpr, srcExpr.Body);
                })
            );

        private Expression<Func<TSource, TDest>> BuildQueryFunc()
        {
            using (var scope = new Scope())
            {
                var dest    = scope.Var<TDest>("dest");
                var src     = scope.Param<TSource>("src");
                var visitor = new MemberReplaceVisitor(src);

                var body = new List<Expression>(capacity: 2 + Mappings.Count)
                {
                    Expression.Assign(
                        dest, Expression.New(typeof(TDest))
                    )
                };

                AddProperties(body, visitor, dest);

                body.Add(dest);

                // Add each mapping to the lambda
                var expr = Lambda<Func<TSource, TDest>>(
                    param: new[] { src },
                    body: scope.Block(body)
                );

                // Build member expression
                return expr;
            }
        }

        private Action<object, TDest> BuildAction()
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
                AddProperties(body, visitor, dest);

                var expr = Lambda<Action<object, TDest>>(
                    param: new[] { srcObj, dest },
                    body:  scope.Block(body)
                ) as Expression<Action<object, TDest>>;

                // Build member expression
                return expr.Compile();
            }
        }
    }

    internal abstract class MapExpression { }

    internal sealed class MapExpression<TDest> : MapExpression
    {
        public Action<object, TDest> CopyParametersFunc;
        public Expression CopyParametersExpr;
    }
}
