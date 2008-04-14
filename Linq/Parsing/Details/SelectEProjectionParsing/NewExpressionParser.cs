using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details.SelectEProjectionParsing
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
      List<IEvaluation> evaluations = new List<IEvaluation> ();
      foreach (Expression expression in newExpression.Arguments)
      {
        evaluations.AddRange(_parsingCall (expression));
      }
      return evaluations;
    }
  }
}