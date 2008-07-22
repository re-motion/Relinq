/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Utilities;
using System.Reflection;
using Assertion=Remotion.Utilities.Assertion;


namespace Remotion.Data.UnitTests.Linq.ParsingTest
{
  public class ExpressionTreeComparer
  {
    public static void CheckAreEqualTrees(Expression expressionTree1, Expression expressionTree2)
    {
      ExpressionTreeComparer comparer = new ExpressionTreeComparer (expressionTree1, expressionTree2);
      comparer.CheckAreEqualNodes (expressionTree1, expressionTree2);
    }

    private readonly Expression _expressionTreeRoot1;
    private readonly Expression _expressionTreeRoot2;

    public ExpressionTreeComparer (Expression expressionTreeRoot1, Expression expressionTreeRoot2)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot1", expressionTreeRoot1);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot2", expressionTreeRoot2);

      _expressionTreeRoot1 = expressionTreeRoot1;
      _expressionTreeRoot2 = expressionTreeRoot2;
    }

    public void CheckAreEqualNodes (Expression e1, Expression e2)
    {
      if (e1 == null)
        Assert.IsNull (e2, GetMessage (e1, e2, "Null nodes"));
      else
      {
        Assert.AreEqual (e1.NodeType, e2.NodeType, GetMessage (e1, e2, "NodeType"));
        Assert.AreEqual (e1.Type, e2.Type, GetMessage (e1, e2, "Type"));
        Assert.AreEqual (e1.GetType(), e2.GetType(), GetMessage (e1, e2, "GetType()"));

        foreach (PropertyInfo property in e1.GetType().GetProperties (BindingFlags.Instance | BindingFlags.Public))
        {
          object value1 = property.GetValue (e1, null);
          object value2 = property.GetValue (e2, null);
          CheckAreEqualProperties (property, property.PropertyType, value1, value2, e1, e2);
        }
      }
    }

    private void CheckAreEqualProperties (PropertyInfo property, Type valueType, object value1, object value2, Expression e1, Expression e2)
    {
      if (typeof (Expression).IsAssignableFrom (valueType))
      {
        Expression subNode1 = (Expression) value1;
        Expression subNode2 = (Expression) value2;
        CheckAreEqualNodes (subNode1, subNode2);
      }
      else if (ReflectionUtility.CanAscribe (valueType, typeof (ReadOnlyCollection<>)))
      {
        Type[] collectionGenericArguments = ReflectionUtility.GetAscribedGenericArguments (valueType, typeof (ReadOnlyCollection<>));
        Assertion.IsTrue (collectionGenericArguments.Length == 1, "ReadOnlyCollection only has one generic argument");
        Type elementType = collectionGenericArguments[0];
        
        IList list1 = (IList) value1;
        IList list2 = (IList) value2;
        Assert.AreEqual (list1.Count, list2.Count, GetMessage (e1, e2, "Number of elements in " + property.Name));
        for (int i = 0; i < list1.Count; ++i)
          CheckAreEqualProperties (property, elementType, list1[i], list2[i], e1, e2);
      }
      else
        Assert.AreEqual (value1, value2, GetMessage (e1, e2, "Property " + property.Name));
    }

    private string GetMessage (Expression e1, Expression e2, string context)
    {
      return string.Format ("Trees are not equal: {0}\nNode 1: {1}\nNode 2: {2}\nTree 1: {3}\nTree 2: {4}", context, e1, e2, _expressionTreeRoot1,
          _expressionTreeRoot2);
    }
  }
}
