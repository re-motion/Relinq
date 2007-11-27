using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Parsing;
using Rubicon.Data.DomainObjects.Linq.UnitTests.Parsing;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest.WhereExpressionParserTest
{
  [TestFixture]
  public class SelectManyWhereExpressionParserTest
  {
    private IQueryable<Student> _querySource1;
    private IQueryable<Student> _querySource2;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private WhereExpressionParser _parser;
    private ExpressionTreeNavigator _selectManyNavigator;


    [SetUp]
    public void SetUp ()
    {
      _querySource1 = ExpressionHelper.CreateQuerySource();
      _querySource2 = ExpressionHelper.CreateQuerySource();
      _expression = TestQueryGenerator.CreateMultiFromWhere_WhereExpression (_querySource1, _querySource2);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new WhereExpressionParser ((MethodCallExpression) _navigator.Arguments[0].Expression, _expression, true);
      _selectManyNavigator = new ExpressionTreeNavigator (_expression).Arguments[0].Arguments[0];
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_parser.FromExpressions);
      Assert.That (_parser.FromExpressions, Is.EqualTo (new object[]
          {
              _selectManyNavigator.Arguments[0].Expression,
              _selectManyNavigator.Arguments[1].Operand.Expression
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
                  _selectManyNavigator.Arguments[2].Operand.Parameters[0].Expression,
                  _selectManyNavigator.Arguments[2].Operand.Parameters[1].Expression
              }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[1]);
      Assert.AreEqual ("s1", _parser.FromIdentifiers[0].Name);
      Assert.AreEqual ("s2", _parser.FromIdentifiers[1].Name);
    }


    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.That (_parser.ProjectionExpressions, Is.EqualTo (new object[] {_selectManyNavigator.Arguments[2].Operand.Expression}));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
    }

    [Test]
    public void ParsesBoolExpressions ()
    {
      Assert.IsNotNull (_parser.BoolExpressions);
      Assert.That (_parser.BoolExpressions, Is.EqualTo (new object[] {_navigator.Arguments[0].Arguments[1].Operand.Expression}));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.BoolExpressions[0]);
    }

    [Test]
    public void TakesWhereExpressionFromMany ()
    {
      Expression expression = TestQueryGenerator.CreateWhereFromWhere_WhereExpression (_querySource1, _querySource2);
      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (expression);
      WhereExpressionParser parser = new WhereExpressionParser ((MethodCallExpression) navigator.Arguments[0].Expression, expression, true);
      Assert.IsNotNull (parser.BoolExpressions);
      Assert.That (parser.BoolExpressions, Is.EqualTo (new object[]
          {
              navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[1].Operand.Expression,
              navigator.Arguments[0].Arguments[1].Operand.Expression
          }));
    }
  }
}