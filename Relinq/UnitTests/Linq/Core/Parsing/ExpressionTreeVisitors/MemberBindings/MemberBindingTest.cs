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
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Linq.Parsing.ExpressionTreeVisitors.MemberBindings;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.MemberBindings
{
  [TestFixture]
  public class MemberBindingTest : MemberBindingTestBase
  {
    [Test]
    public void Bind_Method ()
    {
      var binding = MemberBinding.Bind (Method, AssociatedExpression);

      Assert.That (binding, Is.InstanceOfType (typeof (MethodInfoBinding)));
      Assert.That (binding.BoundMember, Is.SameAs (Method));
      Assert.That (binding.AssociatedExpression, Is.SameAs (AssociatedExpression));
    }

    [Test]
    public void Bind_Property ()
    {
      var binding = MemberBinding.Bind (Property, AssociatedExpression);

      Assert.That (binding, Is.InstanceOfType (typeof (PropertyInfoBinding)));
      Assert.That (binding.BoundMember, Is.SameAs (Property));
      Assert.That (binding.AssociatedExpression, Is.SameAs (AssociatedExpression));
    }

    [Test]
    public void Bind_Field ()
    {
      var binding = MemberBinding.Bind (Field, AssociatedExpression);

      Assert.That (binding, Is.InstanceOfType (typeof (FieldInfoBinding)));
      Assert.That (binding.BoundMember, Is.SameAs (Field));
      Assert.That (binding.AssociatedExpression, Is.SameAs (AssociatedExpression));
    }
  }
}
