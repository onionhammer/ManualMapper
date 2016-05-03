using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Wivuu.ManualMapper
{
    public class Scope : IDisposable
    {
        internal readonly Dictionary<string, ParameterExpression> Parameters =
            new Dictionary<string, ParameterExpression>();

        internal readonly Dictionary<string, ParameterExpression> Variables =
            new Dictionary<string, ParameterExpression>();

        public ParameterExpression Param<T>(string ident)
        {
            var result = Expression.Parameter(typeof(T), ident);
            Parameters[ident] = result;
            return result;
        }

        public ParameterExpression Var<T>(string ident)
        {
            var result = Expression.Parameter(typeof(T), ident);
            Variables[ident] = result;
            return result;
        }

        public ParameterExpression Ref(string ident)
        {
            ParameterExpression result;
            return Parameters.TryGetValue(ident, out result) ?
                result : Variables[ident];
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
        }
    }

    class CallbackVisitor : ExpressionVisitor
    {
        private readonly Func<Expression, Expression> OnVisit;

        public CallbackVisitor(Func<Expression, Expression> visit)
        {
            this.OnVisit = visit;
        }

        public override Expression Visit(Expression node)
        {
            return OnVisit(base.Visit(node));
        }
    }
}
