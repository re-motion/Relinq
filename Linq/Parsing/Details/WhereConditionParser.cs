using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class WhereConditionParser
  {
    private readonly bool _simplify;
    private readonly WhereClause _whereClause;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryModel _queryModel;
    private readonly ClauseFieldResolver _resolver;

    private readonly MemberExpressionParser _memberExpressionParser;
    private readonly ParameterExpressionParser _parameterExpressionParser;
    private readonly ConstantExpressionParser _constantExpressionParser;
    private readonly BinaryExpressionParser _binaryExpressionParser;
    private readonly MethodCallExpressionParser _methodCallExpressionParser;
    private readonly UnaryExpressionParser _unaryExpressionParser;

    public delegate ICriterion ParsingOfExpression (Expression expression);
    private List<FieldDescriptor> _fieldDescriptors;

    public WhereConditionParser (QueryModel queryModel, WhereClause whereClause, IDatabaseInfo databaseInfo, JoinedTableContext context, bool simplify)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("context", context);

      _simplify = simplify;
      _queryModel = queryModel;
      _whereClause = whereClause;
      _databaseInfo = databaseInfo;
      _resolver = new ClauseFieldResolver (databaseInfo, context, new WhereFieldAccessPolicy (_databaseInfo));
      
      _memberExpressionParser = new MemberExpressionParser (_queryModel, _resolver);
      _parameterExpressionParser = new ParameterExpressionParser (_queryModel, _resolver);
      _constantExpressionParser = new ConstantExpressionParser (_databaseInfo);
      _binaryExpressionParser = new BinaryExpressionParser (_whereClause, ParseExpression);
      _methodCallExpressionParser = new MethodCallExpressionParser (_whereClause, ParseExpression);
      _unaryExpressionParser = new UnaryExpressionParser (_whereClause, ParseExpression);
    }

    public Tuple<List<FieldDescriptor>, ICriterion> GetParseResult ()
    {
      _fieldDescriptors = new List<FieldDescriptor>();
      LambdaExpression boolExpression = _simplify ? _whereClause.GetSimplifiedBoolExpression() : _whereClause.BoolExpression;
      return Tuple.NewTuple (_fieldDescriptors, ParseExpression (boolExpression.Body));
    }

    private ICriterion ParseExpression (Expression expression)
    {
      if (expression is BinaryExpression)
        return _binaryExpressionParser.Parse ((BinaryExpression) expression);
      else if (expression is ConstantExpression)
        return _constantExpressionParser.Parse ((ConstantExpression) expression);
      else if (expression is MemberExpression)
        return _memberExpressionParser.Parse ((MemberExpression) expression, _fieldDescriptors);
      else if (expression is ParameterExpression)
        return _parameterExpressionParser.Parse ((ParameterExpression) expression, _fieldDescriptors);
      else if (expression is UnaryExpression)
        return _unaryExpressionParser.Parse ((UnaryExpression) expression);
      else if (expression is MethodCallExpression)
        return _methodCallExpressionParser.Parse ((MethodCallExpression) expression);
      throw ParserUtility.CreateParserException ("binary expression, constant expression,method call expression or member expression", expression, "where condition",
          _whereClause.BoolExpression);
    }

  }
}