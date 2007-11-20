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
  public class SimpleWhereExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private WhereExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = TestQueryGenerator.CreateSimpleWhereQueryWhereExpression (_querySource);
      _parser = new WhereExpressionParser (_expression, _expression);
    }

   
    [Test]
    public void ParsesFromExpressions()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[] { _expression.Arguments[0] }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      Assert.AreSame (_querySource, ((ConstantExpression) _parser.FromExpressions[0]).Value);
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_parser.FromIdentifiers);
      Assert.That (_parser.FromIdentifiers,
                   Is.EqualTo (new object[] { ((LambdaExpression) ((UnaryExpression) _expression.Arguments[1]).Operand).Parameters[0] }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.FromIdentifiers[0]).Name);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_parser.BoolExpressions);
      Assert.That (_parser.BoolExpressions, Is.EqualTo (new object[] { ((UnaryExpression) _expression.Arguments[1]).Operand }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.BoolExpressions[0]);
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.AreEqual (1, _parser.ProjectionExpressions.Count);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.ProjectionExpressions[0].Body);
      Assert.AreSame (((LambdaExpression) ((UnaryExpression) _expression.Arguments[1]).Operand).Parameters[0], _parser.ProjectionExpressions[0].Body);
      Assert.AreEqual ("s", ((ParameterExpression) _parser.ProjectionExpressions[0].Body).Name);
    }
  }
}