using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest;
using Rubicon.Data.Linq.Parsing;
using Rubicon.Data.Linq.UnitTests.ParsingTest.WhereExpressionParserTest;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.SelectManyExpressionParserTest
{
  [TestFixture]
  public class SimpleSelectManyExpressionParserTest
  {
    private IQueryable<Student> _querySource1;
    private IQueryable<Student> _querySource2;
    private MethodCallExpression _expression;
    private ExpressionTreeNavigator _navigator;
    private SelectManyExpressionParser _parser;
    private FromLetWhereHelper _fromLetWhereHelper;

    [SetUp]
    public void SetUp()
    {
      _querySource1 = ExpressionHelper.CreateQuerySource();
      _querySource2 = ExpressionHelper.CreateQuerySource ();
      _expression = TestQueryGenerator.CreateMultiFromQuery_SelectManyExpression (_querySource1, _querySource2);
      _navigator = new ExpressionTreeNavigator (_expression);
      _parser = new SelectManyExpressionParser (_expression, _expression);
      _fromLetWhereHelper = new FromLetWhereHelper (_parser.FromLetWhereExpressions);
    }

    
    [Test]
    public void ParsesFromExpressions()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromExpressions);
      Assert.That (_fromLetWhereHelper.FromExpressions, Is.EqualTo (new object[] { _expression.Arguments[0], _navigator.Arguments[1].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (ConstantExpression), _fromLetWhereHelper.FromExpressions[0]);
      Assert.IsInstanceOfType (typeof (LambdaExpression), _fromLetWhereHelper.FromExpressions[1]);
      Assert.AreSame (_querySource1, ((ConstantExpression) _fromLetWhereHelper.FromExpressions[0]).Value);
      LambdaExpression fromExpression1 = (LambdaExpression) _fromLetWhereHelper.FromExpressions[1];
      Assert.AreSame (_querySource2, ExpressionHelper.ExecuteLambda (fromExpression1, (Student)null));

      //Assert.IsNotNull (_parser.FromExpressions);
      //Assert.That (_parser.FromExpressions, Is.EqualTo (new object[] { _expression.Arguments[0],_navigator.Arguments[1].Operand.Expression }));
      //Assert.IsInstanceOfType (typeof (ConstantExpression), _parser.FromExpressions[0]);
      //Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.FromExpressions[1]);
      //Assert.AreSame (_querySource1, ((ConstantExpression)_parser.FromExpressions[0]).Value);
      //LambdaExpression fromExpression1 = (LambdaExpression) _parser.FromExpressions[1];
      //Assert.AreSame (_querySource2, ExpressionHelper.ExecuteLambda (fromExpression1, (Student)null));
    }

    [Test]
    public void ParsesFromIdentifiers ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.FromIdentifiers);
      Assert.That (_fromLetWhereHelper.FromIdentifiers,
                   Is.EqualTo (new object[] { _navigator.Arguments[2].Operand.Parameters[0].Expression,
                   _navigator.Arguments[2].Operand.Parameters[1].Expression}));
      Assert.IsInstanceOfType (typeof (ParameterExpression), _fromLetWhereHelper.FromIdentifiers[0]);
      Assert.IsInstanceOfType (typeof (ParameterExpression), _fromLetWhereHelper.FromIdentifiers[1]);
      Assert.AreEqual ("s1", _fromLetWhereHelper.FromIdentifiers[0].Name);
      Assert.AreEqual ("s2", _fromLetWhereHelper.FromIdentifiers[1].Name);

      //Assert.IsNotNull (_parser.FromIdentifiers);
      //Assert.That (_parser.FromIdentifiers,
      //             Is.EqualTo (new object[] { _navigator.Arguments[2].Operand.Parameters[0].Expression,
      //             _navigator.Arguments[2].Operand.Parameters[1].Expression}));
      //Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[0]);
      //Assert.IsInstanceOfType (typeof (ParameterExpression), _parser.FromIdentifiers[1]);
      //Assert.AreEqual ("s1", _parser.FromIdentifiers[0].Name);
      //Assert.AreEqual ("s2", _parser.FromIdentifiers[1].Name);
    }

    [Test]
    public void ParseWhereExpressions ()
    {
      Assert.IsNotNull (_fromLetWhereHelper.WhereExpressions);
      Assert.That (_fromLetWhereHelper.WhereExpressions, Is.Empty);

      //Assert.IsNotNull (_parser.WhereExpressions);
      //Assert.That (_parser.WhereExpressions, Is.Empty );
    }

    [Test]
    public void ParsesProjectionExpressions ()
    {
      Assert.IsNotNull (_parser.ProjectionExpressions);
      Assert.That (_parser.ProjectionExpressions, Is.EqualTo (new object[] { _navigator.Arguments[2].Operand.Expression }));
      Assert.IsInstanceOfType (typeof (LambdaExpression), _parser.ProjectionExpressions[0]);
    }
  }
}