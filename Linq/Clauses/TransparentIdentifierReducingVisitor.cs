using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Clauses
{
  public class TransparentIdentifierReducingVisitor : ExpressionTreeVisitor
  {
    private readonly ParameterExpression[] _transparentIdentifiers;

    public TransparentIdentifierReducingVisitor (ParameterExpression[] transparentIdentifiers)
    {
      ArgumentUtility.CheckNotNull ("transparentIdentifiers", transparentIdentifiers);
      _transparentIdentifiers = transparentIdentifiers;
    }

    protected MemberInfo Member { get; private set; }
    protected Expression ExpressionTreeRoot { get; private set; }


    public Expression ParseAndReduce (Expression expression, Expression expressionTreeRoot)
    {
      ExpressionTreeRoot = expressionTreeRoot;
      Expression reducedExpression = VisitExpression (expression);
      return reducedExpression;
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      ParameterExpression transparentIdentifier = _transparentIdentifiers.FirstOrDefault (p => p.Name == expression.Name);
      if (transparentIdentifier != null)
      {
        CheckType (transparentIdentifier.Name, transparentIdentifier.Type, expression.Type);
        return null;
      }
      else
        return base.VisitParameterExpression (expression);
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      Member = expression.Member;
      Expression newExpression = VisitExpression (expression.Expression);
      if (newExpression != expression.Expression)
      {
        if (newExpression == null)
          return Expression.Parameter (expression.Type, expression.Member.Name);
        else
          return Expression.MakeMemberAccess (newExpression, expression.Member);
      }
      return expression;
    }

    protected void CheckType (string identifierName, Type expected, Type actual)
    {
      if (expected != actual)
      {
        string message = string.Format ("The identifier '{0}' has a different type ({1}) than expected ({2}) in expression '{3}'.",
            identifierName, actual, expected, ExpressionTreeRoot);
        throw new ParserException (message);
      }
    }
  }
}