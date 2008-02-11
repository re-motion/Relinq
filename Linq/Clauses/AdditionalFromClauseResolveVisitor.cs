using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;
using System.Reflection;

namespace Rubicon.Data.Linq.Clauses
{
  public class AdditionalFromClauseResolveVisitor : ExpressionTreeVisitor
  {
    public struct Result
    {
      public Result (bool fromIdentifierFound, Expression reducedExpression, MemberInfo member) : this()
      {
        ArgumentUtility.CheckNotNull ("reducedExpression", reducedExpression);

        FromIdentifierFound = fromIdentifierFound;
        ReducedExpression = reducedExpression;
        Member = member;
      }

      public bool FromIdentifierFound { get; private set; }
      public Expression ReducedExpression { get; private set; }
      public MemberInfo Member { get; private set; }
    }

    private readonly ParameterExpression _fromIdentifier;
    private readonly ParameterExpression[] _transparentIdentifiers;

    private Expression _expressionTreeRoot;
    private bool _fromIdentifierFound;
    private MemberInfo _member;

    public AdditionalFromClauseResolveVisitor (ParameterExpression fromIdentifier, ParameterExpression[] transparentIdentifiers)
    {
      ArgumentUtility.CheckNotNull ("fromIdentifier", fromIdentifier);

      _fromIdentifier = fromIdentifier;
      _transparentIdentifiers = transparentIdentifiers;
    }

    public Result ParseAndReduce (Expression expression, Expression expressionTreeRoot)
    {
      _expressionTreeRoot = expressionTreeRoot;
      _fromIdentifierFound = false;
      Expression reducedExpression = VisitExpression (expression);
      return new Result (_fromIdentifierFound, reducedExpression, _member);
    }

    protected override Expression VisitExpression (Expression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Parameter:
        case ExpressionType.MemberAccess:
          return base.VisitExpression (expression);
        default:
          throw ParserUtility.CreateParserException ("ParameterExpression or MemberExpression", expression, "resolving field access in additional "
              + "from clause", _expressionTreeRoot);
      }
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      if (expression.Name == _fromIdentifier.Name)
      {
        CheckType (_fromIdentifier.Name, _fromIdentifier.Type, expression.Type);
        _fromIdentifierFound = true;
      }
      else
      {
        ParameterExpression transparentIdentifier = _transparentIdentifiers.FirstOrDefault (p => p.Name == expression.Name);
        if (transparentIdentifier != null)
        {
          CheckType (transparentIdentifier.Name, transparentIdentifier.Type, expression.Type);
          return null;
        }
      }
      return base.VisitParameterExpression (expression);
    }

    private void CheckType (string identifierName, Type expected, Type actual)
    {
      if (expected != actual)
      {
        string message = string.Format ("The identifier '{0}' has a different type ({1}) than expected ({2}) in expression '{3}'.",
            identifierName, actual, expected, _expressionTreeRoot);
        throw new ParserException (message);
      }
    }

    protected override Expression VisitMemberExpression (MemberExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);
      _member = expression.Member;
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
  }
}