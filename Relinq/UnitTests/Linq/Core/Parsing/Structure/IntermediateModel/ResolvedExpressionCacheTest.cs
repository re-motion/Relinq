// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel.TestDomain;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ResolvedExpressionCacheTest
  {
    private ResolvedExpressionCache<Expression> _cache;

    [SetUp]
    public void SetUp ()
    {
      var parseInfo = new MethodCallExpressionParseInfo("x", ExpressionNodeObjectMother.CreateMainSource(), ExpressionHelper.CreateMethodCallExpression());
      _cache = new ResolvedExpressionCache<Expression> (new TestMethodCallExpressionNode (parseInfo, null));
    }

    [Test]
    public void GetOrCreate_Initial ()
    {
      var fakeResult = ExpressionHelper.CreateExpression ();
      var result = _cache.GetOrCreate (r => fakeResult);

      Assert.That (result, Is.SameAs (fakeResult));
    }

    [Test]
    public void GetOrCreate_Twice ()
    {
      var fakeResult = ExpressionHelper.CreateExpression ();
      var result1 = _cache.GetOrCreate (r => fakeResult);

      var calledASecondTime = false;
      var result2 = _cache.GetOrCreate (r => { calledASecondTime = true; return null; } );

      Assert.That (result2, Is.SameAs (result1));
      Assert.That (calledASecondTime, Is.False);
    }
  }
}
