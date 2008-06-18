using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class BinaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;

    public BinaryExpressionParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
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

    ICriterion IWhereConditionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((BinaryExpression) expression, fieldDescriptors);
    }

    public bool CanParse (Expression expression)
    {
      return expression is BinaryExpression;
    }

    private ComplexCriterion CreateComplexCriterion 
      (BinaryExpression expression, ComplexCriterion.JunctionKind kind, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new ComplexCriterion (
        _parserRegistry.GetParser (expression.Left).Parse (expression.Left, fieldDescriptorCollection),
        _parserRegistry.GetParser (expression.Right).Parse (expression.Right, fieldDescriptorCollection),
        kind
        );
    }

    private BinaryCondition CreateBinaryCondition 
      (BinaryExpression expression, BinaryCondition.ConditionKind kind, List<FieldDescriptor> fieldDescriptorCollection)
    {
      return new BinaryCondition (
        _parserRegistry.GetParser (expression.Left).Parse (expression.Left, fieldDescriptorCollection),
        _parserRegistry.GetParser (expression.Right).Parse (expression.Right, fieldDescriptorCollection),
        kind
        );
    }
  }
}