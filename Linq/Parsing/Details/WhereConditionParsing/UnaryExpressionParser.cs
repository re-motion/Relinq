using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class UnaryExpressionParser : IWhereConditionParser
  {
    private readonly WhereConditionParserRegistry _parserRegistry;
    private readonly Expression _expressionTreeRoot;
    
    public UnaryExpressionParser (Expression expressionTreeRoot, WhereConditionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _expressionTreeRoot = expressionTreeRoot;
      _parserRegistry = parserRegistry;
    }

    public ICriterion Parse (UnaryExpression unaryExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      switch (unaryExpression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, fieldDescriptorCollection));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return (_parserRegistry.GetParser (unaryExpression.Operand).Parse (unaryExpression.Operand, fieldDescriptorCollection));
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", unaryExpression.NodeType, "unary expression in where condition",
              _expressionTreeRoot);
      }
    }

    public bool CanParse(Expression expression)
    {
      return expression is UnaryExpression;
    }

    public ICriterion Parse(Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((UnaryExpression) expression, fieldDescriptors);
    }
  }
}