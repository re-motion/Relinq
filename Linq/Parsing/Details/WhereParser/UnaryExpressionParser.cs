using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class UnaryExpressionParser
  {
    private readonly  WhereClause _whereClause;
    private readonly Func<Expression, ICriterion> _parsingCall;

    public UnaryExpressionParser (WhereClause whereClause, Func<Expression, ICriterion> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);

      _whereClause = whereClause;
      _parsingCall = parsingCall;
    }

    public ICriterion Parse (UnaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (_parsingCall (expression.Operand));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return _parsingCall (expression.Operand);
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", expression.NodeType, "unary expression in where condition",
              _whereClause.BoolExpression);
      }
    }
  }
}