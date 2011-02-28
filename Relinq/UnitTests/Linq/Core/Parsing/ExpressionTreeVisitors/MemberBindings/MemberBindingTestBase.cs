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
using System.Reflection;
using NUnit.Framework;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestUtilities;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors.MemberBindings
{
  public abstract class MemberBindingTestBase
  {
    private Expression _associatedExpression;
    private MethodInfo _method;
    private PropertyInfo _property;
    private FieldInfo _field;

    public Expression AssociatedExpression
    {
      get { return _associatedExpression; }
    }

    public MethodInfo Method
    {
      get { return _method; }
    }

    public MethodInfo OtherMethod
    {
      get { return typeof (MemberBindingTestBase).GetMethod ("get_OtherMethod"); }
    }

    public PropertyInfo Property
    {
      get { return _property; }
    }

    public PropertyInfo OtherProperty
    {
      get { return typeof (MemberBindingTestBase).GetProperty ("OtherProperty"); }
    }

    public PropertyInfo WriteOnlyProperty
    {
      get { return typeof (MemberBindingTestBase).GetProperty ("Foo"); }
    }

    public int Foo { set { Dev.Null = value; } }

      public FieldInfo Field
    {
      get { return _field; }
    }

    [SetUp]
    public virtual void SetUp ()
    {
      _associatedExpression = ExpressionHelper.CreateExpression();
      _method = typeof (AnonymousType).GetMethod ("get_a");
      _property = typeof (AnonymousType).GetProperty ("a");
      _field = typeof (MemberBindingTestBase).GetField ("_associatedExpression", BindingFlags.Instance | BindingFlags.NonPublic);
    }
  }
}
