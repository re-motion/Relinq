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
    private readonly bool _isTopLevel;
    private readonly List<BodyExpressionBase> _bodyExpressions = new List<BodyExpressionBase> ();
    private readonly List<LambdaExpression> _projectionExpressions = new List<LambdaExpression> ();

    public OrderByExpressionParser (MethodCallExpression orderExpression, Expression expressionTreeRoot, bool isTopLevel)
    {
      ArgumentUtility.CheckNotNull ("orderExpression", orderExpression);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);    

      _isTopLevel = isTopLevel;

      if (orderExpression.Arguments.Count != 2)
        throw ParserUtility.CreateParserException ("OrderBy call with two arguments", orderExpression, "OrderBy expressions",
            expressionTreeRoot);

      SourceExpression = orderExpression;

      switch (ParserUtility.CheckMethodCallExpression (orderExpression, expressionTreeRoot,
          "OrderBy", "OrderByDescending", "ThenBy", "ThenByDescending"))
      {
        case "OrderBy":
          ParseOrderBy (expressionTreeRoot, OrderDirection.Asc, true);
          break;
        case "ThenBy":
          ParseOrderBy (expressionTreeRoot, OrderDirection.Asc, false);
          break;
        case "OrderByDescending":
          ParseOrderBy (expressionTreeRoot, OrderDirection.Desc, true);
          break;
        case "ThenByDescending":
          ParseOrderBy (expressionTreeRoot, OrderDirection.Desc, false);
          break;
      }

    }

    private void ParseOrderBy (Expression expressionTreeRoot, OrderDirection direction, bool orderBy)
    {
      UnaryExpression unaryExpression = ParserUtility.GetTypedExpression<UnaryExpression> (SourceExpression.Arguments[1],
          "second argument of OrderBy expression", expressionTreeRoot);
      LambdaExpression ueLambda = ParserUtility.GetTypedExpression<LambdaExpression> (unaryExpression.Operand,
          "second argument of OrderBy expression", expressionTreeRoot);

      SourceExpressionParser sourceExpressionParser = new SourceExpressionParser (SourceExpression.Arguments[0], expressionTreeRoot, false,
          ueLambda.Parameters[0], "first argument of OrderBy expression");
            
      _bodyExpressions.AddRange (sourceExpressionParser.BodyExpressions);
      _bodyExpressions.Add (new OrderExpression (orderBy, direction, ueLambda));


      _projectionExpressions.AddRange (sourceExpressionParser.ProjectionExpressions);

      if (_isTopLevel)
        _projectionExpressions.Add (null);
      
    }


    public MethodCallExpression SourceExpression { get; private set; }

    public ReadOnlyCollection<BodyExpressionBase> BodyExpressions
    {
      get { return _bodyExpressions.AsReadOnly(); }
    }

    public ReadOnlyCollection<LambdaExpression> ProjectionExpressions
    {
      get { return _projectionExpressions.AsReadOnly(); }
    }
  }
}