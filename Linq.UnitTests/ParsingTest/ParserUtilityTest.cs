using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;

namespace Remotion.Data.Linq.UnitTests.ParsingTest
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
    [ExpectedException (typeof (ParserException),
        ExpectedMessage = "Expected NewArrayExpression for source expression, found i (ParameterExpression).")]
    public void GetTypedExpression_InvalidType ()
    {
      Expression sourceExpression = ExpressionHelper.CreateParameterExpression();
      ParserUtility.GetTypedExpression<NewArrayExpression> (sourceExpression, "source expression", ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void CheckMethodCallExpression()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression(ExpressionHelper.CreateQuerySource());
      string result = ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "SelectMany", "Select", "Where");
      Assert.AreEqual ("Select", result);
      result = ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "Select");
      Assert.AreEqual ("Select", result);
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected one of 'SelectMany, Where', but found 'Select' at "
        + "position value(Remotion.Data.Linq.UnitTests.TestQueryable`1[Remotion.Data.Linq.UnitTests."
        + "Student]).Select(s => s) in tree new [] {}.")]
    public void CheckMethodCallExpression_InvalidName ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckMethodCallExpression (selectExpression, ExpressionHelper.CreateExpression (), "SelectMany", "Where");
    }

    [Test]
    public void CheckNumberOfArguments_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckNumberOfArguments (selectExpression, "Select", 2, ExpressionHelper.CreateExpression ());
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected at least 1 argument for Select method call, found 2 arguments.")]
    public void CheckNumberOfArguments_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckNumberOfArguments (selectExpression, "Select", 1, ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void CheckParameterType_Succeed ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckParameterType<ConstantExpression> (selectExpression, "Select", 0, ExpressionHelper.CreateExpression ());
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected ParameterExpression for argument 0 of Select method call, found " 
        + "ConstantExpression (value(Remotion.Data.Linq.UnitTests.TestQueryable`1[Remotion.Data.Linq.UnitTests.Student])).")]
    public void CheckParameterType_Fail ()
    {
      MethodCallExpression selectExpression = SelectTestQueryGenerator.CreateSimpleQuery_SelectExpression (ExpressionHelper.CreateQuerySource ());
      ParserUtility.CheckParameterType<ParameterExpression> (selectExpression, "Select", 0, ExpressionHelper.CreateExpression ());
    }
  }
}