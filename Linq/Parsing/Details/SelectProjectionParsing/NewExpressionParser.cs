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

    public virtual List<IEvaluation> Parse(NewExpression newExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("newExpression", newExpression);
      ArgumentUtility.CheckNotNull ("fieldDescriptorCollection", fieldDescriptorCollection);

      List<IEvaluation> evaluations = new List<IEvaluation> ();
      foreach (Expression exp in newExpression.Arguments)
      {
        evaluations.AddRange (_parserRegistry.GetParser (exp).Parse (exp, fieldDescriptorCollection));
      }
      return evaluations;
    }

    List<IEvaluation> ISelectProjectionParser.Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((NewExpression) expression, fieldDescriptors);
    }

    public bool CanParse(Expression expression)
    {
      return expression is NewExpression;
    }
  }
}