using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class SelectProjectionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public SelectProjectionParserRegistry (QueryModel queryModel, IDatabaseInfo databaseInfo, JoinedTableContext context, ParseContext parseContext)
    {
      _parserRegistry = new ParserRegistry ();
      
      IResolveFieldAccessPolicy policy;
      if (parseContext == ParseContext.SubQueryInWhere)
        policy = new WhereFieldAccessPolicy (databaseInfo);
      else
        policy = new SelectFieldAccessPolicy();

      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, context, policy);

      _parserRegistry.RegisterParser (new BinaryExpressionParser (queryModel, _parserRegistry));
      _parserRegistry.RegisterParser (new ConstantExpressionParser (databaseInfo));
      _parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));
      _parserRegistry.RegisterParser (new MethodCallExpressionParser (queryModel, _parserRegistry));
      _parserRegistry.RegisterParser (new NewExpressionParser (queryModel, resolver, _parserRegistry));
      _parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
    }

    public ISelectProjectionParser<TExpression> GetParser<TExpression> (TExpression expression) where TExpression : Expression
    {
      return (ISelectProjectionParser<TExpression>) _parserRegistry.GetParser (expression);
    }

    public IEnumerable<ISelectProjectionParser<TExpression>> GetParsers<TExpression> () where TExpression : Expression
    {
      return _parserRegistry.GetParsers<TExpression> ().Cast<ISelectProjectionParser<TExpression>> ();
    }

    public void RegisterParser<TExpression> (ISelectProjectionParser<TExpression> parser) where TExpression : Expression
    {
      _parserRegistry.RegisterParser (parser);
    }

    public ISelectProjectionParser GetParser (Expression expression)
    {
      return (ISelectProjectionParser) _parserRegistry.GetParser (expression);
    }
  }
}