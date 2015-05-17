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

namespace Remotion.Linq.UnitTests.Parsing
{
  [TestFixture]
  public class NewExpressionIntegrationTest
  {
    private ConstructorInfo _ctorWithoutParameter;
    private ConstructorInfo _ctorWithOneParameter;
    private PropertyInfo _propertyInfo;
    private ConstantExpression _argumentValue;

    private class TheClass
    {
      public TheClass (string TheProperty)
      {
      }

      public TheClass()
      {}

      public string TheProperty
      {
        get { return null; }
      }
    }

    [SetUp]
    public void SetUp ()
    {
      _ctorWithoutParameter = typeof (TheClass).GetConstructor (new Type[0] );
      Assert.That (_ctorWithoutParameter, Is.Not.Null);

      _ctorWithOneParameter = typeof (TheClass).GetConstructor (new[] { typeof (string) });
      Assert.That (_ctorWithOneParameter, Is.Not.Null);

      _propertyInfo = typeof (TheClass).GetProperty ("TheProperty");
      Assert.That (_propertyInfo, Is.Not.Null);

      _argumentValue = Expression.Constant ("value");
    }

    [Test]
    public void Initialize_WithArguments_AndWithoutMembers ()
    {
      var expression = Expression.New (_ctorWithOneParameter, new[] { _argumentValue });
      Assert.That (expression.Arguments, Is.EqualTo (new[] { _argumentValue }));
      Assert.That (expression.Members, Is.Null);
    }

    [Test]
    public void Initialize_WithArguments_AndWitMembers ()
    {
      var expression = Expression.New (_ctorWithOneParameter, new[] { _argumentValue }, new[] { _propertyInfo });
      Assert.That (expression.Arguments, Is.EqualTo (new[] { _argumentValue }));
      Assert.That (expression.Members, Is.EqualTo (new[] { _propertyInfo }));
    }

    [Test]
    public void Initialize_WithArguments_AndMembersNull_ThrowsArgumentException ()
    {
      Assert.That (
          () => Expression.New (_ctorWithOneParameter, new[] { _argumentValue }, null),
          Throws.ArgumentException.With.Message.StartsWith ("Incorrect number of arguments for the given members"));
    }

    [Test]
    public void Initialize_WithArguments_AndMembersEmpty_ThrowsArgumentException ()
    {
      Assert.That (
          () => Expression.New (_ctorWithOneParameter, new[] { _argumentValue }, new MemberInfo[0]),
          Throws.ArgumentException.With.Message.StartsWith ("Incorrect number of arguments for the given members"));
    }

    [Test]
    public void Initialize_WithoutArguments_AndWithoutMembers ()
    {
      var expression = Expression.New (_ctorWithoutParameter, new Expression[0]);
      Assert.That (expression.Arguments, Is.Empty);
      Assert.That (expression.Members, Is.Null);
    }

    [Test]
    public void Initialize_WithoutArguments_AndMembersNull_ThrowsArgumentException ()
    {
      var expression = Expression.New (_ctorWithoutParameter, new Expression[0]);
      Assert.That (expression.Arguments, Is.Empty);
      Assert.That (expression.Members, Is.Null);
    }

    [Test]
    public void Initialize_WithoutArguments_AndMembersEmpty_ThrowsArgumentException ()
    {
      var expression = Expression.New (_ctorWithoutParameter, new Expression[0]);
      Assert.That (expression.Arguments, Is.Empty);
      Assert.That (expression.Members, Is.Null);
    }
  }
}