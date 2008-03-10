using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereParser
{
  public class BinaryExpressionParser
  {
    private readonly  WhereClause _whereClause;
    private readonly Func<Expression, ICriterion> _parsingCall;

    public BinaryExpressionParser (WhereClause whereClause, Func<Expression, ICriterion> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);

      _whereClause = whereClause;
      _parsingCall = parsingCall;
    }

    public ICriterion Parse (BinaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return CreateComplexCriterion (expression, ComplexCriterion.JunctionKind.And);
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return CreateComplexCriterion (expression, ComplexCriterion.JunctionKind.Or);
        case ExpressionType.Equal:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.Equal);
        case ExpressionType.NotEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.NotEqual);
        case ExpressionType.GreaterThanOrEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.GreaterThanOrEqual);
        case ExpressionType.GreaterThan:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.GreaterThan);
        case ExpressionType.LessThanOrEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.LessThanOrEqual);
        case ExpressionType.LessThan:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.LessThan);
        default:
          throw ParserUtility.CreateParserException ("and, or, or comparison expression", expression.NodeType, "binary expression in where condition",
              _whereClause.BoolExpression);
      }
    }

    private ComplexCriterion CreateComplexCriterion 
      (BinaryExpression expression, ComplexCriterion.JunctionKind kind)
    {
      return new ComplexCriterion (_parsingCall (expression.Left), _parsingCall (expression.Right), kind);
    }

    private BinaryCondition CreateBinaryCondition 
      (BinaryExpression expression, BinaryCondition.ConditionKind kind)
    {
      return new BinaryCondition (_parsingCall (expression.Left), _parsingCall (expression.Right), kind);
    }
  }
}