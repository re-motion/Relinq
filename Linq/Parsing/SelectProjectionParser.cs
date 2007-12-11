using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
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

      if (_selectClause.ProjectionExpression == null)
      {
        FromClauseBase fromClause = ClauseFinderHelper.FindClause<FromClauseBase> (_selectClause);
        Assertion.IsTrue (fromClause is MainFromClause, "When there are two or more from clauses, there must be a projection expression.");
        _fields.Add (Tuple.NewTuple (fromClause, (MemberInfo) null));
      }
      else
      {
        Expression expression = _selectClause.ProjectionExpression.Body;
        FindSelectedFields (expression);
      }
    }

    public IEnumerable<Tuple<FromClauseBase, MemberInfo>> SelectedFields
    {
      get { return _fields; }
    }

    private void FindSelectedFields (Expression expression)
    {
      ParameterExpression parameterExpression;
      MemberExpression memberExpression;
      NewExpression newExpression;
      MethodCallExpression callExpression;
      UnaryExpression unaryExpression;
      BinaryExpression binaryExpression;
      NewArrayExpression newArrayExpression;
      LambdaExpression lambdaExpression;
      InvocationExpression invocationExpression;

      try
      {
        if ((parameterExpression = expression as ParameterExpression) != null)
          FindSelectedFields (parameterExpression);
        else if ((memberExpression = expression as MemberExpression) != null)
          FindSelectedFields (memberExpression);
        else if ((newExpression = expression as NewExpression) != null)
          FindSelectedFields (newExpression);
        else if ((callExpression = expression as MethodCallExpression) != null)
          FindSelectedFields (callExpression);
        else if ((unaryExpression = expression as UnaryExpression) != null)
          FindSelectedFields (unaryExpression);
        else if ((binaryExpression = expression as BinaryExpression) != null)
          FindSelectedFields (binaryExpression);
        else if ((newArrayExpression = expression as NewArrayExpression) != null)
          FindSelectedFields (newArrayExpression);
        else if ((lambdaExpression = expression as LambdaExpression) != null)
          FindSelectedFields (lambdaExpression);
        else if ((invocationExpression = expression as InvocationExpression) != null)
          FindSelectedFields (invocationExpression);
      }
      catch (QueryParserException ex)
      {
        string message = string.Format ("The select clause contains an expression that cannot be parsed: {0}", expression);
        throw new QueryParserException (message, ex);
      }
    }

    private void FindSelectedFields (ParameterExpression expression)
    {
      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (_selectClause, expression);
      if (fromClause != null)
        _fields.Add (Tuple.NewTuple (fromClause, (MemberInfo) null));
    }

    private void FindSelectedFields (MemberExpression memberExpression)
    {
      FromClauseBase fromClause = FromClauseFinder.FindFromClauseForExpression (_selectClause, memberExpression.Expression);
      if (fromClause != null && _databaseInfo.GetColumnName (memberExpression.Member) != null)
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

    private void FindSelectedFields (UnaryExpression unaryExpression)
    {
      FindSelectedFields (unaryExpression.Operand);
    }

    private void FindSelectedFields (BinaryExpression binaryExpression)
    {
      FindSelectedFields (binaryExpression.Left);
      FindSelectedFields (binaryExpression.Right);
    }

    private void FindSelectedFields (NewArrayExpression newArrayExpression)
    {
      foreach (Expression expression in newArrayExpression.Expressions)
        FindSelectedFields (expression);
    }

    private void FindSelectedFields (LambdaExpression lambdaExpression)
    {
      FindSelectedFields (lambdaExpression.Body);
    }

    private void FindSelectedFields (InvocationExpression invocationExpression)
    {
      foreach (Expression arg in invocationExpression.Arguments)
        FindSelectedFields (arg);
      
      FindSelectedFields (invocationExpression.Expression);
    }


  }
}