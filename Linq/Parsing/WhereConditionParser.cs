using System;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing
{
  public class WhereConditionParser
  {
    private readonly bool _simplify;
    private readonly WhereClause _whereClause;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryExpression _queryExpression;

    public WhereConditionParser (QueryExpression queryExpression, WhereClause whereClause, IDatabaseInfo databaseInfo, bool simplify)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);

      _simplify = simplify;
      _queryExpression = queryExpression;
      _whereClause = whereClause;
      _databaseInfo = databaseInfo;
    }

    public ICriterion GetCriterion ()
    {
      LambdaExpression boolExpression = _simplify ? _whereClause.GetSimplifiedBoolExpression() : _whereClause.BoolExpression;
      return ParseExpression (boolExpression.Body);
    }

    private ICriterion ParseExpression (Expression expression)
    {
      if (expression is BinaryExpression)
        return ParseBinaryExpression ((BinaryExpression) expression);
      else if (expression is ConstantExpression)
        return ParseConstantExpression ((ConstantExpression) expression);
      else if (expression is MemberExpression)
        return ParseMemberExpression ((MemberExpression) expression);
      else if (expression is UnaryExpression)
        return ParseUnaryExpression ((UnaryExpression) expression);
      else if (expression is MethodCallExpression)
        return ParseMethodCallExpression((MethodCallExpression)expression);
      throw ParserUtility.CreateParserException ("binary expression, constant expression,method call expression or member expression", expression, "where condition",
            _whereClause.BoolExpression);
    }

    private ICriterion ParseMemberExpression (MemberExpression expression)
    {
      return _queryExpression.ResolveField (_databaseInfo, expression).GetMandatoryColumn();
    }

    private ICriterion ParseConstantExpression (ConstantExpression expression)
    {
      return new Constant (expression.Value);
    }

    private ICriterion ParseBinaryExpression (BinaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.And:
        case ExpressionType.AndAlso:
          return new ComplexCriterion (ParseExpression (expression.Left), ParseExpression (expression.Right), ComplexCriterion.JunctionKind.And);
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return new ComplexCriterion (ParseExpression (expression.Left), ParseExpression (expression.Right), ComplexCriterion.JunctionKind.Or);
        case ExpressionType.Equal:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.Equal);
        case ExpressionType.NotEqual:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.NotEqual);
        case ExpressionType.GreaterThanOrEqual:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.GreaterThanOrEqual);
        case ExpressionType.GreaterThan:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.GreaterThan);
        case ExpressionType.LessThanOrEqual:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.LessThanOrEqual);
        case ExpressionType.LessThan:
          return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), BinaryCondition.ConditionKind.LessThan);
        default:
          throw ParserUtility.CreateParserException ("and, or, or comparison expression", expression.NodeType, "binary expression in where condition",
              _whereClause.BoolExpression);
      }
    }

    private ICriterion ParseMethodCallExpression(MethodCallExpression expression)
    {
      if (expression.Method.Name == "StartsWith")
        return new BinaryCondition (ParseExpression (expression.Object), new Constant (((ConstantExpression)expression.Arguments[0]).Value + "%"), BinaryCondition.ConditionKind.Like);
      else if (expression.Method.Name == "EndsWith")
        return new BinaryCondition (ParseExpression (expression.Object), new Constant ("%" +  ((ConstantExpression) expression.Arguments[0]).Value), BinaryCondition.ConditionKind.Like);
      throw ParserUtility.CreateParserException("StartsWith, EndsWith",expression.NodeType,"method call expression in where condition",_whereClause.BoolExpression);
    }

    private ICriterion ParseUnaryExpression (UnaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (ParseExpression (expression.Operand));
        default:
          throw ParserUtility.CreateParserException ("not expression", expression.NodeType, "unary expression in where condition",
              _whereClause.BoolExpression);
      }
    }

    //BinaryExpression binaryExpression = _whereClause.BoolExpression.Body as BinaryExpression;
    //  Assertion.IsNotNull (binaryExpression);
    //  Assertion.IsTrue (binaryExpression.NodeType == ExpressionType.Equal);

    //  MemberExpression leftSide = binaryExpression.Left as MemberExpression;
    //  Assertion.IsNotNull (leftSide);
    //  ParameterExpression tableParameter = leftSide.Expression as ParameterExpression;
    //  Assertion.IsNotNull (tableParameter);

    //  FromClauseBase fromClause = ClauseFinder.FindFromClauseForExpression (_whereClause, tableParameter);
    //  Table table = DatabaseInfoUtility.GetTableForFromClause (_databaseInfo, fromClause);
    //  MemberInfo columnMember = leftSide.Member;
    //  Column leftColumn = DatabaseInfoUtility.GetColumn (_databaseInfo, table, columnMember);

    //  ConstantExpression rightSide = binaryExpression.Right as ConstantExpression;
    //  Assertion.IsNotNull (rightSide);
    //  Constant rightConstant = new Constant (rightSide.Value);
    //  return new BinaryCondition (leftColumn, rightConstant, BinaryCondition.ConditionKind.Equal);
  }
}