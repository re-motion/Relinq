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
  public class AdditionalFromClauseResolveVisitor : TransparentIdentifierReducingVisitor
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

    private bool _fromIdentifierFound;

    public AdditionalFromClauseResolveVisitor (ParameterExpression fromIdentifier, ParameterExpression[] transparentIdentifiers)
        : base (transparentIdentifiers)
    {
      ArgumentUtility.CheckNotNull ("fromIdentifier", fromIdentifier);
      _fromIdentifier = fromIdentifier;
    }

    public new Result ParseAndReduce (Expression expression, Expression expressionTreeRoot)
    {
      _fromIdentifierFound = false;
      Expression reducedExpression = base.ParseAndReduce (expressionTreeRoot, expressionTreeRoot);
      return new Result (_fromIdentifierFound, reducedExpression, Member);
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
              + "from clause", ExpressionTreeRoot);
      }
    }

    protected override Expression VisitParameterExpression (ParameterExpression expression)
    {
      if (expression.Name == _fromIdentifier.Name)
      {
        CheckType (_fromIdentifier.Name, _fromIdentifier.Type, expression.Type);
        _fromIdentifierFound = true;
        return expression;
      }
      else
        return base.VisitParameterExpression (expression);
    }
  }
}