using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.SelectExpressionParserTest
{
  [TestFixture]
  public class WhereSelectExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private SelectExpressionParser _parser;
    private FromLetWhereHelper _fromLetWhereHelper;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();

      _expression = TestQueryGenerator.CreateSelectWhereQuery_SelectExpression (_querySource);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new SelectExpressionParser (_expression, _expression);
      _fromLetWhereHelper = new FromLetWhereHelper (_parser.FromLetWhereExpressions);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromExpressions);
      Assert.That (_fromLetWhereHelper.FromExpressions, Is.EqualTo (new object[] { GetWhereExpression ().Arguments[0].Expression }));

      Assert.IsInstanceOfType (typeof (ConstantExpression), _fromLetWhereHelper.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _fromLetWhereHelper.FromExpressions[0]).Value);
    }

    private ExpressionTreeNavigator GetWhereExpression ()
    {
      return new ExpressionTreeNavigator(_expression.Arguments[0]);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromIdentifiers);
      Assert.That (_fromLetWhereHelper.FromIdentifiers,
                   Is.EqualTo (new object[] { GetWhereExpression ().Arguments[1].Operand.Parameters[0].Expression }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _fromLetWhereHelper.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _fromLetWhereHelper.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesWhereExpressions ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.WhereExpressions);
      Assert.That (_fromLetWhereHelper.WhereExpressions, Is.EqualTo (new object[] { GetWhereExpression ().Arguments[1].Operand.Expression }));
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.That (_parser.ProjectionExpressions, Is.EqualTo (new object[] { _navigator.Arguments[1].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
    }



  }
}