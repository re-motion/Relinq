using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details
{
  public class SelectProjectionParser
  {
    private readonly QueryExpression _queryExpression;
    private readonly SelectClause _selectClause;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly JoinedTableContext _context;

    public SelectProjectionParser (QueryExpression queryExpression, SelectClause selectClause, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      _queryExpression = queryExpression;
      _selectClause = selectClause;
      _databaseInfo = databaseInfo;
      _context = context;
    }

    public IEnumerable<FieldDescriptor> GetSelectedFields ()
    {
      List<FieldDescriptor> fields = new List<FieldDescriptor> ();

      if (_selectClause.ProjectionExpression == null)
      {
        FromClauseBase fromClause = ClauseFinder.FindClause<FromClauseBase> (_selectClause);
        Assertion.IsTrue (fromClause is MainFromClause, "When there are two or more from clauses, there must be a projection expression.");
        Table table = fromClause.GetTable (_databaseInfo);
        Column? column = DatabaseInfoUtility.GetColumn (_databaseInfo, table, null);
        fields.Add (new FieldDescriptor (null, fromClause, table, column));
      }
      else
      {
        Expression expression = _selectClause.ProjectionExpression.Body;
        FindSelectedFields (fields, expression);
      }

      return fields;
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, Expression expression)
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
          ResolveField (fields, parameterExpression);
        else if ((memberExpression = expression as MemberExpression) != null)
          ResolveField (fields, memberExpression);
        else if ((newExpression = expression as NewExpression) != null)
          FindSelectedFields (fields, newExpression);
        else if ((callExpression = expression as MethodCallExpression) != null)
          FindSelectedFields (fields, callExpression);
        else if ((unaryExpression = expression as UnaryExpression) != null)
          FindSelectedFields (fields, unaryExpression);
        else if ((binaryExpression = expression as BinaryExpression) != null)
          FindSelectedFields (fields, binaryExpression);
        else if ((newArrayExpression = expression as NewArrayExpression) != null)
          FindSelectedFields (fields, newArrayExpression);
        else if ((lambdaExpression = expression as LambdaExpression) != null)
          FindSelectedFields (fields, lambdaExpression);
        else if ((invocationExpression = expression as InvocationExpression) != null)
          FindSelectedFields (fields, invocationExpression);
      }
      catch (Exception ex)
      {
        string message = string.Format ("The select clause contains an expression that cannot be parsed: {0}", expression);
        throw new ParserException (message, ex);
      }
    }

    private void ResolveField (List<FieldDescriptor> fields, Expression expression)
    {
      FieldDescriptor fieldDescriptor = _queryExpression.ResolveField (_databaseInfo, _context, expression);
      if (fieldDescriptor.Column != null)
        fields.Add (fieldDescriptor);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, NewExpression newExpression)
    {
      foreach (Expression arg in newExpression.Arguments)
        FindSelectedFields (fields, arg);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, MethodCallExpression callExpression)
    {
      if (callExpression.Object != null)
        FindSelectedFields (fields, callExpression.Object);
      foreach (Expression arg in callExpression.Arguments)
        FindSelectedFields (fields, arg);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, UnaryExpression unaryExpression)
    {
      FindSelectedFields (fields, unaryExpression.Operand);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, BinaryExpression binaryExpression)
    {
      FindSelectedFields (fields, binaryExpression.Left);
      FindSelectedFields (fields, binaryExpression.Right);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, NewArrayExpression newArrayExpression)
    {
      foreach (Expression expression in newArrayExpression.Expressions)
        FindSelectedFields (fields, expression);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, LambdaExpression lambdaExpression)
    {
      FindSelectedFields (fields, lambdaExpression.Body);
    }

    private void FindSelectedFields (List<FieldDescriptor> fields, InvocationExpression invocationExpression)
    {
      foreach (Expression arg in invocationExpression.Arguments)
        FindSelectedFields (fields, arg);
      
      FindSelectedFields (fields, invocationExpression.Expression);
    }
  }
}