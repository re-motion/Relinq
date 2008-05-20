using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class NewExpressionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;
    private readonly Func<Expression, List<IEvaluation>> _parsingCall;

    public NewExpressionParser (QueryModel queryModel, ClauseFieldResolver fieldResolver,Func<Expression,List<IEvaluation>> parsingCall)
    {
      ArgumentUtility.CheckNotNull ("parsingCall", parsingCall);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fieldResolver", fieldResolver);
      
      _queryModel = queryModel;
      _resolver = fieldResolver;
      _parsingCall = parsingCall;
    }

    public virtual List<IEvaluation> Parse(NewExpression newExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("newExpression", newExpression);
      ArgumentUtility.CheckNotNull ("fieldDescriptorCollection", fieldDescriptorCollection);

      List<IEvaluation> evaluations = new List<IEvaluation> ();
      foreach (Expression expression in newExpression.Arguments)
      {
        evaluations.AddRange(_parsingCall (expression));
      }
      return evaluations;
    }
  }
}