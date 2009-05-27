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
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure
{
  [TestFixture]
  public class ExpressionNodeTypeRegistryTest
  {
    [Test]
    public void Register ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      Assert.That (registry.Count, Is.EqualTo (0));

      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      Assert.That(registry.Count, Is.EqualTo (1));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException))]
    public void Register_ClosedGenericMethod_NotAllowed ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      var closedGenericMethod = SelectExpressionNode.SupportedMethods[0].MakeGenericMethod (typeof (int), typeof (int));
      registry.Register (new[] { closedGenericMethod }, typeof (SelectExpressionNode));
    }

    [Test]
    public void GetNodeType ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var type = registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_WithMultipleNodes ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var type1 = registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);
      var type2 = registry.GetNodeType (SumExpressionNode.SupportedMethods[0]);
      var type3 = registry.GetNodeType (SumExpressionNode.SupportedMethods[1]);

      Assert.That (type1, Is.SameAs (typeof (SelectExpressionNode)));
      Assert.That (type2, Is.SameAs (typeof (SumExpressionNode)));
      Assert.That (type3, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    public void GetNodeType_ClosedGenericMethod ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.Register (SelectExpressionNode.SupportedMethods, typeof (SelectExpressionNode));

      var closedGenericMethodCallExpression = 
          (MethodCallExpression) ExpressionHelper.MakeExpression<IQueryable<int>, IQueryable<int>> (q => q.Select (i => i + 1));
      var type = registry.GetNodeType (closedGenericMethodCallExpression.Method);

      Assert.That (type, Is.SameAs (typeof (SelectExpressionNode)));
    }

    [Test]
    public void GetNodeType_NonGenericMethod ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.Register (SumExpressionNode.SupportedMethods, typeof (SumExpressionNode));

      var nonGenericMethod = SumExpressionNode.SupportedMethods[0];
      Assert.That (nonGenericMethod.IsGenericMethod, Is.False);

      var type = registry.GetNodeType (nonGenericMethod);

      Assert.That (type, Is.SameAs (typeof (SumExpressionNode)));
    }

    [Test]
    [ExpectedException (typeof (KeyNotFoundException), 
        ExpectedMessage = "No corresponding expression node type was registered for method 'System.Linq.Queryable.Select'.")]
    public void GetNodeType_UnknownMethod ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.GetNodeType (SelectExpressionNode.SupportedMethods[0]);
    }

    [Test]
    public void Register_SameMethodTwice_OverridesPreviousNodeType ()
    {
      var registry = new ExpressionNodeTypeRegistry ();
      registry.Register (WhereExpressionNode.SupportedMethods, typeof (SelectExpressionNode));
      registry.Register (WhereExpressionNode.SupportedMethods, typeof (WhereExpressionNode));

      var type = registry.GetNodeType (WhereExpressionNode.SupportedMethods[0]);
      Assert.That (type, Is.SameAs (typeof (WhereExpressionNode)));
    }
  }
}