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
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (true);

    public QueryParser (Expression expressionTreeRoot)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      SourceExpression = expressionTreeRoot;
    }

    public Expression SourceExpression { get; private set; }

    public QueryExpression GetParsedQuery ()
    {
      ParseResultCollector resultCollector = new ParseResultCollector (SourceExpression);
      _sourceParser.Parse (resultCollector, SourceExpression, null, "parsing query");

      QueryExpressionCreator expressionCreator = new QueryExpressionCreator (SourceExpression, resultCollector);
      return expressionCreator.CreateQueryExpression();
    }
  }
}