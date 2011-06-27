// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using NUnit.Framework;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.MemberBindings;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.MemberBindings
{
  [TestFixture]
  public class MethodInfoBindingTest : MemberBindingTestBase
  {
    [Test]
    public void MatchesReadAccess_True ()
    {
      var binding = new MethodInfoBinding (Method, AssociatedExpression);
      Assert.That (binding.MatchesReadAccess (Method), Is.True);
    }

    [Test]
    public void MatchesReadAccess_True_WithProperty ()
    {
      var binding = new MethodInfoBinding (Method, AssociatedExpression);
      Assert.That (binding.MatchesReadAccess (Property), Is.True);
    }

    [Test]
    public void MatchesReadAccess_False_WithOtherProperty ()
    {
      var binding = new MethodInfoBinding (Method, AssociatedExpression);
      Assert.That (binding.MatchesReadAccess (OtherProperty), Is.False);
    }

    [Test]
    public void MatchesReadAccess_False_WithReadOnlyProperty ()
    {
      var binding = new MethodInfoBinding (Method, AssociatedExpression);
      Assert.That (binding.MatchesReadAccess (WriteOnlyProperty), Is.False);
    }

    [Test]
    public void MatchesReadAccess_False ()
    {
      var binding = new MethodInfoBinding (Method, AssociatedExpression);
      Assert.That (binding.MatchesReadAccess (Field), Is.False);
    }
  }
}
