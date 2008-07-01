using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.UnitTests.TestQueryGenerators;
using System.Collections.Generic;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest
{
  [TestFixture]
  public class SubQueryFindingVisitorTest
  {
    private SubQueryFindingVisitor _visitor;
    private List<QueryModel> _subQueryRegistry;

    [SetUp]
    public void SetUp ()
    {
      _subQueryRegistry = new List<QueryModel>();
      _visitor = new SubQueryFindingVisitor (_subQueryRegistry);
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

    [Test]
    public void SubqueryIsRegistered ()
    {
      Assert.That (_subQueryRegistry, Is.Empty);
      
      Expression subQuery = SelectTestQueryGenerator.CreateSimpleQuery (ExpressionHelper.CreateQuerySource ()).Expression;
      Expression surroundingExpression = Expression.Lambda (subQuery);

      LambdaExpression newLambdaExpression = (LambdaExpression) _visitor.ReplaceSubQuery (surroundingExpression);
      SubQueryExpression newSubQueryExpression = (SubQueryExpression) newLambdaExpression.Body;
      Assert.That (_subQueryRegistry, Is.EquivalentTo (new[] { newSubQueryExpression.QueryModel }));
    }
  }
}