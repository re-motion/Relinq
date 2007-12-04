using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.DomainObjects.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq.Parsing
{
  public class SelectProjectionParser
  {
    private readonly SelectClause _selectClause;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly List<Tuple<FromClauseBase, MemberInfo>> _fields = new List<Tuple<FromClauseBase, MemberInfo>> ();

    public SelectProjectionParser (SelectClause selectClause, IDatabaseInfo databaseInfo)
    {
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);

      _selectClause = selectClause;
      _databaseInfo = databaseInfo;

      Expression expression = _selectClause.ProjectionExpression.Body;
      FindSelectedFields (expression);
    }

    public IEnumerable<Tuple<FromClauseBase, MemberInfo>> SelectedFields
    {
      get { return _fields; }
    }

    private FromClauseBase FindFromClauseForExpression(Expression expression)
    {
      string identifierName;

      switch (expression.NodeType)
      {
        case  ExpressionType.Parameter:
          ParameterExpression parameterExpression = (ParameterExpression)expression;
          identifierName = parameterExpression.Name;
          return FindFromClauseForIdentifierName (identifierName);
        case ExpressionType.MemberAccess:
          MemberExpression memberExpression = (MemberExpression) expression;
          identifierName = memberExpression.Member.Name;
          return FindFromClauseForIdentifierName (identifierName);
        default:
          return null;
      }
    }

    private FromClauseBase FindFromClauseForIdentifierName (string identifierName)
    {
      IClause currentClause = _selectClause.PreviousClause;
      while (currentClause != null)
      {
        if (currentClause is FromClauseBase
            && ((FromClauseBase) currentClause).Identifier.Name == identifierName)
          break;
        
        currentClause = currentClause.PreviousClause;
      }
      return (FromClauseBase) currentClause;
    }

    private void FindSelectedFields (Expression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Parameter:
          FindSelectedFields ((ParameterExpression) expression);
          break;
        case ExpressionType.MemberAccess:
          MemberExpression memberExpression = (MemberExpression) expression;
          FindSelectedFields (memberExpression);
          break;
        case ExpressionType.New:
          NewExpression newExpression = (NewExpression) expression;
          FindSelectedFields (newExpression);
          break;
        case ExpressionType.Call:
          MethodCallExpression callExpression = (MethodCallExpression) expression;
          FindSelectedFields (callExpression);
          break;
      }
    }

    private void FindSelectedFields (ParameterExpression expression)
    {
      FromClauseBase fromClause = FindFromClauseForExpression (expression);
      if (fromClause != null)
        _fields.Add (Tuple.NewTuple (fromClause, (MemberInfo) null));
    }

    private void FindSelectedFields (MemberExpression memberExpression)
    {
      FromClauseBase fromClause = FindFromClauseForExpression (memberExpression.Expression);
      if (fromClause != null && _databaseInfo.IsDbColumn(memberExpression.Member))
        _fields.Add (Tuple.NewTuple (fromClause, memberExpression.Member));
    }

    private void FindSelectedFields (NewExpression newExpression)
    {
      foreach (Expression arg in newExpression.Arguments)
        FindSelectedFields (arg);
    }

    private void FindSelectedFields (MethodCallExpression callExpression)
    {
      if (callExpression.Object != null)
        FindSelectedFields (callExpression.Object);
      foreach (Expression arg in callExpression.Arguments)
        FindSelectedFields (arg);
    }


  }
}