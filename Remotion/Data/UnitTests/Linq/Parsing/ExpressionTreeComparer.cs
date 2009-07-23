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
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Utilities;
using System.Reflection;
using Assertion=Remotion.Utilities.Assertion;

namespace Remotion.Data.UnitTests.Linq.Parsing
{
  public class ExpressionTreeComparer
  {
    public static void CheckAreEqualTrees(Expression expressionTree1, Expression expressionTree2)
    {
      ExpressionTreeComparer comparer = new ExpressionTreeComparer (expressionTree1, expressionTree2);
      comparer.CheckAreEqualNodes (expressionTree1, expressionTree2);
    }

    private readonly object _object1;
    private readonly object _object2;

    public ExpressionTreeComparer (object object1, object object2)
    {
      ArgumentUtility.CheckNotNull ("object1", object1);
      ArgumentUtility.CheckNotNull ("object2", object2);

      _object1 = object1;
      _object2 = object2;
    }

    public void CheckAreEqualNodes (Expression e1, Expression e2)
    {
      if (e1 == null)
        Assert.IsNull (e2, GetMessage (e1, e2, "Null nodes"));
      else
      {
        Assert.AreEqual (e1.NodeType, e2.NodeType, GetMessage (e1, e2, "NodeType"));
        Assert.AreEqual (e1.Type, e2.Type, GetMessage (e1, e2, "Type"));
        CheckAreEqualObjects(e1, e2);
      }
    }

    public void CheckAreEqualObjects (object e1, object e2)
    {
      Assert.AreEqual (e1.GetType(), e2.GetType(), GetMessage (e1, e2, "GetType()"));

      foreach (PropertyInfo property in e1.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public))
      {
        object value1 = property.GetValue (e1, null);
        object value2 = property.GetValue (e2, null);
        CheckAreEqualProperties (property, property.PropertyType, value1, value2, e1, e2);
      }
    }

    private void CheckAreEqualProperties (PropertyInfo property, Type valueType, object value1, object value2, object e1, object e2)
    {
      if (typeof (Expression).IsAssignableFrom (valueType))
      {
        Expression subNode1 = (Expression) value1;
        Expression subNode2 = (Expression) value2;
        CheckAreEqualNodes (subNode1, subNode2);
      }
      else if (typeof (MemberBinding).IsAssignableFrom (valueType) || typeof (ElementInit).IsAssignableFrom (valueType))
      {
        CheckAreEqualObjects (value1, value2);
      }
      else if (ReflectionUtility.CanAscribe (valueType, typeof (ReadOnlyCollection<>)))
      {
        Type[] collectionGenericArguments = ReflectionUtility.GetAscribedGenericArguments (valueType, typeof (ReadOnlyCollection<>));
        Assertion.IsTrue (collectionGenericArguments.Length == 1, "ReadOnlyCollection only has one generic argument");
        Type elementType = collectionGenericArguments[0];
        
        IList list1 = (IList) value1;
        IList list2 = (IList) value2;
        if (list1 == null || list2 == null)
        {
          Assert.IsNull (list1, "One of the lists in " + property.Name + " is null.");
          Assert.IsNull (list2, "One of the lists in " + property.Name + " is null.");
        }
        else
        {
          Assert.AreEqual (list1.Count, list2.Count, GetMessage (e1, e2, "Number of elements in " + property.Name));
          for (int i = 0; i < list1.Count; ++i)
            CheckAreEqualProperties (property, elementType, list1[i], list2[i], e1, e2);
        }
      }
      else
        Assert.AreEqual (value1, value2, GetMessage (e1, e2, "Property " + property.Name));
    }

    private string GetMessage (object e1, object e2, string context)
    {
      return string.Format ("Trees are not equal: {0}\nNode 1: {1}\nNode 2: {2}\nTree 1: {3}\nTree 2: {4}", context, e1, e2, _object1, _object2);
    }
  }
}
