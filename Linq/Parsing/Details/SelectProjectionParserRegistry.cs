using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using System.Linq;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class SelectProjectionParserRegistry
  {
    private readonly ParserRegistry _parserRegistry;

    public SelectProjectionParserRegistry (IDatabaseInfo databaseInfo, ParseMode parseMode)
    {
      _parserRegistry = new ParserRegistry ();
      
      IResolveFieldAccessPolicy policy;
      if (parseMode == ParseMode.SubQueryInWhere)
        policy = new WhereFieldAccessPolicy (databaseInfo);
      else
        policy = new SelectFieldAccessPolicy();

      ClauseFieldResolver resolver = new ClauseFieldResolver (databaseInfo, policy);

      RegisterParser (typeof (BinaryExpression), new BinaryExpressionParser (this));
      RegisterParser (typeof (ConstantExpression), new ConstantExpressionParser (databaseInfo));
      RegisterParser (typeof (MemberExpression), new MemberExpressionParser (resolver));
      RegisterParser (typeof (MethodCallExpression), new MethodCallExpressionParser (this));
      RegisterParser (typeof (NewExpression), new NewExpressionParser (this));
      RegisterParser (typeof (ParameterExpression), new ParameterExpressionParser (resolver));
    }

    public IEnumerable<ISelectProjectionParser> GetParsers (Type expressionType)
    {
      return _parserRegistry.GetParsers (expressionType).Cast<ISelectProjectionParser>();
    }

    public ISelectProjectionParser GetParser (Expression expression)
    {
      return (ISelectProjectionParser) _parserRegistry.GetParser (expression);
    }

    public void RegisterParser (Type expressionType, ISelectProjectionParser parser)
    {
      _parserRegistry.RegisterParser (expressionType, parser);
    }
  }
}