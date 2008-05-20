using System.Linq.Expressions;
using Remotion.Utilities;


namespace Remotion.Data.Linq.Parsing.Structure
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

    public QueryModel GetParsedQuery ()
    {
      ParseResultCollector resultCollector = new ParseResultCollector (SourceExpression);
      _sourceParser.Parse (resultCollector, SourceExpression, null, "parsing query");

      QueryModelCreator modelCreator = new QueryModelCreator (SourceExpression, resultCollector);
      return modelCreator.CreateQueryExpression();
    }
  }
}