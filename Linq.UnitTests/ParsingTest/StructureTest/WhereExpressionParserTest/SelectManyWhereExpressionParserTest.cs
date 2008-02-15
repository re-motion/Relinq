using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.WhereExpressionParserTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest.WhereExpressionParserTest
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
    private BodyHelper _bodyWhereHelper;


    [SetUp]
    public void SetUp ()
    {
      _querySource1 = ExpressionHelper.CreateQuerySource();
      _querySource2 = ExpressionHelper.CreateQuerySource();
      _expression = TestQueryGenerator.CreateMultiFromWhere_WhereExpression (_querySource1, _querySource2);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new WhereExpressionParser ((MethodCallExpression) _navigator.Arguments[0].Expression, _expression, false);
      _selectManyNavigator = new ExpressionTreeNavigator (_expression).Arguments[0].Arguments[0];
      _bodyWhereHelper = new BodyHelper (_parser.FromLetWhereExpressions);
    }

    [Test]
    public void ParsesFromExpressions ()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromExpressions);
      Assert.That (_bodyWhereHelper.FromExpressions, Is.EqualTo (new object[]
          {
              _selectManyNavigator.Arguments[0].Expression,
              _selectManyNavigator.Arguments[1].Operand.Expression
          }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _bodyWhereHelper.FromExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.FromExpressions[1]);
      Assert.AreSame (_querySource1, ((ConstantExpression) _bodyWhereHelper.FromExpressions[0]).Value);
      LambdaExpression fromExpression1 = (LambdaExpression) _bodyWhereHelper.FromExpressions[1];
      Assert.AreSame (_querySource2, ExpressionHelper.ExecuteLambda (fromExpression1, (Student) null));
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_bodyWhereHelper.FromIdentifiers);
      Assert.That (_bodyWhereHelper.FromIdentifiers,
          Is.EqualTo (new object[]
              {
                  _selectManyNavigator.Arguments[2].Operand.Parameters[0].Expression,
                  _selectManyNavigator.Arguments[2].Operand.Parameters[1].Expression
              }));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyWhereHelper.FromIdentifiers[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _bodyWhereHelper.FromIdentifiers[1]);
      Assert.AreEqual ("s1", _bodyWhereHelper.FromIdentifiers[0].Name);
      Assert.AreEqual ("s2", _bodyWhereHelper.FromIdentifiers[1].Name);
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
      Assert.IsNotNull (_bodyWhereHelper.WhereExpressions);
      Assert.That (_bodyWhereHelper.WhereExpressions, Is.EqualTo (new object[] { _navigator.Arguments[0].Arguments[1].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _bodyWhereHelper.WhereExpressions[0]);
    }

    [Test]
    public void TakesWhereExpressionFromMany ()
    {
      Expression expression = TestQueryGenerator.CreateWhereFromWhere_WhereExpression (_querySource1, _querySource2);
      ExpressionTreeNavigator navigator = new ExpressionTreeNavigator (expression);
      WhereExpressionParser parser = new WhereExpressionParser ((MethodCallExpression) navigator.Arguments[0].Expression, expression, true);
      BodyHelper bodyWhereHelper = new BodyHelper (parser.FromLetWhereExpressions);
      Assert.IsNotNull (bodyWhereHelper.WhereExpressions);
      Assert.That (bodyWhereHelper.WhereExpressions, Is.EqualTo (new object[]
          {
              navigator.Arguments[0].Arguments[0].Arguments[0].Arguments[1].Operand.Expression,
              navigator.Arguments[0].Arguments[1].Operand.Expression
          }));
    }
  }
}