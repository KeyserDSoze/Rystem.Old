using System;
using System.Linq.Expressions;

namespace Rystem.Azure.NoSql
{
    internal static class ExpressionTypeExtensions
    {
        internal static string MakeLogic(ExpressionType type)
        {
            switch (type)
            {
                default:
                case ExpressionType.AndAlso:
                    return " and ";
                case ExpressionType.And:
                    return " and ";
                case ExpressionType.Or:
                    return " or ";
                case ExpressionType.OrElse:
                    return " or ";
                case ExpressionType.LessThan:
                    return " lt ";
                case ExpressionType.LessThanOrEqual:
                    return " le ";
                case ExpressionType.GreaterThan:
                    return " gt ";
                case ExpressionType.GreaterThanOrEqual:
                    return " ge ";
                case ExpressionType.Equal:
                    return " eq ";
                case ExpressionType.NotEqual:
                    return " ne ";
            }
        }
        internal static bool IsRightASingleValue(ExpressionType type)
        {
            switch (type)
            {
                case ExpressionType.AndAlso:
                case ExpressionType.And:
                case ExpressionType.Or:
                case ExpressionType.OrElse:
                    return false;
                default:
                    return true;
            }
        }
    }
    internal class QueryStrategy
    {
        internal static string Create(Expression expression)
        {
            IExpressionStrategy expressionFactory = new BinaryExpressionStrategy();
            if (expression is MethodCallExpression)
            {
                expressionFactory = new MethodCallerExpressionStrategy();
            }
            else if (expression is UnaryExpression)
            {
                expressionFactory = new UnaryExpressionStrategy();
            }
            else if (expression.GetType().Name == "PropertyExpression")
            {
                expressionFactory = new MemberExpressionStrategy();
            }
            return expressionFactory.Convert(expression);
        }
        internal static string ValueToString(object value)
        {
            if (value is string) return $"'{value.ToString()}'";
            if (value is DateTime) return $"datetime'{((DateTime)value).ToString("yyyy-MM-dd")}T{((DateTime)value).ToString("HH:mm:ss")}Z'";
            if (value is DateTimeOffset) return $"datetime'{((DateTimeOffset)value).ToString("yyyy-MM-dd")}T{((DateTimeOffset)value).ToString("HH:mm:ss")}Z'";
            if (value is Guid) return $"guid'{value.ToString()}'";
            if (value is double) return ((double)value).ToString(new System.Globalization.CultureInfo("en"));
            if (value is float) return ((float)value).ToString(new System.Globalization.CultureInfo("en"));
            if (value is decimal) return ((decimal)value).ToString(new System.Globalization.CultureInfo("en"));
            return value.ToString();
        }
    }
    internal interface IExpressionStrategy
    {
        string Convert(Expression expression);
    }
    internal class BinaryExpressionStrategy : IExpressionStrategy
    {
        public string Convert(Expression expression)
        {
            BinaryExpression binaryExpression = (BinaryExpression)expression;
            if (ExpressionTypeExtensions.IsRightASingleValue(binaryExpression.NodeType))
            {
                dynamic xx = binaryExpression.Left;
                string name = xx.Member.Name;
                object uu = Expression.Lambda(binaryExpression.Right).Compile().DynamicInvoke();
                return name + ExpressionTypeExtensions.MakeLogic(binaryExpression.NodeType) + QueryStrategy.ValueToString(uu);
            }
            return null;
        }
    }
    internal class UnaryExpressionStrategy : IExpressionStrategy
    {
        public string Convert(Expression expression)
        {
            UnaryExpression unaryExpression = (UnaryExpression)expression;
            dynamic aa = unaryExpression.Operand;
            if (aa.Member.PropertyType == typeof(bool))
            {
                //qui ci entra solo per i booleani, altrimenti la query è sbagliata
                return aa.Member.Name + " eq false";
            }
            return null;
        }
    }
    internal class MethodCallerExpressionStrategy : IExpressionStrategy
    {
        public string Convert(Expression expression)
        {
            MethodCallExpression methodCallExpression = (MethodCallExpression)expression;
            if (ExpressionTypeExtensions.IsRightASingleValue(methodCallExpression.NodeType))
            {
                dynamic xx = methodCallExpression.Arguments[0];
                string name = xx.Member.Name;
                object uu = Expression.Lambda(methodCallExpression.Arguments[1]).Compile().DynamicInvoke();
                return name + ExpressionTypeExtensions.MakeLogic((ExpressionType)Enum.Parse(typeof(ExpressionType), methodCallExpression.Method.Name)) + QueryStrategy.ValueToString(uu);
            }
            return null;
        }
    }
    internal class MemberExpressionStrategy : IExpressionStrategy
    {
        public string Convert(Expression expression)
        {
            MemberExpression memberExpression = (MemberExpression)expression;
            dynamic property = memberExpression.Member;
            if (property.PropertyType == typeof(bool))
            {
                //qui ci entra solo per i booleani, altrimenti la query è sbagliata
                string name = property.Name;
                return name + " eq " + "true";
            }
            return null;
        }
    }
}
