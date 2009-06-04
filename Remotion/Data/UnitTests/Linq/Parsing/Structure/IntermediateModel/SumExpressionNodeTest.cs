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
using System;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses.ResultModifications;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;
using Remotion.Development.UnitTesting;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class SumExpressionNodeTest : ExpressionNodeTestBase
  {
    [Test]
    public void SupportedMethod_WithoutSelector_OnDecimal ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<decimal>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDecimal ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<decimal?>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnDouble ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<double>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNDouble ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<double?>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnSingle ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<float>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNSingle ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<float?>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt32 ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<int>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt32 ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<int?>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnInt64 ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<long>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithoutSelector_OnNInt64 ()
    {
      MethodInfo method = GetMethod (q => ((IQueryable<long?>) q).Sum ());
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithDecimalSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => 0.0m));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithNDecimalSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => (decimal?) 0.0m));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithDoubleSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => 0.0));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithNDoubleSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => (double?) 0.0));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithSingleSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => 0.0f));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithNSingleSelector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => (float?) 0.0f));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithInt32Selector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => 0));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithNInt32Selector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => (int?) 0));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithInt64Selector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => 0L));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    public void SupportedMethod_WithNInt64Selector ()
    {
      MethodInfo method = GetGenericMethodDefinition (q => q.Sum (i => (long?) 0L));
      Assert.That (SumExpressionNode.SupportedMethods, List.Contains (method));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      var node = new SumExpressionNode ("x", SourceStub, null);
      node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression ());
    }

    [Test]
    public void GetResolvedSelector ()
    {
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SumExpressionNode ("x", SourceStub, predicate);

      var expectedResult = Expression.MakeBinary (ExpressionType.GreaterThan, SourceReference, Expression.Constant (5));

      var result = node.GetResolvedOptionalSelector();

      ExpressionTreeComparer.CheckAreEqualTrees (expectedResult, result);
    }

    [Test]
    public void GetResolvedSelector_Null ()
    {
      var sourceMock = MockRepository.GenerateMock<IExpressionNode> ();
      var node = new SumExpressionNode("x", sourceMock, null);
      var result = node.GetResolvedOptionalSelector ();
      Assert.That (result, Is.Null);
    }

    [Test]
    public void GetResolvedSelector_Cached ()
    {
      var sourceMock = new MockRepository ().StrictMock<IExpressionNode> ();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new SumExpressionNode ("x", sourceMock, predicate);
      var expectedResult = ExpressionHelper.CreateLambdaExpression ();

      sourceMock.Expect (mock => mock.Resolve (Arg<ParameterExpression>.Is.Anything, Arg<Expression>.Is.Anything)).Repeat.Once ().Return (expectedResult);

      sourceMock.Replay ();

      node.GetResolvedOptionalSelector ();
      node.GetResolvedOptionalSelector ();

      sourceMock.VerifyAllExpectations ();
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void CreateParameterForOutput ()
    {
      var node = new SumExpressionNode ("x", SourceStub, null);
      node.CreateParameterForOutput ();
    }

    [Test]
    public void CreateClause_WithoutSelector_PreviousClauseIsSelect ()
    {
      var node = new SumExpressionNode ("x", SourceStub, null);
      TestCreateClause_PreviousClauseIsSelect (node, typeof (SumResultModification));
    }

    [Test]
    public void CreateClause_WithoutSelector_PreviousClauseIsNoSelect ()
    {
      var node = new SumExpressionNode ("x", SourceStub, null);
      TestCreateClause_PreviousClauseIsNoSelect (node, typeof (SumResultModification));
    }

    [Test]
    public void CreateClause_WithSelector_AdjustsSelectClause ()
    {
      var selector = ExpressionHelper.CreateLambdaExpression<int, string> (i => i.ToString ());
      var node = new SumExpressionNode ("x", SourceStub, selector);

      var selectorOfPreviousClause = ExpressionHelper.CreateLambdaExpression<Student, int> (s => s.ID);
      var expectedNewSelector = ExpressionHelper.CreateLambdaExpression<Student, string> (s => s.ID.ToString ());

      TestCreateClause_WithOptionalSelector (node, selectorOfPreviousClause, expectedNewSelector);
    }
  }
}