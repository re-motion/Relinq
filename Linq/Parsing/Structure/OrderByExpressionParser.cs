using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Structure
{
  public class OrderByExpressionParser
  {
    private readonly ParseResultCollector _resultCollector;
    private readonly bool _isTopLevel;
    private readonly SourceExpressionParser _sourceParser = new SourceExpressionParser (false);

    public OrderByExpressionParser (ParseResultCollector resultCollector, MethodCallExpression orderExpression, bool isTopLevel)
    {
      ArgumentUtility.CheckNotNull ("orderExpression", orderExpression);
      ArgumentUtility.CheckNotNull ("resultCollector", resultCollector);

      _resultCollector = resultCollector;
      _isTopLevel = isTopLevel;

      if (orderExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("OrderBy call with two arguments", orderExpression, "OrderBy expressions",
            _resultCollector.ExpressionTreeRoot);

      SourceExpression = orderExpression;

      switch (ParserUtility.CheckMethodCallExpression (orderExpression, _resultCollector.ExpressionTreeRoot,
          "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"))
      {
        case "OrderBy":
          ParseOrderBy (OrderDirection.Asc, true);
          break;
        case "ThenBy":
          ParseOrderBy (OrderDirection.Asc, false);
          break;
        case "OrderByDescending":
          ParseOrderBy (OrderDirection.Desc, true);
          break;
        case "ThenByDescending":
          ParseOrderBy (OrderDirection.Desc, false);
          break;
      }
    }

    public MethodCallExpression SourceExpression { get; private set; }

    private void ParseOrderBy (OrderDirection direction, bool orderBy)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of OrderBy expression", _resultCollector.ExpressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of OrderBy expression", _resultCollector.ExpressionTreeRoot);

      _sourceParser.Parse (_resultCollector, SourceExpression.Arguments[0], ueLambda.Parameters[0], "first argument of OrderBy expression");
            
      _resultCollector.AddBodyExpression (new OrderExpression (orderBy, direction, ueLambda));
      if (_isTopLevel)
        _resultCollector.AddProjectionExpression (null);
    }
  }
}