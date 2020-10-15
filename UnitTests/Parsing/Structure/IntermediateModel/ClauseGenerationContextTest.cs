// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Collections.Generic;
using NUnit.Framework;
using Remotion.Linq.Clauses;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.Parsing.Structure.NodeTypeProviders;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ClauseGenerationContextTest
  {
    private ClauseGenerationContext _context;
    private MainSourceExpressionNode _node;
    private MainFromClause _clause;

    [SetUp]
    public void SetUp ()
    {
      _node = ExpressionNodeObjectMother.CreateMainSource ();
      _context = new ClauseGenerationContext (new MethodInfoBasedNodeTypeRegistry ());
      _clause = ExpressionHelper.CreateMainFromClause_Int ();
    }

    [Test]
    public void AddContextInfo ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.GetContextInfo (_node), Is.Not.Null);
    }

    [Test]
    public void AddContextInfo_IncreasesCount ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.Count, Is.EqualTo (1));
    }

    [Test]
    public void AddContextInfoTwice ()
    {
      _context.AddContextInfo (_node, _clause);
      Assert.That (
          () => _context.AddContextInfo (_node, _clause),
          Throws.InvalidOperationException
              .With.Message.EqualTo (
                  "Node already has associated context info."));
    }

    [Test]
    public void GetContextInfo ()
    {
      _context.AddContextInfo (_node, _clause);

      Assert.That (_context.GetContextInfo (_node), Is.SameAs (_clause));
    }

    [Test]
    public void GetContextInfo_ThrowsException ()
    {
      Assert.That (
          () => _context.GetContextInfo (_node),
          Throws.InstanceOf<KeyNotFoundException>()
              .With.Message.EqualTo ("Node has no associated context info."));
    }
    
  }
}
