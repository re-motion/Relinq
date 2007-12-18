using System;
using System.Collections.Generic;
using NUnit.Framework;
using Rubicon.Data.Linq.Parsing.TreeEvaluation;
using System.Linq.Expressions;
using NUnit.Framework.SyntaxHelpers;

namespace Rubicon.Data.Linq.UnitTests.ParsingTest.TreeEvaluationTest
{
  [TestFixture]
  public class FilteredSubtreeFinderTest
  {
    [Test]
    public void Filter_Nothing()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate { return true; });
      HashSet<Expression> result = finder.GetFilteredSubtrees();
      Assert.That (GetArray (result), Is.EquivalentTo (new object[] {parameter, constant1, constant2, constant3, multiply, lambda, invoke, startNode}));
    }

    [Test]
    public void Filter_All ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate { return false; });
      HashSet<Expression> result = finder.GetFilteredSubtrees ();
      Assert.That (GetArray (result), Is.Empty);
    }

    [Test]
    public void Filter_Root ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate (Expression currentNode) { return currentNode != startNode; });
      HashSet<Expression> result = finder.GetFilteredSubtrees ();
      Assert.That (GetArray (result), Is.EquivalentTo (new object[] { parameter, constant1, constant2, constant3, multiply, lambda, invoke }));
    }

    [Test]
    public void Filter_LeafConstant ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate (Expression currentNode) { return currentNode != constant1; });
      HashSet<Expression> result = finder.GetFilteredSubtrees ();
      Assert.That (GetArray (result), Is.EquivalentTo (new object[] { parameter, constant2, constant3 }));
    }

    [Test]
    public void Filter_LeafParameter ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate (Expression currentNode) { return currentNode != parameter; });
      HashSet<Expression> result = finder.GetFilteredSubtrees ();
      Assert.That (GetArray (result), Is.EquivalentTo (new object[] { constant1, constant2, constant3 }));
    }

    [Test]
    public void Filter_Middle ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (int), "i");
      ConstantExpression constant1 = Expression.Constant (0);
      ConstantExpression constant2 = Expression.Constant (0);
      ConstantExpression constant3 = Expression.Constant (1);
      BinaryExpression multiply = Expression.Multiply (constant1, parameter);
      LambdaExpression lambda = Expression.Lambda (multiply, parameter);
      InvocationExpression invoke = Expression.Invoke (lambda, constant3);
      Expression startNode = Expression.Add (constant2, invoke);

      FilteredSubtreeFinder finder = new FilteredSubtreeFinder (startNode, delegate (Expression currentNode) { return currentNode != invoke; });
      HashSet<Expression> result = finder.GetFilteredSubtrees ();
      Assert.That (GetArray (result), Is.EquivalentTo (new object[] { parameter, constant1, constant2, constant3, multiply, lambda }));
    }

    private Expression[] GetArray (IEnumerable<Expression> expressions)
    {
      return new List<Expression> (expressions).ToArray();
    }
  }
}