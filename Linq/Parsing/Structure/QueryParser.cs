using System;
using System.Linq;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class QueryParser
  {
    private readonly ParseResultCollector _resultCollector;

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      SourceExpression = expressionTreeRoot;
      _resultCollector = new ParseResultCollector (expressionTreeRoot);

      new SourceExpressionParser (_resultCollector, SourceExpression, true, null, "parsing query");
    }

    public Expression SourceExpression { get; private set; }

    public QueryExpression GetParsedQuery ()
    {
      MainFromClause mainFromClause = CreateMainFromClause();
      QueryBody queryBody = CreateQueryBody(mainFromClause);

      return new QueryExpression (mainFromClause, queryBody, SourceExpression);
    }

    private MainFromClause CreateMainFromClause ()
    {
      FromExpression mainFromExpression = (FromExpression) _resultCollector.BodyExpressions[0];
      return new MainFromClause (mainFromExpression.Identifier,
          (IQueryable) ((ConstantExpression) mainFromExpression.Expression).Value);
    }

    private QueryBody CreateQueryBody (MainFromClause mainFromClause)
    {
      QueryBodyCreator bodyCreator = new QueryBodyCreator (SourceExpression, mainFromClause, _resultCollector);
      return bodyCreator.GetQueryBody();
    }
  }
}