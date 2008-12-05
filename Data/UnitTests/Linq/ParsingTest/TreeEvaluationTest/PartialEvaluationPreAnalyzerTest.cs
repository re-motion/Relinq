// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.TreeEvaluationTest
{
  [TestFixture]
  public class PartialEvaluationPreAnalyzerTest
  {
    private TestablePartialEvaluationPreAnalyzer _analyzer;

    [SetUp]
    public void SetUp ()
    {
      _analyzer = new TestablePartialEvaluationPreAnalyzer ();
    }

    [Test]
    public void PrepareExpression ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      
      Assert.That (_analyzer.EvaluationData.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.UsedParameters[current1].ToArray(), Is.Empty);

      Assert.That (_analyzer.EvaluationData.DeclaredParameters.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.EvaluationData.SubQueries.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.SubQueries[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.CurrentExpressions, Is.EqualTo (new[] {current1}));
    }

    [Test]
    public void PrepareExpression_Twice ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      _analyzer.PrepareExpression (current1);

      Assert.That (_analyzer.EvaluationData.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.UsedParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.EvaluationData.DeclaredParameters.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.EvaluationData.SubQueries.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.SubQueries[current1].ToArray (), Is.Empty);

      Assert.That (_analyzer.CurrentExpressions, Is.EqualTo (new[] { current1, current1 }));
    }

    [Test]
    public void FinishExpression ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);
      _analyzer.FinishExpression();

      Assert.That (_analyzer.EvaluationData.UsedParameters.ContainsKey (current1));
      Assert.That (_analyzer.EvaluationData.DeclaredParameters.ContainsKey (current1));

      Assert.That (_analyzer.CurrentExpressions, Is.Empty);
    }

    [Test]
    public void VisitExpression_WithNullExpression ()
    {
      _analyzer.VisitExpression (null);

      Assert.That (_analyzer.EvaluationData.UsedParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.SubQueries, Is.Empty);
    }

    [Test]
    public void VisitExpression_AddsUsage ()
    {
      BinaryExpression binaryExpression = Expression.MakeBinary(ExpressionType.Equal, Expression.Constant (0), Expression.Constant (1));
      _analyzer.VisitExpression (binaryExpression);

      Assert.That (_analyzer.EvaluationData.UsedParameters.ContainsKey (binaryExpression));
      Assert.That (_analyzer.EvaluationData.DeclaredParameters.ContainsKey (binaryExpression));
    }

    [Test]
    public void VisitExpression_WithConstant_DoesntAddUsage ()
    {
      ConstantExpression constantExpression = Expression.Constant (0);
      _analyzer.VisitExpression (constantExpression);

      Assert.That (_analyzer.EvaluationData.UsedParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.SubQueries, Is.Empty);
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

      Assert.That (_analyzer.EvaluationData.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.UsedParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.SubQueries, Is.Empty);
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

      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current1].ToArray (), Is.Empty);
      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current2].ToArray (), Is.Empty);

      Assert.That (_analyzer.EvaluationData.UsedParameters[current1].ToArray (), Is.EquivalentTo (new[] { parameter }));
      Assert.That (_analyzer.EvaluationData.UsedParameters[current2].ToArray (), Is.EquivalentTo (new[] { parameter }));
    }

    [Test]
    public void VisitParameterExpression_Twice ()
    {
      Expression current1 = Expression.Constant (0);

      _analyzer.PrepareExpression (current1);

      ParameterExpression parameter = Expression.Parameter (typeof (string), "p");
      _analyzer.VisitParameterExpression (parameter);
      _analyzer.VisitParameterExpression (parameter);

      Assert.That (_analyzer.EvaluationData.UsedParameters[current1].ToArray(), Is.EqualTo (new[] { parameter }));
    }

    [Test]
    public void VisitLambdaExpression_NoCurrentExpression ()
    {
      ParameterExpression declaredParameter = Expression.Parameter (typeof (string), "p1");

      LambdaExpression lambda = Expression.Lambda (Expression.Constant(0), declaredParameter);
      _analyzer.VisitLambdaExpression (lambda);

      Assert.That (_analyzer.EvaluationData.DeclaredParameters, Is.Empty);
      Assert.That (_analyzer.EvaluationData.UsedParameters, Is.Empty);
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

      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current1].ToArray (), Is.EquivalentTo (new[] { declaredParameter }));
      Assert.That (_analyzer.EvaluationData.UsedParameters[current1].ToArray (), Is.EquivalentTo (new[] { usedParameter }));

      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current2].ToArray (), Is.EquivalentTo (new[] { declaredParameter }));
      Assert.That (_analyzer.EvaluationData.UsedParameters[current2].ToArray (), Is.EquivalentTo (new[] { usedParameter }));
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

      Assert.That (_analyzer.EvaluationData.DeclaredParameters[current1].ToArray(), Is.EqualTo (new[] { declaredParameter }));
    }

    [Test]
    public void VisitSubQueryExpression ()
    {
      Expression current1 = Expression.Constant (0);
      _analyzer.PrepareExpression (current1);

      SubQueryExpression subQueryExpression = new SubQueryExpression (ExpressionHelper.CreateQueryModel ());
      _analyzer.VisitSubQueryExpression (subQueryExpression);

      Assert.That (_analyzer.EvaluationData.SubQueries[current1].ToArray (), Is.EqualTo (new[] { subQueryExpression }));
    }
  }
}
