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
using System;
using NUnit.Framework;
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Collections;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.TreeEvaluationTest
{
  [TestFixture]
  public class PartialTreeEvaluatorTest
  {
    [Test]
    public void EvaluateTopBinary ()
    {
      Expression treeRoot = Expression.Add (Expression.Constant (1), Expression.Constant (2));
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Expression expected = Expression.Constant (3);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopMemberAccess ()
    {
      Tuple<int, int> tuple = Tuple.NewTuple (1, 2);

      Expression treeRoot = Expression.MakeMemberAccess (Expression.Constant (tuple), typeof (Tuple<int, int>).GetProperty ("A"));
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Expression expected = Expression.Constant (1);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateTopLambda()
    {
      Expression treeRoot = Expression.Lambda (Expression.Constant (0), Expression.Parameter (typeof (string), "s"));
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator(treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Assert.AreSame (result, result);
    }

    [Test]
    public void EvaluateTopInvoke ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (string), "s");
      Expression treeRoot = Expression.Invoke (Expression.Lambda (parameter, parameter), Expression.Constant ("foo"));
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Expression expected = Expression.Constant ("foo");
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateBinaryInLambdaWithoutParameter ()
    {
      Expression treeRoot = Expression.Lambda (Expression.Add (Expression.Constant (5), Expression.Constant (1)),
          Expression.Parameter (typeof (string), "s"));
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Expression expected = Expression.Lambda (Expression.Constant (6), Expression.Parameter (typeof (string), "s"));
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateBinaryInLambdaWithParameter ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "p");
      Expression constant1 = Expression.Constant (3);
      Expression constant2 = Expression.Constant (4);
      Expression constant3 = Expression.Constant (3);
      Expression multiply1 = Expression.Multiply (parameter, constant1);
      Expression multiply2 = Expression.Multiply (constant2, constant3);
      Expression add = Expression.Add (multiply1, multiply2);
      Expression treeRoot = Expression.Lambda (typeof (System.Func<int, int>), add, parameter);
      
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (treeRoot);
      Expression result = evaluator.GetEvaluatedTree ();
      Expression expected = Expression.Lambda (Expression.Add (Expression.Multiply (parameter, constant1), Expression.Constant (12)), parameter);
      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    public void EvaluateLambdaWithParameterFromOutside ()
    {
      ParameterExpression outsideParameter = Expression.Parameter (typeof (int), "p");
      LambdaExpression lambdaExpression = Expression.Lambda (outsideParameter);
      
      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (lambdaExpression);
      Expression result = evaluator.GetEvaluatedTree ();
      Assert.AreSame (lambdaExpression, result);
    }

    [Test]
    public void EvaluateLambdaWithSubQuery  ()
    {
      SubQueryExpression subQuery = new SubQueryExpression(ExpressionHelper.CreateQueryModel());
      LambdaExpression lambdaExpression = Expression.Lambda (subQuery);

      PartialTreeEvaluator evaluator = new PartialTreeEvaluator (lambdaExpression);
      Expression result = evaluator.GetEvaluatedTree ();
      Assert.AreSame (lambdaExpression, result);
    }
  }
}
