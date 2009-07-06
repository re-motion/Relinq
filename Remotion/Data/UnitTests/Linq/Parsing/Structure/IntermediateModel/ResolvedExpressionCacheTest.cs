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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class ResolvedExpressionCacheTest
  {
    private ResolvedExpressionCache _cache;

    [SetUp]
    public void SetUp ()
    {
      _cache = new ResolvedExpressionCache (ExpressionNodeObjectMother.CreateMainSource ());
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