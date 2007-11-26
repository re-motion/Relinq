using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.WhereExpressionParserTest
{
  [TestFixture]
  public class RecursiveWhereExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private WhereExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = TestQueryGenerator.CreateMultiWhereQuery_WhereExpression (_querySource);
      _navigator = new ExpressionTreeNavigator(_expression);
      _parser = new WhereExpressionParser (_expression, _expression, true);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[] { _navigator.Arguments[0].Arguments[0].Arguments[0].Expression }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _parser.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_parser.FromIdentifiers);
      Assert.That (_parser.FromIdentifiers,
          Is.EqualTo (new object[] {_navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Parameters[0].Expression }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_parser.BoolExpressions);
      Assert.That (_parser.BoolExpressions, Is.EqualTo (new object[] 
        {_navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Expression,
        _navigator.Arguments[0].Arguments[1].Operand.Expression,
        _navigator.Arguments[1].Operand.Expression
        }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.BoolExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.BoolExpressions[1]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.BoolExpressions[2]);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.AreEqual (1, _parser.ProjectionExpressions.Count);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.ProjectionExpressions[0].Body);
      Assert.AreSame (_navigator.Arguments[0].Arguments[0].Arguments[1].Operand.Parameters[0].Expression, _parser.ProjectionExpressions[0].Body);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.ProjectionExpressions[0].Body).Name);
    }

    [Test]
    public void ParsesProjectionExpressions_NotTopLevel ()
    {
      WhereExpressionParser parser = new WhereExpressionParser (_expression, _expression, false);
      Assert.IsNotNull (parser.ProjectionExpressions);
      Assert.AreEqual (0, parser.ProjectionExpressions.Count);
    }
  }
}