using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace Wivuu.ManualMapper
{
    public static class Expr
    {
        public static Scope Scope() => new Scope();

        public static Expression<T> Lambda<T>( Expression body)
            => Expression.Lambda<T>(body);

        public static Expression<T> Lambda<T>(
            ParameterExpression[] param, Expression body)
            => Expression.Lambda<T>(body, param);

        public static BlockExpression Block(this Scope scope, params Expression[] expressions)
            => Expression.Block(scope.Variables.Values, expressions);

        public static BlockExpression Block(this Scope scope, IEnumerable<Expression> expressions)
            => Expression.Block(scope.Variables.Values, expressions);

        public static BlockExpression Block(params Expression[] expressions)
            => Expression.Block(expressions);

        public static MemberInitExpression MemberInit(this Scope scope, NewExpression newExpr, IEnumerable<MemberBinding> bindings)
            => Expression.MemberInit(newExpr, bindings);

        public static MethodCallExpression Invoke(this Expression self, MethodInfo method, params Expression[] arguments)
            => Expression.Call(self, method, arguments);

        public static MethodCallExpression Invoke<T>(this Expression self, MethodInfo method, Expression<T> expr)
            => Expression.Call(self, method, expr);

        public static MethodCallExpression Invoke<T>(this MethodInfo method, Expression<T> expr)
            => Expression.Call(method, expr);

        public static MethodCallExpression Invoke(this MethodInfo cb, params Expression[] arguments)
            => Expression.Call(cb, arguments);

        public static Expression New<T>(Expression<T> value) => value.Body;
        public static Expression New(Expression<Action> value) => value.Body;
        public static Expression New<T>(Expression<Action<T>> value) => value.Body;
        public static Expression New<T, K>(Expression<Func<T, K>> value) => value.Body;
    }
}
