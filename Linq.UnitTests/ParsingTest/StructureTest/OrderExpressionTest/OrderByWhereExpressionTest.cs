using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Clauses;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.WhereExpressionParserTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.OrderExpressionTest
{
  [TestFixture]
  public class OrderByWhereExpressionTest
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
    private BodyHelper _bodyOrderByHelper;
    private ParseResultCollector _result;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = TestQueryGenerator.CreateOrderByQueryWithWhere_OrderByExpression (_querySource);
      _navigator = new ExpressionTreeNavigator (_expression);
      _result = new ParseResultCollector (_expression);
      new OrderByExpressionParser (true).Parse (_result, _expression);
      _bodyOrderByHelper = new BodyHelper (_result.BodyExpressions);
    }

    [Test]
    public void ParsesOrderingExpressions ()
    {
      Assert.IsNotNull (_bodyOrderByHelper.OrderingExpressions);
      Assert.AreEqual (1, _bodyOrderByHelper.OrderingExpressions.Count);
      AssertOrderExpressionsEqual (new OrderExpression (true, OrderDirection.Asc,
          (LambdaExpression) _navigator.Arguments[1].Operand.Expression), _bodyOrderByHelper.OrderingExpressions[0]);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromExpressions);

      Assert.That (_bodyOrderByHelper.FromExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Expression
          }));

      Assert.IsInstanceOfType (typeof (ConstantExpression), _navigator.Arguments[0].Arguments[0].Expression);
      Assert.AreSame (_querySource, ((ConstantExpression) _bodyOrderByHelper.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_bodyOrderByHelper.FromIdentifiers);
      Assert.That (_bodyOrderByHelper.FromIdentifiers, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[1].Operand.Parameters[0].Expression
          }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyOrderByHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", (_bodyOrderByHelper.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_bodyOrderByHelper.WhereExpressions);
      Assert.That (_bodyOrderByHelper.WhereExpressions, Is.EqualTo (new object[]
          {

              _navigator.Arguments[0].Arguments[1].Operand.Expression
          }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyOrderByHelper.WhereExpressions[0]);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_result.ProjectionExpressions);
      Assert.AreEqual (1, _result.ProjectionExpressions.Count);
      Assert.IsNull (_result.ProjectionExpressions[0]);
    }

  }
}