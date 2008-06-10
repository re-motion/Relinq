using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class UnaryExpressionParser : IWhereConditionParser<UnaryExpression>, IWhereConditionParser
  {
    private readonly ParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;
    private readonly QueryModel _queryModel;

    public UnaryExpressionParser (QueryModel queryModel, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = queryModel.GetExpressionTree ();
      _parserRegistry = parserRegistry;
      _queryModel = queryModel;
    }

    public ICriterion Parse (UnaryExpression unaryExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      switch (unaryExpression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, fieldDescriptorCollection));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return (GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, fieldDescriptorCollection));
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", unaryExpression.NodeType, "unary expression in where condition",
              _expressionTreeRoot);
      }
    }

    public bool CanParse(UnaryExpression unaryExpression)
    {
      return true;
    }

    public ICriterion Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((UnaryExpression) expression, fieldDescriptors);
    }

    private IWhereConditionParser GetParser (Expression expression)
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