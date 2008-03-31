using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Data.Linq.Parsing.Structure;
using Rubicon.Data.Linq.UnitTests.TestQueryGenerators;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class SubQueryFindingVisitorTest
  {
    private SubQueryFindingVisitor _visitor;

    [SetUp]
    public void SetUp ()
    {
      _visitor = new SubQueryFindingVisitor ();
    }

    [Test]
    public void TreeWithNoSubquery()
    {
      Expression expression = Expression.Constant ("test");

      Expression newExpression = _visitor.ReplaceSubQuery (expression);
      Assert.That (newExpression, Is.SameAs (expression));
    }

    [Test]
    public void TreeWithSubquery ()
    {
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      Expression newExpression = _visitor.ReplaceSubQuery (surroundingExpression);

      Assert.That (newExpression, Is.Not.SameAs (surroundingExpression));
      Assert.That (newExpression, Is.InstanceOfType (typeof (LambdaExpression)));

      LambdaExpression newLambdaExpression = (LambdaExpression) newExpression;
      Assert.That (newLambdaExpression.Body, Is.InstanceOfType (typeof (SubQueryExpression)));

      SubQueryExpression newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (newSubQueryExpression.QueryModel.GetExpressionTree (), Is.SameAs (subQuery));
    }
  }
}