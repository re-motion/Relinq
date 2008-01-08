using System;
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.OrderExpressionTest
{
  [TestFixture]
  public class SimpleOrderByExpressionTest
  {
    public static void AssertOrderExpressionsEqual (OrderExpression one, OrderExpression two)
    {
      Assert.AreEqual (one.Expression, two.Expression);
      Assert.AreEqual (one.OrderDirection, two.OrderDirection);
      Assert.AreEqual (one.FirstOrderBy, two.FirstOrderBy);
    }

    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private OrderByExpressionParser _parser;
    private BodyHelper _bodyOrderByHelper;

    [SetUp]
    public void SetUp()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = TestQueryGenerator.CreateOrderByQueryWithOrderByAndThenBy_OrderByExpression (_querySource);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new OrderByExpressionParser (_expression, _expression,true);
      _bodyOrderByHelper = new BodyHelper (_parser.BodyExpressions);
    }

    [Test]
    public void ParsesOrderingExpressions()
    {
      Assert.IsNotNull (_bodyOrderByHelper.OrderingExpressions);
      Assert.AreEqual(3, _bodyOrderByHelper.OrderingExpressions.Count);
      AssertOrderExpressionsEqual (new OrderExpression (true, OrderDirection.Asc,
          (UnaryExpression) _navigator.Arguments[0].Arguments[0].Arguments[1].Expression), _bodyOrderByHelper.OrderingExpressions[0]);
      AssertOrderExpressionsEqual (new OrderExpression (false, OrderDirection.Desc,
          (UnaryExpression) _navigator.Arguments[0].Arguments[1].Expression), _bodyOrderByHelper.OrderingExpressions[1]);
      AssertOrderExpressionsEqual (new OrderExpression (false, OrderDirection.Asc,
          (UnaryExpression) _navigator.Arguments[1].Expression), _bodyOrderByHelper.OrderingExpressions[2]);
    }
        
    [Test]
    public void ParsesFromExpressions()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromExpressions);
      Assert.That (_bodyOrderByHelper.FromExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Arguments[0].Expression
    }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _navigator.Arguments[0].Arguments[0].Arguments[0].Expression);
      Assert.AreSame (_querySource, ((ConstantExpression) _bodyOrderByHelper.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromIdentifiers);
      Assert.That (_bodyOrderByHelper.FromIdentifiers, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Parameters[0].Expression
     }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyOrderByHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", (_bodyOrderByHelper.FromIdentifiers[0]).Name);
    }
  }
}