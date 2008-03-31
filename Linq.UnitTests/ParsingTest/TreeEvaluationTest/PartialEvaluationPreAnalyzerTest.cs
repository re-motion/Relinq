using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.TreeEvaluationTest
{
  [TestFixture]
  public class PartialEvaluationPreAnalyzerTest
  {
    private TestableParameterUsageAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _analyzer = new TestableParameterUsageAnalyzer ();
    }

    [Test]
    public void PrepareExpression ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      
      Assert.That (_analyzer.Usage.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.Usage.UsedParameters[current1].ToArray(), Is.Empty);

      Assert.That (_analyzer.Usage.DeclaredParameters.ContainsKey (current1));
      Assert.That (_analyzer.Usage.DeclaredParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.CurrentExpressions, Is.EqualTo (new[] {current1}));
    }

    [Test]
    public void PrepareExpression_Twice ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      _analyzer.PrepareExpression (current1);

      Assert.That (_analyzer.Usage.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.Usage.UsedParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.Usage.DeclaredParameters.ContainsKey (current1));
      Assert.That (_analyzer.Usage.DeclaredParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.CurrentExpressions, Is.EqualTo (new[] { current1, current1 }));
    }

    [Test]
    public void FinishExpression ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      _analyzer.FinishExpression();

      Assert.That (_analyzer.Usage.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.Usage.DeclaredParameters.ContainsKey (current1));

      Assert.That (_analyzer.CurrentExpressions, Is.Empty);
    }

    [Test]
    public void VisitExpression_WithNullExpression ()
    {
      _analyzer.VisitExpression (null);

      Assert.That (_analyzer.Usage.UsedParameters, Is.Empty);
      Assert.That (_analyzer.Usage.DeclaredParameters, Is.Empty);
    }

    [Test]
    public void VisitExpression_AddsUsage ()
    {
      BinaryExpression binaryExpression = Expression.MakeBinary(ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));
      _analyzer.VisitExpression (binaryExpression);

      Assert.That (_analyzer.Usage.UsedParameters.ContainsKey (binaryExpression));
      Assert.That (_analyzer.Usage.DeclaredParameters.ContainsKey (binaryExpression));
    }

    [Test]
    public void VisitExpression_WithConstant_DoesntAddUsage ()
    {
      ConstantExpression constantExpression = Expression.Constant (0);
      _analyzer.VisitExpression (constantExpression);

      Assert.That (_analyzer.Usage.UsedParameters, Is.Empty);
      Assert.That (_analyzer.Usage.DeclaredParameters, Is.Empty);
    }

    [Test]
    public void VisitExpression_InvokesConcreteVisitMethods ()
    {
      BinaryExpression binaryExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));
      _analyzer.VisitExpression (binaryExpression);

      Assert.That (_analyzer.VisitBinaryExpressionCalled);
    }

    [Test]
    public void VisitExpression_InvokesConcreteVisitMethods_WithExpressionOnStack ()
    {
      BinaryExpression binaryExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));
      _analyzer.VisitExpression (binaryExpression);

      Assert.That (_analyzer.StackTopInVisitBinary, Is.SameAs (binaryExpression));
    }

    [Test]
    public void VisitExpression_CleansUpStack ()
    {
      BinaryExpression binaryExpression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));
      _analyzer.VisitExpression (binaryExpression);

      Assert.That (_analyzer.CurrentExpressions, Is.Empty);
    }

    [Test]
    public void VisitParameterExpression_NoCurrentExpression ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (string), "p");
      _analyzer.VisitParameterExpression (parameter);

      Assert.That (_analyzer.Usage.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.Usage.UsedParameters, Is.Empty);
    }

    [Test]
    public void VisitParameterExpression_CurrentExpressions ()
    {
      Expression current1 = Expression.Constant (0);
      Expression current2 = Expression.Constant (0);

      _analyzer.PrepareExpression (current1);
      _analyzer.PrepareExpression (current2);

      ParameterExpression parameter = Expression.Parameter (typeof (string), "p");
      _analyzer.VisitParameterExpression (parameter);

      Assert.That (_analyzer.Usage.DeclaredParameters[current1].ToArray (), Is.Empty);
      Assert.That (_analyzer.Usage.DeclaredParameters[current2].ToArray (), Is.Empty);

      Assert.That (_analyzer.Usage.UsedParameters[current1].ToArray (), Is.EquivalentTo (new[] { parameter }));
      Assert.That (_analyzer.Usage.UsedParameters[current2].ToArray (), Is.EquivalentTo (new[] { parameter }));
    }

    [Test]
    public void VisitParameterExpression_Twice ()
    {
      Expression current1 = Expression.Constant (0);

      _analyzer.PrepareExpression (current1);

      ParameterExpression parameter = Expression.Parameter (typeof (string), "p");
      _analyzer.VisitParameterExpression (parameter);
      _analyzer.VisitParameterExpression (parameter);

      Assert.That (_analyzer.Usage.UsedParameters[current1].ToArray(), Is.EqualTo (new[] { parameter }));
    }

    [Test]
    public void VisitLambdaExpression_NoCurrentExpression ()
    {
      ParameterExpression declaredParameter = Expression.Parameter (typeof (string), "p1");

      LambdaExpression lambda = Expression.Lambda (Expression.Constant(0), declaredParameter);
      _analyzer.VisitLambdaExpression (lambda);

      Assert.That (_analyzer.Usage.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.Usage.UsedParameters, Is.Empty);
    }

    [Test]
    public void VisitLambdaExpression_WithCurrentExpressions ()
    {
      Expression current1 = Expression.Constant (0);
      Expression current2 = Expression.Constant (0);

      _analyzer.PrepareExpression (current1);
      _analyzer.PrepareExpression (current2);

      ParameterExpression declaredParameter = Expression.Parameter (typeof (string), "p1");
      ParameterExpression usedParameter = Expression.Parameter (typeof (string), "p2");

      LambdaExpression lambda = Expression.Lambda (usedParameter, declaredParameter);
      _analyzer.VisitLambdaExpression (lambda);

      Assert.That (_analyzer.Usage.DeclaredParameters[current1].ToArray (), Is.EquivalentTo (new[] { declaredParameter }));
      Assert.That (_analyzer.Usage.UsedParameters[current1].ToArray (), Is.EquivalentTo (new[] { usedParameter }));

      Assert.That (_analyzer.Usage.DeclaredParameters[current2].ToArray (), Is.EquivalentTo (new[] { declaredParameter }));
      Assert.That (_analyzer.Usage.UsedParameters[current2].ToArray (), Is.EquivalentTo (new[] { usedParameter }));
    }

    [Test]
    public void VisitLambdaExpression_Twice ()
    {
      Expression current1 = Expression.Constant (0);

      _analyzer.PrepareExpression (current1);

      ParameterExpression declaredParameter = Expression.Parameter (typeof (string), "p1");

      LambdaExpression lambda = Expression.Lambda (Expression.Constant (0), declaredParameter);
      _analyzer.VisitLambdaExpression (lambda);
      _analyzer.VisitLambdaExpression (lambda);

      Assert.That (_analyzer.Usage.DeclaredParameters[current1].ToArray(), Is.EqualTo (new[] { declaredParameter }));
    }
  }
}