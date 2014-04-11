// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NUnit.Framework;
using Remotion.Linq.Clauses.ResultOperators;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class CountExpressionNodeTest : ExpressionNodeTestBase
  {
    private CountExpressionNode _node;

    public override void SetUp ()
    {
      base.SetUp ();
      _node = new CountExpressionNode (CreateParseInfo (), null);
    }

    [Test]
    public void SupportedMethods_WithoutPredicate ()
    {
      AssertSupportedMethod_Generic (CountExpressionNode.SupportedMethods, q => q.Count (), e => e.Count ());
    }

    [Test]
    public void SupportedMethods_WithoutPredicate_FromCollections ()
    {
      Assert.That (CountExpressionNode.SupportedMethods, Has.Member (typeof (List<>).GetProperty ("Count").GetGetMethod()));
      Assert.That (CountExpressionNode.SupportedMethods, Has.Member (typeof (ArrayList).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, Has.Member (typeof (ICollection<>).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, Has.Member (typeof (ICollection).GetProperty ("Count").GetGetMethod ()));
      Assert.That (CountExpressionNode.SupportedMethods, Has.Member (typeof (Array).GetProperty ("Length").GetGetMethod ()));
    }

    [Test]
    public void SupportedMethod_WithPredicate ()
    {
      AssertSupportedMethod_Generic (CountExpressionNode.SupportedMethods, q => q.Count (o => o == null), e => e.Count (o => o == null));
    }

    [Test]
    public void Initialization_WithPredicate ()
    {
      var parseInfo = CreateParseInfo();
      var predicate = ExpressionHelper.CreateLambdaExpression<int, bool> (i => i > 5);
      var node = new CountExpressionNode (parseInfo, predicate);

      Assert.That (node.Source, Is.InstanceOf (typeof (WhereExpressionNode)));
      Assert.That (((WhereExpressionNode) node.Source).Predicate, Is.SameAs (predicate));
      Assert.That (((WhereExpressionNode) node.Source).Source, Is.SameAs (SourceNode));
    }

    [Test]
    public void Initialization_WithoutPredicate ()
    {
      var parseInfo = CreateParseInfo ();
      var node = new CountExpressionNode (parseInfo, null);

      Assert.That (node.Source, Is.SameAs (SourceNode));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Resolve_ThrowsInvalidOperationException ()
    {
      _node.Resolve (ExpressionHelper.CreateParameterExpression (), ExpressionHelper.CreateExpression (), ClauseGenerationContext);
    }

    [Test]
    public void Apply ()
    {
      TestApply (_node, typeof (CountResultOperator));
    }
  }
}
