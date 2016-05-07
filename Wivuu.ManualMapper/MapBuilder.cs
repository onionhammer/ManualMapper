using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Linq.Expressions;
using static Wivuu.ManualMapper.Expr;

namespace Wivuu.ManualMapper
{
    /// <summary>
    /// Replace instances of source object within source lambda body
    /// </summary>
    internal class MemberReplaceVisitor : ExpressionVisitor
    {
        private bool Once = false;
        private readonly Expression With;

        public MemberReplaceVisitor(Expression with)
        {
            this.With = with;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            if (!Once && node.Type == With.Type)
            {
                Once = true;
                return With;
            }

            return node;
        }
    }

    /// <summary>
    /// Build to create mappings
    /// </summary>
    public sealed class MapBuilder<TSource, TDest>
    {
        private readonly MapExpression<TDest> Expr;

        private readonly List<Tuple<LambdaExpression, MemberExpression>> Mappings 
            = new List<Tuple<LambdaExpression, MemberExpression>>();

        internal MapBuilder(MapExpression<TDest> expr)
        {
            Contract.Assert(expr != null);

            this.Expr = expr;
        }

        /// <summary>
        /// Map input source property to input destination property
        /// </summary>
        public MapBuilder<TSource, TDest> ForMember<TProp>(
            Expression<Func<TDest, TProp>> dest,
            Expression<Func<TSource, TProp>> source)
        {
            Contract.Assert(dest != null);
            Contract.Assert(source != null);
            Contract.Assert(source.ReturnType == source.ReturnType);

            // Add expression
            Mappings.Add(Tuple.Create<LambdaExpression, MemberExpression>(
                source, dest.Body as MemberExpression));

            return this;
        }

        /// <summary>
        /// Compile mapping expressions
        /// </summary>
        public void Compile()
        {
            Expr.CopyParametersExpr   = BuildExpr();
            Expr.CopyParametersAction = BuildAction();
        }

        private Expression<Func<TSource, TDest>> BuildExpr()
        {
            using (var scope = new Scope())
            {
                var src = scope.Param<TSource>("src");

                // Build member expression
                return Lambda<Func<TSource, TDest>>(
                    param: new[] { src },
                    body: scope.MemberInit(
                        newExpr:  Expression.New(typeof(TDest)),
                        bindings: from map in Mappings
                                  select Expression.Bind(
                                      map.Item2.Member,
                                      new MemberReplaceVisitor(src).Visit(map.Item1.Body)
                                  )
                    )
                );
            }
        }

        private Action<object, TDest> BuildAction()
        {
            using (var scope = new Scope())
            {
                var srcObj  = scope.Param<object>("srcObj");
                var dest    = scope.Param<TDest>("dest");
                var src     = scope.Var<TSource>("src");
                var body    = new List<Expression>(capacity: 1 + Mappings.Count)
                {
                    Expression.Assign(
                        src, 
                        Expression.TypeAs(srcObj, typeof(TSource))
                    )
                };

                // Add each mapping to the lambda
                body.AddRange(
                    Mappings.Select(map =>
                        Expression.Assign(
                            map.Item2.Update(dest),
                            new MemberReplaceVisitor(src).Visit(map.Item1.Body))
                    )
                );

                // Build member expression
                return Lambda<Action<object, TDest>>(
                    param: new[] { srcObj, dest },
                    body:  scope.Block(body)
                ).Compile();
            }
        }
    }

    internal abstract class MapExpression { }

    internal sealed class MapExpression<TDest> : MapExpression
    {
        public Action<object, TDest> CopyParametersAction;
        public Expression CopyParametersExpr;
    }
}