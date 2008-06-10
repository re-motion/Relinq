using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing
{
  public class NewExpressionParser : ISelectProjectionParser<NewExpression>, ISelectProjectionParser
  {
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;
    private readonly ParserRegistry _parserRegistry;

    public NewExpressionParser (QueryModel queryModel, ClauseFieldResolver fieldResolver, ParserRegistry parserRegistry)
    {
      ArgumentUtility.CheckNotNull ("parserRegistry", parserRegistry);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("fieldResolver", fieldResolver);

      _queryModel = queryModel;
      _resolver = fieldResolver;
      _parserRegistry = parserRegistry;
    }

    public virtual List<IEvaluation> Parse(NewExpression newExpression, List<FieldDescriptor> fieldDescriptorCollection)
    {
      ArgumentUtility.CheckNotNull ("newExpression", newExpression);
      ArgumentUtility.CheckNotNull ("fieldDescriptorCollection", fieldDescriptorCollection);

      List<IEvaluation> evaluations = new List<IEvaluation> ();
      foreach (Expression exp in newExpression.Arguments)
      {
        evaluations.AddRange (GetParser (exp).Parse (exp, fieldDescriptorCollection));
      }
      return evaluations;
    }

    public bool CanParse(NewExpression newExpression)
    {
      return true;
    }
    
    public List<IEvaluation> Parse (Expression expression, List<FieldDescriptor> fieldDescriptors)
    {
      return Parse ((NewExpression) expression, fieldDescriptors);
    }

    private ISelectProjectionParser GetParser (Expression expression)
    {
      if (expression.GetType () == typeof (ConstantExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ConstantExpression) expression);
      else if (expression.GetType () == typeof (BinaryExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((BinaryExpression) expression);
      else if (expression.GetType () == typeof (MemberExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MemberExpression) expression);
      else if (expression.GetType () == typeof (MethodCallExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((MethodCallExpression) expression);
      else if (expression.GetType () == typeof (ParameterExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((ParameterExpression) expression);
      else if (expression.GetType () == typeof (NewExpression))
        return (ISelectProjectionParser) _parserRegistry.GetParser ((NewExpression) expression);
      throw ParserUtility.CreateParserException ("no parser for expression found", expression, "GetParser",
        _queryModel.GetExpressionTree ());
    }
  }
}