// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation;

namespace Remotion.Data.UnitTests.Linq.Parsing.ExpressionTreeVisitors.TreeEvaluation
{
  [TestFixture]
  public class PartialEvaluationPreAnalyzerIntegrationTest
  {
    private PartialEvaluationPreAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _analyzer = new PartialEvaluationPreAnalyzer();
    }

    [Test]
    public void NoParameters ()
    {
      Expression expression = Expression.MakeBinary (ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.Empty);
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.Empty);
    }

    [Test]
    public void ParameterExpression ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      Expression expression = p1;

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.EquivalentTo (new[] {p1}));
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.Empty);
    }

    [Test]
    public void BinaryExpression_UsingParameter ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (int), "p1");
      Expression expression = Expression.MakeBinary (ExpressionType.Equal, p1, Expression.Constant (0));

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.EquivalentTo (new[] {p1}));
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.Empty);
    }

    [Test]
    public void BinaryExpression_UsingMultipleParameters ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      Expression expression = Expression.MakeBinary (ExpressionType.Equal, p1, p2);

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.EquivalentTo (new[] {p1, p2}));
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.Empty);
    }

    [Test]
    public void LambdaExpression_DeclarationOnly ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      Expression expression = Expression.Lambda (Expression.Constant (0), p1, p2);

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.Empty);
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.EquivalentTo (new[] {p1, p2}));
    }

    [Test]
    public void LambdaExpression_DeclarationAndUsage ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      Expression expression = Expression.Lambda (p2, p1, p2);

      _analyzer.Analyze (expression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[expression].ToArray(), Is.EquivalentTo (new[] {p2}));
      Assert.That (evaluationData.DeclaredParameters[expression].ToArray(), Is.EquivalentTo (new[] {p1, p2}));
    }

    [Test]
    public void BinaryExpression_WithInnerLambdaExpression ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      ParameterExpression p3 = Expression.Parameter (typeof (string), "p3");
      ParameterExpression p4 = Expression.Parameter (typeof (string), "p4");
      Expression lambdaExpression = Expression.Lambda (p3, p1, p2);
      Expression lambdaCall = Expression.Invoke (lambdaExpression, Expression.Constant ("a"), Expression.Constant ("a"));
      Expression binaryExpression = Expression.MakeBinary (ExpressionType.Equal, lambdaCall, p4);

      _analyzer.Analyze (binaryExpression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[lambdaExpression].ToArray(), Is.EquivalentTo (new[] { p3 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression].ToArray (), Is.EquivalentTo (new[] { p1, p2 }));

      Assert.That (evaluationData.UsedParameters[binaryExpression].ToArray (), Is.EquivalentTo (new[] { p3, p4 }));
      Assert.That (evaluationData.DeclaredParameters[binaryExpression].ToArray (), Is.EquivalentTo (new[] { p1, p2 }));
    }

    [Test]
    public void BinaryExpression_WithMultipleInnerLambdaExpressions ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      ParameterExpression p3 = Expression.Parameter (typeof (string), "p3");
      ParameterExpression p5 = Expression.Parameter (typeof (string), "p5");
      ParameterExpression p6 = Expression.Parameter (typeof (string), "p6");
      ParameterExpression p7 = Expression.Parameter (typeof (string), "p7");

      Expression lambdaExpression1 = Expression.Lambda (p3, p1, p2);
      Expression lambdaExpression2 = Expression.Lambda (p5, p6, p7);
      Expression binaryExpression = Expression.MakeBinary (ExpressionType.Equal, lambdaExpression1, lambdaExpression2);

      _analyzer.Analyze (binaryExpression);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p3 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p1, p2 }));

      Assert.That (evaluationData.UsedParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p5 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p6, p7 }));

      Assert.That (evaluationData.UsedParameters[binaryExpression].ToArray (), Is.EquivalentTo (new[] { p3, p5 }));
      Assert.That (evaluationData.DeclaredParameters[binaryExpression].ToArray (), Is.EquivalentTo (new[] { p1, p2, p6, p7 }));
    }

    [Test]
    public void LambdaExpression_WithInnerLambdaExpression ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");
      ParameterExpression p2 = Expression.Parameter (typeof (string), "p2");
      ParameterExpression p3 = Expression.Parameter (typeof (string), "p3");
      ParameterExpression p6 = Expression.Parameter (typeof (string), "p6");
      ParameterExpression p7 = Expression.Parameter (typeof (string), "p7");

      Expression lambdaExpression1 = Expression.Lambda (p3, p1, p2);
      Expression lambdaExpression2 = Expression.Lambda (lambdaExpression1, p6, p7);

      _analyzer.Analyze (lambdaExpression2);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p3 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p1, p2 }));

      Assert.That (evaluationData.UsedParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p3 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p1, p2, p6, p7 }));
    }

    [Test]
    public void ParameterDeclaredMultipleTimes ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");

      Expression lambdaExpression1 = Expression.Lambda (Expression.Constant (0), p1);
      Expression lambdaExpression2 = Expression.Lambda (lambdaExpression1, p1);

      _analyzer.Analyze (lambdaExpression2);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.DeclaredParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p1 }));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p1 }));

      Assert.That (evaluationData.DeclaredParameters[lambdaExpression1].Count, Is.EqualTo (1));
      Assert.That (evaluationData.DeclaredParameters[lambdaExpression2].Count, Is.EqualTo (1));
    }

    [Test]
    public void ParameterUsedMultipleTimes ()
    {
      ParameterExpression p1 = Expression.Parameter (typeof (string), "p1");

      Expression lambdaExpression1 = Expression.Lambda (p1);
      Expression lambdaCall = Expression.Invoke (lambdaExpression1);
      Expression lambdaExpression2 = Expression.Lambda (Expression.MakeBinary (ExpressionType.Equal, lambdaCall, p1));

      _analyzer.Analyze (lambdaExpression2);
      PartialEvaluationData evaluationData = _analyzer.EvaluationData;

      Assert.That (evaluationData.UsedParameters[lambdaExpression1].ToArray (), Is.EquivalentTo (new[] { p1 }));
      Assert.That (evaluationData.UsedParameters[lambdaExpression2].ToArray (), Is.EquivalentTo (new[] { p1 }));

      Assert.That (evaluationData.UsedParameters[lambdaExpression1].Count, Is.EqualTo (1));
      Assert.That (evaluationData.UsedParameters[lambdaExpression2].Count, Is.EqualTo (1));
    }
  }
}
