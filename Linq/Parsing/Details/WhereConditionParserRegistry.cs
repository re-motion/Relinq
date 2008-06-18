using System;
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

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (queryModel.GetExpressionTree(), this));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (queryModel, resolver));
      RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (queryModel, resolver));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MethodCallExpression), new LikeParser (queryModel.GetExpressionTree(), this));
      RegisterParser (typeof (MethodCallExpression), new ContainsParser (queryModel.GetExpressionTree (), this));
      RegisterParser (typeof (MethodCallExpression), new ContainsFullTextParser (queryModel.GetExpressionTree (), this));
      RegisterParser (typeof (UnaryExpression), new UnaryExpressionParser (queryModel.GetExpressionTree(), this));  
    }

    public IEnumerable<IWhereConditionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<IWhereConditionParser> ();
    }
    
    public IWhereConditionParser GetParser (Expression expression)
    {
      return (IWhereConditionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, IWhereConditionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}