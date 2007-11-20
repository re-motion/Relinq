using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.DomainObjects.Linq.Parsing;

namespace Rubicon.Data.DomainObjects.Linq.UnitTests.ParsingTest
{
  [TestFixture]
  public class ParserUtilityTest
  {
    [Test]
    public void GetTypedExpression()
    {
      Expression sourceExpression = ExpressionHelper.CreateNewIntArrayExpression();
      NewArrayExpression expression = ParserUtility.GetTypedExpression<NewArrayExpression> (sourceExpression,
          "...", ExpressionHelper.CreateExpression());
      Assert.AreSame (sourceExpression, expression);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException),
        ExpectedMessage = "Expected NewArrayExpression for source expression, found ParameterExpression (i).")]
    public void GetTypedExpression_InvalidType ()
    {
      Expression sourceExpression = ExpressionHelper.CreateParameterExpression();
      ParserUtility.GetTypedExpression<NewArrayExpression> (sourceExpression, "source expression", ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void CheckMethodCallExpression()
    {
      MethodCallExpression selectExpression = TestQueryGenerator.CreateSimpleQuerySelectExpression(ExpressionHelper.CreateQuerySource());
      string result = ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "SelectMany", "Select", "Where");
      Assert.AreEqual ("Select", result);
      result = ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "Select");
      Assert.AreEqual ("Select", result);
    }

    [Test]
    [ExpectedException (typeof (QueryParserException), ExpectedMessage = "Expected one of 'SelectMany, Where', but found 'Select' at "
        + "position value(Rubicon.Data.DomainObjects.Linq.QueryProviderImplementation.StandardQueryable`1[Rubicon.Data.DomainObjects.Linq.UnitTests."
        + "Parsing.Student]).Select(s => s) in tree new [] {}.")]
    public void CheckMethodCallExpression_InvalidName ()
    {
      MethodCallExpression selectExpression = TestQueryGenerator.CreateSimpleQuerySelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "SelectMany", "Where");
    }
  }
}