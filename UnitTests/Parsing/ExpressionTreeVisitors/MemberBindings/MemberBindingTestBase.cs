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
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting;

namespace Remotion.Linq.UnitTests.Parsing.ExpressionTreeVisitors.MemberBindings
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

    public int Foo { set { /* NOP */ } }

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
