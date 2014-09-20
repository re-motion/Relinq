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
using System.Collections;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using JetBrains.Annotations;
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting
{
  /// <summary>
  /// Base class for compares two <see cref="Expression"/> trees constructed from <b>System.Linq</b> expressions.
  /// </summary>
  public abstract class ExpressionTreeComparerBase
  {
    private readonly string _expectedInitial;
    private readonly string _actualInitial;
    private readonly Type[] _structurallyComparedTypes;

    protected ExpressionTreeComparerBase (string expectedInitial, string actualInitial, params Type[] additionalStructurallyComparedTypes)
    {
      ArgumentUtility.CheckNotNull ("expectedInitial", expectedInitial);
      ArgumentUtility.CheckNotNull ("actualInitial", actualInitial);
      ArgumentUtility.CheckNotNull ("additionalStructurallyComparedTypes", additionalStructurallyComparedTypes);

      _expectedInitial = expectedInitial;
      _actualInitial = actualInitial;
      _structurallyComparedTypes = new[] { typeof (MemberBinding), typeof (ElementInit) }.Concat (additionalStructurallyComparedTypes).ToArray();
    }

    protected void CheckAreEqualNodes ([CanBeNull] Expression expected, [CanBeNull] Expression actual)
    {
      if (expected == null)
      {
        Assert.AreEqual (null, actual, GetMessage (null, actual, "Null nodes"));
      }
      else
      {
        Assert.IsNotNull (actual, "Actual node is null");
        Assert.AreEqual (expected.NodeType, actual.NodeType, GetMessage (expected.NodeType, actual.NodeType, "NodeType"));
        Assert.AreEqual (expected.Type, actual.Type, GetMessage (expected.Type, actual.Type, "Type"));
        CheckAreEqualObjects (expected, actual);
      }
    }

    private void CheckAreEqualObjects (object expected, object actual)
    {
      if (expected is Expression && actual is Expression)
      {
        var expectedType = expected.GetType();
        var actualType = actual.GetType();
        var publicExpectedType = GetPublicType (expectedType);
        var publicActualType = GetPublicType (actualType);
        Assert.AreEqual (publicExpectedType, publicActualType, GetMessage (expected.GetType(), actual.GetType(), "GetType()"));
      }
      else
      {
        Assert.AreEqual (expected.GetType(), actual.GetType(), GetMessage (expected.GetType(), actual.GetType(), "GetType()"));
      }

      foreach (PropertyInfo property in expected.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public))
      {
        object expectedValue = property.GetValue (expected, null);
        object actualValue = property.GetValue (actual, null);
        var valueType = expectedValue != null ? expectedValue.GetType() : property.PropertyType;
        CheckAreEqualProperties (property, valueType, expectedValue, actualValue, expected, actual);
      }
    }

    private void CheckAreEqualProperties (PropertyInfo property, Type valueType, object value1, object value2, object e1, object e2)
    {
      var structurallyComparedTypes = _structurallyComparedTypes;
      if (typeof (Expression).IsAssignableFrom (valueType))
      {
        Expression subNode1 = (Expression) value1;
        Expression subNode2 = (Expression) value2;
        CheckAreEqualNodes (subNode1, subNode2);
      }
      else if (structurallyComparedTypes.Any (t => t.IsAssignableFrom (valueType)))
      {
        CheckAreEqualObjects (value1, value2);
      }
      else if (typeof (IList).IsAssignableFrom (valueType))
      {
        IList list1 = (IList) value1;
        IList list2 = (IList) value2;
        if (list1 == null || list2 == null)
        {
          Assert.AreEqual (list1, list2, "One of the lists in property '" + property.Name + "' is null.");
        }
        else
        {
          Assert.AreEqual (list1.Count, list2.Count, GetMessage (list1.Count, list2.Count, "Number of elements in property '" + property.Name + "'"));
          for (int i = 0; i < list1.Count; ++i)
          {
            var elementType1 = list1[i] != null ? list1[i].GetType () : typeof (object);
            var elementType2 = list2[i] != null ? list2[i].GetType () : typeof (object);
            Assert.AreEqual (
                elementType1, 
                elementType2, 
                string.Format (
                    "The item types of the items in the lists in property '{0}' differ: One is '{1}', the other is '{2}'.\nTree 1: {3}\nTree 2: {4}", 
                    property.Name, 
                    elementType1, 
                    elementType2,
                    _expectedInitial, 
                    _actualInitial));

            CheckAreEqualProperties (property, elementType1, list1[i], list2[i], e1, e2);
          }
        }
      }
      else
        Assert.AreEqual (value1, value2, GetMessage (e1, e2, "Property '" + property.Name + "'"));
    }

    private string GetMessage (object e1, object e2, string context)
    {
      return string.Format ("Trees are not equal: {0}\nNode 1: {1}\nNode 2: {2}\nTree 1: {3}\nTree 2: {4}", context, e1, e2, _expectedInitial, _actualInitial);
    }

    private object GetPublicType (Type type)
    {
      for (var currentType = type; currentType != null; currentType = currentType.BaseType)
      {
        if (currentType.IsPublic)
          return currentType;
      }
      throw new InvalidOperationException ("Unreachable code because every type eventually results in a public base type.");
    }
  }

  internal static class Assert
  {
    public static void AreEqual (object expected, object actual, string message)
    {
      if (expected == actual)
        return;

      if (expected == null)
        throw new InvalidOperationException (message);

      if (expected.Equals (actual))
        return;

      var expectedEnumerable = expected as IEnumerable;
      var actualEnumerable = actual as IEnumerable;
      if (expectedEnumerable != null && actualEnumerable != null && expectedEnumerable.Cast<object>().SequenceEqual (actualEnumerable.Cast<object>()))
        return;

      throw new InvalidOperationException (message);
    }

    public static void IsNotNull (object actual, string message)
    {
      if (actual == null)
        throw new InvalidOperationException (message);
    }
  }
}
