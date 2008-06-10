using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class MethodCallExpressionParser : IWhereConditionParser<MethodCallExpression>, IWhereConditionParser
  {
    private readonly ParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;
    private readonly QueryModel _queryModel;

   public MethodCallExpressionParser (QueryModel queryModel, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

     _expressionTreeRoot = queryModel.GetExpressionTree ();
     _parserRegistry = parserRegistry;
     _queryModel = queryModel;
    }

    public ICriterion Parse (MethodCallExpression methodCallExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      if (methodCallExpression.Method.Name == "StartsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "StartsWith", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "StartsWith", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", fieldDescriptorCollection);
      }
      else if (methodCallExpression.Method.Name == "EndsWith")
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "EndsWith", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "EndsWith", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value, fieldDescriptorCollection);
      }
      else if (methodCallExpression.Method.Name == "Contains" && methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 2, _expressionTreeRoot);
        ParserUtility.CheckParameterType<SubQueryExpression> (methodCallExpression, "Contains", 0, _expressionTreeRoot);
        return CreateContains ((SubQueryExpression) methodCallExpression.Arguments[0], methodCallExpression.Arguments[1], fieldDescriptorCollection);
      }
      else if (methodCallExpression.Method.Name == "Contains" && !methodCallExpression.Method.IsGenericMethod)
      {
        ParserUtility.CheckNumberOfArguments (methodCallExpression, "Contains", 1, _expressionTreeRoot);
        ParserUtility.CheckParameterType<ConstantExpression> (methodCallExpression, "Contains", 0, _expressionTreeRoot);
        return CreateLike (methodCallExpression, "%" + ((ConstantExpression) methodCallExpression.Arguments[0]).Value + "%", fieldDescriptorCollection);
      }
      else if (methodCallExpression.Method.Name == "ContainsFulltext")
      {
        return CreateContainsFulltext (methodCallExpression, (string) ((ConstantExpression) methodCallExpression.Arguments[1]).Value, fieldDescriptorCollection);
      }

      throw ParserUtility.CreateParserException ("StartsWith, EndsWith, Contains, ContainsFulltext", methodCallExpression.Method.Name,
          "method call expression in where condition", _expressionTreeRoot);
    }

    public bool CanParse(MethodCallExpression methodCallExpression)
    {
      throw new System.NotImplementedException();
    }

    private BinaryCondition CreateLike (MethodCallExpression expression, string pattern, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (GetParser (expression.Object).Parse (expression.Object, fieldDescriptorCollection), new Constant (pattern), BinaryCondition.ConditionKind.Like);
    }

    private BinaryCondition CreateContains (SubQueryExpression subQueryExpression, Expression itemExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (new SubQuery (subQueryExpression.QueryModel, null), GetParser(itemExpression).Parse(itemExpression,fieldDescriptorCollection),
          BinaryCondition.ConditionKind.Contains);
    }

    private BinaryCondition CreateContainsFulltext (MethodCallExpression expression, string pattern, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (GetParser (expression.Arguments[0]).Parse (expression.Arguments[0],fieldDescriptorCollection), new Constant (pattern), BinaryCondition.ConditionKind.ContainsFulltext);
    }

    public ICriterion Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((MethodCallExpression) expression, fieldDescriptors);
    }

    private IWhereConditionParser GetParser (Expression expression)
    {
      if (expression.GetType () == typeof (ConstantExpression))
        return  (IWhereConditionParser) _parserRegistry.GetParser ((ConstantExpression) expression);
      else if (expression.GetType () == typeof (BinaryExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((BinaryExpression) expression);
      else if (expression.GetType () == typeof (MemberExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((MemberExpression) expression);
      else if (expression.GetType () == typeof (MethodCallExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((MethodCallExpression) expression);
      else if (expression.GetType () == typeof (ParameterExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((ParameterExpression) expression);
      else if (expression.GetType () == typeof (UnaryExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((UnaryExpression) expression);
      throw ParserUtility.CreateParserException ("no parser for expression found", expression, "GetParser",
        _queryModel.GetExpressionTree ());
    }
  }
}