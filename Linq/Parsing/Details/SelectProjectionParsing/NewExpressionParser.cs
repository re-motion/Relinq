using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class NewExpressionParser : ISelectProjectionParser
  {
    private readonly SelectProjectionParserRegistry _parserRegistry;

    public NewExpressionParser (SelectProjectionParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);

      _parserRegistry = parserRegistry;
    }

    public virtual List<IEvaluation> Parse (NewExpression newExpression, ParseContext parseContext)
    {
      ArgumentUtility.CheckNotNull ("newExpression", newExpression);
      ArgumentUtility.CheckNotNull ("parseContext", parseContext);

      List<IEvaluation> argumentEvaluations = new List<IEvaluation> ();
      foreach (Expression exp in newExpression.Arguments)
      {
        argumentEvaluations.AddRange (_parserRegistry.GetParser (exp).Parse (exp, parseContext));
      }
      return new List<IEvaluation> { new NewObject(newExpression.Constructor, argumentEvaluations.ToArray()) };
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((NewExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      return expression is NewExpression;
    }
  }
}