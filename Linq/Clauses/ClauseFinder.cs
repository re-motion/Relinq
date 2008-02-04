using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;

namespace Rubicon.Data.Linq.Clauses
{
  public static class ClauseFinder
  {
    public static T FindClause<T> (IClause startingPoint) where T : class, IClause
    {
      
      IClause currentClause = startingPoint;
      while (currentClause != null && !(currentClause is T))
        currentClause = currentClause.PreviousClause;
      return (T) currentClause;
    }


    public static IEnumerable<T> FindClauses<T>(IClause startingPoint) where T : class, IClause
    {
      IClause currentClause = startingPoint;
      while (currentClause != null)
      {
        T currentClauseAsT = currentClause as T;
        if (currentClauseAsT != null)
          yield return currentClauseAsT;

        currentClause = currentClause.PreviousClause;
      }
    }

    // Finds the from clause matching either a ParameterExpression (e.g. parameter 's1') or a MemberExpression for a transparent identifier
    // (e.g. '<>h__TransparentIdentifier1.s1'). Do not pass it an expression selecting an entity field (e.g. '<>h__TransparentIdentifier1.s1.First'),
    // but split the surrounding member expression first.
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
          throw new ParserException (message, fromIdentifierExpression, null, null);
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
        throw new ParserException (message);
      }

      return (FromClauseBase) currentClause;
    }
  }
}