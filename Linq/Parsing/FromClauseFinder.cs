using System;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;

namespace Rubicon.Data.Linq.Parsing
{
  public static class FromClauseFinder
  {
    public static FromClauseBase FindFromClauseForExpression (IClause startingPoint, Expression fromIdentifierExpression)
    {
      string identifierName;

      switch (fromIdentifierExpression.NodeType)
      {
        case  ExpressionType.Parameter:
          ParameterExpression parameterExpression = (ParameterExpression)fromIdentifierExpression;
          identifierName = parameterExpression.Name;
          return FindFromClauseForIdentifierName (startingPoint, identifierName);
        case ExpressionType.MemberAccess:
          MemberExpression memberExpression = (MemberExpression) fromIdentifierExpression;
          identifierName = memberExpression.Member.Name;
          return FindFromClauseForIdentifierName (startingPoint, identifierName);
        default:
          string message = string.Format ("The expression cannot be parsed because the expression type {0} is not supported.",
              fromIdentifierExpression.NodeType);
          throw new QueryParserException (message, fromIdentifierExpression, null);
      }
    }

    public static FromClauseBase FindFromClauseForIdentifierName (IClause startingPoint, string identifierName)
    {
      IClause currentClause = startingPoint;
      while (currentClause != null)
      {
        if (currentClause is FromClauseBase
            && ((FromClauseBase) currentClause).Identifier.Name == identifierName)
          break;
        
        currentClause = currentClause.PreviousClause;
      }
      
      if (currentClause == null)
      {
        string message = string.Format ("The identifier '{0}' is not defined in a from clause previous to the given {1}.",
            identifierName, startingPoint.GetType().Name);
        throw new QueryParserException (message);
      }

      return (FromClauseBase) currentClause;
    }
  }
}