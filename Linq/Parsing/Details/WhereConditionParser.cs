using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.FieldResolving;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.Details
{
  public class WhereConditionParser
  {
    private readonly bool _simplify;
    private readonly WhereClause _whereClause;
    private readonly IDatabaseInfo _databaseInfo;
    private readonly QueryExpression _queryExpression;
    private readonly JoinedTableContext _context;

    private List<FieldDescriptor> _fieldDescriptors;
    

    public WhereConditionParser (QueryExpression queryExpression, WhereClause whereClause, IDatabaseInfo databaseInfo, JoinedTableContext context, bool simplify)
    {
      ArgumentUtility.CheckNotNull ("whereClause", whereClause);
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("context", context);

      _simplify = simplify;
      _queryExpression = queryExpression;
      _whereClause = whereClause;
      _databaseInfo = databaseInfo;
      _context = context;
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
      _fieldDescriptors.Add (_queryExpression.ResolveField (_databaseInfo, _context, expression));
      return _queryExpression.ResolveField (_databaseInfo, _context, expression).GetMandatoryColumn();
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
          return CreateComplexCriterion(expression, ComplexCriterion.JunctionKind.And);
        case ExpressionType.Or:
        case ExpressionType.OrElse:
          return CreateComplexCriterion (expression, ComplexCriterion.JunctionKind.Or);
        case ExpressionType.Equal:
          return CreateBinaryCondition(expression, BinaryCondition.ConditionKind.Equal);
        case ExpressionType.NotEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.NotEqual);
        case ExpressionType.GreaterThanOrEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.GreaterThanOrEqual);
        case ExpressionType.GreaterThan:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.GreaterThan);
        case ExpressionType.LessThanOrEqual:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.LessThanOrEqual);
        case ExpressionType.LessThan:
          return CreateBinaryCondition (expression, BinaryCondition.ConditionKind.LessThan);
        default:
          throw ParserUtility.CreateParserException ("and, or, or comparison expression", expression.NodeType, "binary expression in where condition",
              _whereClause.BoolExpression);
      }
    }

    private ComplexCriterion CreateComplexCriterion (BinaryExpression expression, ComplexCriterion.JunctionKind kind)
    {
      return new ComplexCriterion (ParseExpression (expression.Left), ParseExpression (expression.Right), kind);
    }

    private BinaryCondition CreateBinaryCondition (BinaryExpression expression, BinaryCondition.ConditionKind kind)
    {
      return new BinaryCondition (ParseExpression (expression.Left), ParseExpression (expression.Right), kind);
    }

    private ICriterion ParseMethodCallExpression(MethodCallExpression expression)
    {
      if (expression.Method.Name == "StartsWith")
        return CreateLike (expression, ((ConstantExpression)expression.Arguments[0]).Value + "%");
      else if (expression.Method.Name == "EndsWith")
        return CreateLike (expression, "%" + ((ConstantExpression) expression.Arguments[0]).Value);

      throw ParserUtility.CreateParserException("StartsWith, EndsWith",expression.NodeType, "method call expression in where condition",_whereClause.BoolExpression);
    }

    private BinaryCondition CreateLike (MethodCallExpression expression, string pattern)
    {
      return new BinaryCondition (ParseExpression (expression.Object), new Constant (pattern), BinaryCondition.ConditionKind.Like);
    }

    private ICriterion ParseUnaryExpression (UnaryExpression expression)
    {
      switch (expression.NodeType)
      {
        case ExpressionType.Not:
          return new NotCriterion (ParseExpression (expression.Operand));
        case ExpressionType.Convert: // Convert is simply ignored ATM, change to more sophisticated logic when needed
          return ParseExpression (expression.Operand);
        default:
          throw ParserUtility.CreateParserException ("not or convert expression", expression.NodeType, "unary expression in where condition",
              _whereClause.BoolExpression);
      }
    }
  }
}