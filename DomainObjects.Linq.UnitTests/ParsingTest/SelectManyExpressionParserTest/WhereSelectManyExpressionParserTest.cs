using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.SelectManyExpressionParserTest
{
  [TestFixture]
  public class WhereSelectManyExpressionParserTest
  {
    private IQueryable<Student> _querySource1;
    private IQueryable<Student> _querySource2;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private SelectManyExpressionParser _parser;

    [SetUp]
    public void SetUp ()
    {
      _querySource1 = ExpressionHelper.CreateQuerySource();
      _querySource2 = ExpressionHelper.CreateQuerySource();
      _expression = TestQueryGenerator.CreateReverseFromWhere_WhereExpression (_querySource1, _querySource2);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new SelectManyExpressionParser (_expression, _expression);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[0].Expression,
              _navigator.Arguments[1].Operand.Expression
          }));

      Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.FromExpressions[1]);

      Assert.AreSame (_querySource1, ((ConstantExpression) _parser.FromExpressions[0]).Value);

      LambdaExpression fromExpression1 = (LambdaExpression) _parser.FromExpressions[1];
      Assert.AreSame (_querySource2, ExpressionHelper.ExecuteLambda (fromExpression1, (Student) null));
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_parser.FromIdentifiers);
      Assert.That (_parser.FromIdentifiers,
          Is.EqualTo (new object[]
              {
                  _navigator.Arguments[0].Arguments[1].Operand.Parameters[0].Expression,
                  _navigator.Arguments[2].Operand.Parameters[1].Expression
              }));

      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[1]);

      Assert.AreEqual ("s1", _parser.FromIdentifiers[0].Name);
      Assert.AreEqual ("s2", _parser.FromIdentifiers[1].Name);
    }

    [Test]
    public void ParseWhereExpressions()
    {
      Assert.IsNotNull (_parser.WhereExpressions);
      Assert.That (_parser.WhereExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[0].Arguments[1].Operand.Expression
          }));
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.That (_parser.ProjectionExpressions, Is.EqualTo (new object[]
          {
              _navigator.Arguments[2].Operand.Expression
    }));
      
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
    }
  }
}