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
    private SelectExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();

      _expression = TestQueryGenerator.CreateSelectWhereQuery_SelectExpression (_querySource);
      _parser = new SelectExpressionParser (_expression, _expression);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[] { GetWhereExpression().Arguments[0] }));

      Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _parser.FromExpressions[0]).Value);
    }

    private MethodCallExpression GetWhereExpression ()
    {
      return ((MethodCallExpression)_expression.Arguments[0]);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_parser.FromIdentifiers);
      Assert.That (_parser.FromIdentifiers,
                   Is.EqualTo (new object[] { ((LambdaExpression) ((UnaryExpression) GetWhereExpression ().Arguments[1]).Operand).Parameters[0] }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesWhereExpressions ()
    {
      Assert.IsNotNull (_parser.WhereExpressions);
      Assert.That (_parser.WhereExpressions, Is.EqualTo(new object[] {((UnaryExpression)GetWhereExpression().Arguments[1]).Operand}));
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.That (_parser.ProjectionExpressions, Is.EqualTo (new object[] { ((UnaryExpression) _expression.Arguments[1]).Operand }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
    }



  }
}