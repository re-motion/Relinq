using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest
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
    private FromLetWhereHelper _fromLetWhereHelper;


    [SetUp]
    public void SetUp ()
    {
      _querySource1 = ExpressionHelper.CreateQuerySource();
      _querySource2 = ExpressionHelper.CreateQuerySource();
      _expression = TestQueryGenerator.CreateMultiFromWhere_WhereExpression (_querySource1, _querySource2);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new WhereExpressionParser ((MethodCallExpression) _navigator.Arguments[0].Expression, _expression, false);
      _selectManyNavigator = new ExpressionTreeNavigator (_expression).Arguments[0].Arguments[0];
      _fromLetWhereHelper = new FromLetWhereHelper (_parser.FromLetWhereExpressions);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromExpressions);
      Assert.That (_fromLetWhereHelper.FromExpressions, Is.EqualTo (new object[]
          {
              _selectManyNavigator.Arguments[0].Expression,
              _selectManyNavigator.Arguments[1].Operand.Expression
          }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _fromLetWhereHelper.FromExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _fromLetWhereHelper.FromExpressions[1]);
      Assert.AreSame (_querySource1, ((ConstantExpression) _fromLetWhereHelper.FromExpressions[0]).Value);
      LambdaExpression fromExpression1 = (LambdaExpression) _fromLetWhereHelper.FromExpressions[1];
      Assert.AreSame (_querySource2, ExpressionHelper.ExecuteLambda (fromExpression1, (Student) null));
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromIdentifiers);
      Assert.That (_fromLetWhereHelper.FromIdentifiers,
          Is.EqualTo (new object[]
              {
                  _selectManyNavigator.Arguments[2].Operand.Parameters[0].Expression,
                  _selectManyNavigator.Arguments[2].Operand.Parameters[1].Expression
              }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _fromLetWhereHelper.FromIdentifiers[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _fromLetWhereHelper.FromIdentifiers[1]);
      Assert.AreEqual ("s1", _fromLetWhereHelper.FromIdentifiers[0].Name);
      Assert.AreEqual ("s2", _fromLetWhereHelper.FromIdentifiers[1].Name);
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
      Assert.IsNotNull (_fromLetWhereHelper.WhereExpressions);
      Assert.That (_fromLetWhereHelper.WhereExpressions, Is.EqualTo (new object[] { _navigator.Arguments[0].Arguments[1].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _fromLetWhereHelper.WhereExpressions[0]);
    }

    [Test]
    public void TakesWhereExpressionFromMany ()
    {
      Expression expression = TestQueryGenerator.CreateWhereFromWhere_WhereExpression (_querySource1, _querySource2);
      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (expression);
      WhereExpressionParser parser = new WhereExpressionParser ((MethodCallExpression) navigator.Arguments[0].Expression, expression, true);
      FromLetWhereHelper fromLetWhereHelper = new FromLetWhereHelper (parser.FromLetWhereExpressions);
      Assert.IsNotNull (fromLetWhereHelper.WhereExpressions);
      Assert.That (fromLetWhereHelper.WhereExpressions, Is.EqualTo (new object[]
          {
              navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[1].Operand.Expression,
              navigator.Arguments[0].Arguments[1].Operand.Expression
          }));
    }
  }
}