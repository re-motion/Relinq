using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing.Structure;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
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
      FromExpression expression1 = new FromExpression (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      FromExpression expression2 = new FromExpression (ExpressionHelper.CreateExpression (), ExpressionHelper.CreateParameterExpression ());
      _collector.AddBodyExpression (expression1);
      _collector.AddBodyExpression (expression2);
      Assert.That (_collector.BodyExpressions, Is.EqualTo (new[] {expression1, expression2}));
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