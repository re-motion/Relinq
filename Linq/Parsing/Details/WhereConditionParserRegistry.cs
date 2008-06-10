using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class WhereConditionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public WhereConditionParserRegistry (QueryModel queryModel, IDatabaseInfo databaseInfo, JoinedTableContext context)
    {
      _parserRegistry = new ParserRegistry ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, context, new WhereFieldAccessPolicy (databaseInfo));
      _parserRegistry.RegisterParser (new BinaryExpressionParser (queryModel, _parserRegistry));
      _parserRegistry.RegisterParser (new MemberExpressionParser (queryModel, resolver));
      _parserRegistry.RegisterParser (new ParameterExpressionParser (queryModel, resolver));
      _parserRegistry.RegisterParser (new ConstantExpressionParser (databaseInfo)); 
      _parserRegistry.RegisterParser (new MethodCallExpressionParser (queryModel, _parserRegistry));
      _parserRegistry.RegisterParser (new UnaryExpressionParser (queryModel, _parserRegistry));  
    }

    public IWhereConditionParser<TExpression> GetParser<TExpression> (TExpression expression) where TExpression : Expression
    {
      return (IWhereConditionParser<TExpression>) _parserRegistry.GetParser (expression);
    }

    public IEnumerable<IWhereConditionParser<TExpression>> GetParsers<TExpression> () where TExpression : Expression
    {
      return _parserRegistry.GetParsers<TExpression> ().Cast<IWhereConditionParser<TExpression>> ();
    }

    public void RegisterParser<TExpression> (IWhereConditionParser<TExpression> parser) where TExpression : Expression
    {
      _parserRegistry.RegisterParser (parser);
    }

    public IWhereConditionParser GetParser (Expression expression)
    {
      return (IWhereConditionParser) _parserRegistry.GetParser (expression);
    }
  }
}