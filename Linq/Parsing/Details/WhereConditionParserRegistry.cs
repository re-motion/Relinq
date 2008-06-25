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

    public WhereConditionParserRegistry (IDatabaseInfo databaseInfo)
    {
      _parserRegistry = new ParserRegistry ();
      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, new WhereFieldAccessPolicy (databaseInfo));

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (this));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));
      RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MethodCallExpression), new LikeParser (this));
      RegisterParser (typeof (MethodCallExpression), new ContainsParser (this));
      RegisterParser (typeof (MethodCallExpression), new ContainsFullTextParser (this));
      RegisterParser (typeof (UnaryExpression), new UnaryExpressionParser (this));  
    }

    public IEnumerable<IWhereConditionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<IWhereConditionParser> ();
    }
    
    public virtual IWhereConditionParser GetParser (Expression expression)
    {
      return (IWhereConditionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, IWhereConditionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}