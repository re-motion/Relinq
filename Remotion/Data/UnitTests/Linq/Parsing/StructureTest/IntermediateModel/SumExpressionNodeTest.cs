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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.IntermediateModel
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
  }
}