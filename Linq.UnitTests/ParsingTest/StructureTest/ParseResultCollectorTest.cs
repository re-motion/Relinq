using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class ParseResultCollectorTest
  {
    private ParseResultCollector _collector;
    private Expression _root;

    [SetUp]
    public void SetUp()
    {
      _root = ExpressionHelper.CreateExpression();
      _collector = new ParseResultCollector (_root);
    }

    [Test]
    public void ExpressionTreeRoot ()
    {
      Assert.AreSame (_root, _collector.ExpressionTreeRoot);
    }

    [Test]
    public void IsDistinct()
    {
      Assert.IsFalse (_collector.IsDistinct);
      _collector.SetDistinct();
      Assert.IsTrue (_collector.IsDistinct);
    }

    [Test]
    public void BodyExpressions()
    {
      Assert.That (_collector.BodyExpressions, Is.Empty);
      FromExpressionData expression1 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpressionData expression2 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      _collector.AddBodyExpression (expression1);
      _collector.AddBodyExpression (expression2);
      Assert.That (_collector.BodyExpressions, Is.EqualTo (new[] {expression1, expression2}));
    }

    [Test]
    public void ExtractMainFromExpression()
    {
      FromExpressionData expression1 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpressionData expression2 = new FromExpressionData (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      _collector.AddBodyExpression (expression1);
      _collector.AddBodyExpression (expression2);

      FromExpressionData mainFromExpressionData = _collector.ExtractMainFromExpression();
      Assert.AreSame (expression1, mainFromExpressionData);
      Assert.That (_collector.BodyExpressions, Is.EqualTo (new[] { expression2 }));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "There are no body expressions to be extracted.")]
    public void ExtractMainFromExpression_NoBodyExpressions ()
    {
      _collector.ExtractMainFromExpression ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "The first body expression is no FromExpressionData.")]
    public void ExtractMainFromExpression_NotFromExpression ()
    {
      WhereExpressionData expression1 = new WhereExpressionData (ExpressionHelper.CreateLambdaExpression());
      _collector.AddBodyExpression (expression1);
      _collector.ExtractMainFromExpression ();
    }

    [Test]
    public void ProjectionExpressions ()
    {
      Assert.That (_collector.ProjectionExpressions, Is.Empty);
      LambdaExpression expression1 = ExpressionHelper.CreateLambdaExpression();
      LambdaExpression expression2 = ExpressionHelper.CreateLambdaExpression ();
      _collector.AddProjectionExpression (expression1);
      _collector.AddProjectionExpression (null);
      _collector.AddProjectionExpression (expression2);
      Assert.That (_collector.ProjectionExpressions, Is.EqualTo (new[] { expression1, null, expression2 }));
    }
  }
}