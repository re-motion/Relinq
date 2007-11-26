using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.SelectExpressionParserTest
{
  [TestFixture]
  public class WhereSelectExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private SelectExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();

      _expression = TestQueryGenerator.CreateSelectWhereQuery_SelectExpression (_querySource);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new SelectExpressionParser (_expression, _expression);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[] { GetWhereExpression().Arguments[0].Expression }));

      Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _parser.FromExpressions[0]).Value);
    }

    private ExpressionTreeNavigator GetWhereExpression ()
    {
      return new ExpressionTreeNavigator(_expression.Arguments[0]);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_parser.FromIdentifiers);
      Assert.That (_parser.FromIdentifiers,
                   Is.EqualTo (new object[] { GetWhereExpression ().Arguments[1].Operand.Parameters[0].Expression }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesWhereExpressions ()
    {
      Assert.IsNotNull (_parser.WhereExpressions);
      Assert.That (_parser.WhereExpressions, Is.EqualTo(new object[] {GetWhereExpression().Arguments[1].Operand.Expression}));
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