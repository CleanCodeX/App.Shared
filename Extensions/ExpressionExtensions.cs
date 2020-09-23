using System;
using System.Linq.Expressions;
using System.Reflection;

namespace Common.Shared.Extensions
{
    
    public static class ExpressionExtensions
    {
        public static PropertyInfo GetPropertyInfo<TInstance, TProp>(this Expression<Func<TInstance, TProp>> getPropertyLambda) => (PropertyInfo)getPropertyLambda.GetMemberInfo();

        public static MemberInfo GetMemberInfo<TInstance, TProp>(this Expression<Func<TInstance, TProp>> getMemberLambda) => getMemberLambda.Body switch
        {
            UnaryExpression unExp when unExp.Operand is MemberExpression operand => operand.Member,
            UnaryExpression _ => throw new ArgumentException(),
            MemberExpression body => body.Member,
            MethodCallExpression instanceMethodCall => instanceMethodCall.Method,
            _ => throw new ArgumentException()
        };

        public static void Parse<T>(this Expression<Func<T>> accessor, out object model, out string fieldName)
        {
            var accessorBody = accessor.Body;

            // Unwrap casts to object
            if (accessorBody is UnaryExpression unaryExpression
                    && unaryExpression.NodeType == ExpressionType.Convert
                    && unaryExpression.Type == typeof(object))
                accessorBody = unaryExpression.Operand;

            if (accessorBody is not MemberExpression memberExpression)
                throw new ArgumentException($"The provided expression contains a {accessorBody.GetType().Name} which is not supported. {nameof(Parse)} only supports simple member accessors (fields, properties) of an object.");

            // Identify the field name. We don't mind whether it's a property or field, or even something else.
            fieldName = memberExpression.Member.Name;

            // Get a reference to the model object
            // i.e., given an value like "(something).MemberName", determine the runtime value of "(something)",
            switch (memberExpression.Expression)
            {
                case ConstantExpression constantExpression:
                    model = constantExpression.Value!;
                    break;
                default:
                    // It would be great to cache this somehow, but it's unclear there's a reasonable way to do
                    // so, given that it embeds captured values such as "this". We could consider special-casing
                    // for "() => something.Member" and building a cache keyed by "something.GetType()" with values
                    // of type Func<object, object> so we can cheaply map from "something" to "something.Member".
                    var modelLambda = Expression.Lambda(memberExpression.Expression!);
                    var modelLambdaCompiled = (Func<object>)modelLambda.Compile();
                    model = modelLambdaCompiled();
                    break;
            }
        }
    }
}