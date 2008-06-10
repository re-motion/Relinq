using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class BinaryExpressionParser : IWhereConditionParser<BinaryExpression>, IWhereConditionParser
  {
    private readonly ParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;
    private readonly QueryModel _queryModel;

    public BinaryExpressionParser (QueryModel queryModel, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = queryModel.GetExpressionTree ();
      _parserRegistry = parserRegistry;
      _queryModel = queryModel;
    }


    public ICriterion Parse (BinaryExpression binaryExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      switch (binaryExpression.NodeType)
      {
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return CreateComplexCriterion (binaryExpression, ComplexCriterion.JunctionKind.And, fieldDescriptorCollection);
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return CreateComplexCriterion (binaryExpression, ComplexCriterion.JunctionKind.Or, fieldDescriptorCollection);
        case ExpressionType.Equal:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.Equal, fieldDescriptorCollection);
        case ExpressionType.NotEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.NotEqual, fieldDescriptorCollection);
        case ExpressionType.GreaterThanOrEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.GreaterThanOrEqual, fieldDescriptorCollection);
        case ExpressionType.GreaterThan:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.GreaterThan, fieldDescriptorCollection);
        case ExpressionType.LessThanOrEqual:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.LessThanOrEqual, fieldDescriptorCollection);
        case ExpressionType.LessThan:
          return CreateBinaryCondition (binaryExpression, BinaryCondition.ConditionKind.LessThan, fieldDescriptorCollection);
        default:
          throw ParserUtility.CreateParserException ("and, or, or comparison expression", binaryExpression.NodeType, "binary expression in where condition",
              _expressionTreeRoot);
      }
    }

    public bool CanParse (BinaryExpression binaryExpression)
    {
      return true;
    }

    private ComplexCriterion CreateComplexCriterion 
      (BinaryExpression expression, ComplexCriterion.JunctionKind kind, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new ComplexCriterion (
        GetParsers (expression.Left).Parse (expression.Left, fieldDescriptorCollection),
        GetParsers (expression.Right).Parse (expression.Right, fieldDescriptorCollection),
        kind
        );
    }

    private BinaryCondition CreateBinaryCondition 
      (BinaryExpression expression, BinaryCondition.ConditionKind kind, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (
        GetParsers (expression.Left).Parse (expression.Left, fieldDescriptorCollection),
        GetParsers (expression.Right).Parse (expression.Right, fieldDescriptorCollection),
        kind
        );
    }
    
    public ICriterion Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((BinaryExpression) expression, fieldDescriptors);
    }
    
    private IWhereConditionParser GetParsers (Expression expression)
    {
      if (expression.GetType () == typeof (ConstantExpression))
        return (IWhereConditionParser) _parserRegistry.GetParser ((ConstantExpression) expression);
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