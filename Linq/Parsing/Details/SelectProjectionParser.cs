using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Details.SelectEProjectionParsing;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details
{
  public class SelectProjectionParser
  {
    public ParseContext ParseContext { get; private set; }
    private readonly QueryModel _queryModel;
    private readonly Expression _projectionBody;
    private readonly ClauseFieldResolver _resolver;

    private readonly MemberExpressionParser _memberExpressionParser;
    private readonly ParameterExpressionParser _parameterExpressionParser;
    private readonly NewExpressionParser _newExpressionParser;
    

    private List<FieldDescriptor> _fieldDescriptors;

    public SelectProjectionParser (QueryModel queryModel, Expression projectionBody, IDatabaseInfo databaseInfo, JoinedTableContext context, 
      ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("context", context);

      _queryModel = queryModel;
      _projectionBody = projectionBody;

      Assertion.IsNotNull (_projectionBody);
      ParseContext = parseContext;

      IResolveFieldAccessPolicy policy;
      if (parseContext == ParseContext.SubQueryInWhere)
        policy = new WhereFieldAccessPolicy (databaseInfo);
      else
        policy = new SelectFieldAccessPolicy();

      _resolver = new ClauseFieldResolver (databaseInfo, context, policy);

      _memberExpressionParser = new MemberExpressionParser (_queryModel, _resolver);
      _parameterExpressionParser = new ParameterExpressionParser (_queryModel, _resolver);
      _newExpressionParser = new NewExpressionParser (_queryModel, _resolver,ParseExpression);
      
    }

    public Tuple<List<FieldDescriptor>, List<IEvaluation>> GetParseResult ()
    {
      _fieldDescriptors = new List<FieldDescriptor> ();
      return Tuple.NewTuple (_fieldDescriptors, ParseExpression (_projectionBody));
    }

    private List<IEvaluation> ParseExpression (Expression expression)
    {
      List<IEvaluation> evaluations = new List<IEvaluation> ();
      if (expression is ParameterExpression)
      {
        IEvaluation evaluation = _parameterExpressionParser.Parse ((ParameterExpression) expression, _fieldDescriptors);
        evaluations.Add (evaluation);
        return evaluations;
      }
      else if (expression is MemberExpression)
      {
        IEvaluation evaluation = _memberExpressionParser.Parse ((MemberExpression) expression, _fieldDescriptors);
        evaluations.Add (evaluation);
        return evaluations;
      }
      else if (expression is NewExpression)
        return _newExpressionParser.Parse ((NewExpression) expression, _fieldDescriptors);
      throw ParserUtility.CreateParserException ("member expression, parameter expression or new expression", expression, "select projection",
          _projectionBody);
    }

    //private void FindSelectedFields (List<FieldDescriptor> fields, Expression expression)
    //{
    //  NewExpression newExpression;
    //  MethodCallExpression callExpression;
    //  UnaryExpression unaryExpression;
    //  BinaryExpression binaryExpression;
    //  NewArrayExpression newArrayExpression;
    //  LambdaExpression lambdaExpression;
    //  InvocationExpression invocationExpression;

    //  try
    //  {
    //    if (expression is ParameterExpression || expression is MemberExpression)
    //      ResolveField (fields, expression);
    //    else if ((newExpression = expression as NewExpression) != null)
    //      FindSelectedFields (fields, newExpression);
    //    else if ((callExpression = expression as MethodCallExpression) != null)
    //      FindSelectedFields (fields, callExpression);
    //    else if ((unaryExpression = expression as UnaryExpression) != null)
    //      FindSelectedFields (fields, unaryExpression);
    //    else if ((binaryExpression = expression as BinaryExpression) != null)
    //      FindSelectedFields (fields, binaryExpression);
    //    else if ((newArrayExpression = expression as NewArrayExpression) != null)
    //      FindSelectedFields (fields, newArrayExpression);
    //    else if ((lambdaExpression = expression as LambdaExpression) != null)
    //      FindSelectedFields (fields, lambdaExpression);
    //    else if ((invocationExpression = expression as InvocationExpression) != null)
    //      FindSelectedFields (fields, invocationExpression);
    //  }
    //  catch (Exception ex)
    //  {
    //    string message = string.Format ("The select clause contains an expression that cannot be parsed: {0}", expression);
    //    throw new ParserException (message, ex);
    //  }
    //}

    //private void ResolveField (List<FieldDescriptor> fields, Expression expression)
    //{
    //  FieldDescriptor fieldDescriptor = _queryModel.ResolveField (_resolver, expression);
    //  if (fieldDescriptor.Column != null)
    //    fields.Add (fieldDescriptor);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, NewExpression newExpression)
    //{
    //  foreach (Expression arg in newExpression.Arguments)
    //    FindSelectedFields (fields, arg);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, MethodCallExpression callExpression)
    //{
    //  if (callExpression.Object != null)
    //    FindSelectedFields (fields, callExpression.Object);
    //  foreach (Expression arg in callExpression.Arguments)
    //    FindSelectedFields (fields, arg);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, UnaryExpression unaryExpression)
    //{
    //  FindSelectedFields (fields, unaryExpression.Operand);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, BinaryExpression binaryExpression)
    //{
    //  FindSelectedFields (fields, binaryExpression.Left);
    //  FindSelectedFields (fields, binaryExpression.Right);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, NewArrayExpression newArrayExpression)
    //{
    //  foreach (Expression expression in newArrayExpression.Expressions)
    //    FindSelectedFields (fields, expression);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, LambdaExpression lambdaExpression)
    //{
    //  FindSelectedFields (fields, lambdaExpression.Body);
    //}

    //private void FindSelectedFields (List<FieldDescriptor> fields, InvocationExpression invocationExpression)
    //{
    //  foreach (Expression arg in invocationExpression.Arguments)
    //    FindSelectedFields (fields, arg);
      
    //  FindSelectedFields (fields, invocationExpression.Expression);
    //}
  }
}