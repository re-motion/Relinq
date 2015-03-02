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
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using Remotion.Utilities;

// ReSharper disable once CheckNamespace
namespace Remotion.Development.UnitTesting
{
  /// <summary>
  /// Provides utility functions for accessing non-public types and members.
  /// </summary>
  static partial class PrivateInvoke
  {
    // static members

    private static MethodInfo GetMethod (Type type, string methodName, BindingFlags bindingFlags, object[] arguments)
    {
      Debug.Assert (methodName != null);
      return (MethodInfo) GetMethodBaseInternal (type, methodName, type.GetMethods (bindingFlags), arguments);
    }

    private static ConstructorInfo GetConstructor (Type type, BindingFlags bindingFlags, object[] arguments)
    {
      return (ConstructorInfo) GetMethodBaseInternal (type, null, type.GetConstructors (bindingFlags), arguments);
    }

    private static MethodBase GetMethodBaseInternal (Type type, string methodName, MethodBase[] methods, object[] arguments)
    {
      MethodBase callMethod = null;

      foreach (MethodBase method in methods)
      {
        if (methodName == null || methodName == method.Name)
        {
          ParameterInfo[] parameters = method.GetParameters ();
          if (parameters.Length == arguments.Length)
          {
            bool isMatch = true;
            for (int i = 0; i < parameters.Length; ++i)
            {
              object argument = arguments[i];
              Type parameterType = parameters[i].ParameterType;

              if (! (    (argument == null && ! parameterType.IsValueType)        // null is a valid argument for any reference type
                      || (argument != null && parameterType.IsAssignableFrom (argument.GetType()))))  
              {
                isMatch = false;
                break;
              }
            }
            if (isMatch)
            {
              if (callMethod != null)
              {
                var message = string.Format ("There is no method \"{0}\" in type {1} that accepts the specified argument types.", methodName, type);
                throw new AmbiguousMatchException (message);
              }

              callMethod = method;
            }
          }
        }
      }
      if (callMethod == null)
        throw new MissingMethodException (type.Name, methodName);

      return callMethod;
    }

    private static PropertyInfo GetPropertyRecursive (Type type, BindingFlags bindingFlags, string propertyName)
    {
      for (PropertyInfo property = null; type != null; type = type.BaseType)
      {
        property = type.GetProperty (propertyName, bindingFlags);
        if (property != null)
          return property;
      }
      return null;
    }

    private static FieldInfo GetFieldRecursive (Type type, BindingFlags bindingFlags, string fieldName)
    {
      for (FieldInfo field = null; type != null; type = type.BaseType)
      {
        field = type.GetField (fieldName, bindingFlags);
        if (field != null)
          return field;
      }
      return null;
    }

    #region InvokeMethod methods

    public static object InvokeNonPublicStaticMethod (Type type, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);

      return InvokeMethodInternal (null, type, BindingFlags.Static | BindingFlags.NonPublic, methodName, arguments);
    }

    public static object InvokePublicStaticMethod (Type type, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("type", type);
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);

      return InvokeMethodInternal (null, type, BindingFlags.Static | BindingFlags.Public, methodName, arguments);
    }

    public static object InvokeNonPublicMethod (object target, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("target", target);

      return InvokeNonPublicMethod (target, target.GetType(), methodName, arguments);
    }

    public static object InvokeNonPublicMethod (object target, Type definingType, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("target", target);
      ArgumentUtility.CheckNotNull ("definingType", definingType);
      ArgumentUtility.CheckType ("target", target, definingType);
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);

      return InvokeMethodInternal (target, definingType, BindingFlags.Instance | BindingFlags.NonPublic, methodName, arguments);
    }

    public static object InvokePublicMethod (object target, string methodName, params object[] arguments)
    {
      ArgumentUtility.CheckNotNull ("target", target);
      ArgumentUtility.CheckNotNullOrEmpty ("methodName", methodName);

      return InvokeMethodInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.Public, methodName, arguments);
    }

    private static object InvokeMethodInternal (object instance, Type type, BindingFlags bindingFlags, string methodName, object[] arguments)
    {
      if (arguments == null)
        arguments = new object[] { null };

      MethodInfo callMethod = GetMethod (type, methodName, bindingFlags, arguments);

      try
      {
        return callMethod.Invoke (instance, bindingFlags, null, arguments, CultureInfo.InvariantCulture);
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace();
      }
    }

    #endregion

    #region CreateInstance methods

    public static object CreateInstancePublicCtor (string assemblyString, string typeName, params object[] arguments)
    {
      return CreateInstancePublicCtor (Assembly.Load (assemblyString), typeName, arguments);
    }

    public static object CreateInstancePublicCtor (Assembly assembly, string typeName, params object[] arguments)
    {
      return CreateInstancePublicCtor (assembly.GetType (typeName, true, false), arguments);
    }

    public static object CreateInstancePublicCtor (Type type, params object[] arguments)
    {
      return CreateInstanceInternal (type, true, arguments);
    }

    public static object CreateInstanceNonPublicCtor (string assemblyString, string typeName, params object[] arguments)
    {
      return CreateInstanceNonPublicCtor (Assembly.Load (assemblyString), typeName, arguments);
    }

    public static object CreateInstanceNonPublicCtor (Assembly assembly, string typeName, params object[] arguments)
    {
      return CreateInstanceNonPublicCtor (assembly.GetType (typeName, true, false), arguments);
    }

    public static object CreateInstanceNonPublicCtor (Type type, params object[] arguments)
    {
      return CreateInstanceInternal (type, false, arguments);
    }

    private static object CreateInstanceInternal (Type type, bool isPublic, object[] arguments)
    {
      if (arguments == null)
        arguments = new object[] { null };

      BindingFlags bindingFlags = BindingFlags.Instance;
      bindingFlags |= isPublic ? BindingFlags.Public : BindingFlags.NonPublic;

      ConstructorInfo ctor = GetConstructor (type, bindingFlags, arguments);

      try
      {
        return ctor.Invoke (BindingFlags.CreateInstance, null, arguments, CultureInfo.InvariantCulture);
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace ();
      }
    }

    #endregion

    #region GetProperty methods

    public static object GetPublicProperty (object target, string propertyName)
    {
      if (target == null) throw new ArgumentNullException ("target");
      return GetPropertyInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.Public, propertyName);
    }

    public static object GetNonPublicProperty (object target, string propertyName)
    {
      if (target == null) throw new ArgumentNullException ("target");
      return GetPropertyInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.NonPublic, propertyName);
    }

    public static object GetNonPublicProperty (object target, Type declaringType, string propertyName)
    {
      ArgumentUtility.CheckNotNull ("target", target);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      return GetPropertyInternal (target, declaringType, BindingFlags.Instance | BindingFlags.NonPublic, propertyName);
    }

    public static object GetPublicStaticProperty (Type type, string propertyName)
    {
      if (type == null) throw new ArgumentNullException ("type");
      return GetPropertyInternal (null, type, BindingFlags.Static | BindingFlags.Public, propertyName);
    }

    public static object GetNonPublicStaticProperty (Type type, string propertyName)
    {
      if (type == null) throw new ArgumentNullException ("type");
      return GetPropertyInternal (null, type, BindingFlags.Static | BindingFlags.NonPublic, propertyName);
    }

    private static object GetPropertyInternal (object instance, Type type, BindingFlags bindingFlags, string propertyName)
    {
      PropertyInfo property = GetPropertyRecursive (type, bindingFlags, propertyName);
      if (property == null)
      {
        throw new ArgumentException ("No property '" + propertyName + "' found on type '" + type.FullName + "' with binding flags '" + bindingFlags + "'.",
                                     "propertyName");
      }

      try
      {
        return property.GetValue (instance, new object[] {});
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace ();
      }
    }

    #endregion

    #region SetProperty methods

    public static void SetPublicProperty (object target, string propertyName, object value)
    {
      if (target == null) throw new ArgumentNullException ("target");
      SetPropertyInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.Public, propertyName, value);
    }

    public static void SetNonPublicProperty (object target, string propertyName, object value)
    {
      if (target == null) throw new ArgumentNullException ("target");
      SetPropertyInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.NonPublic, propertyName, value);
    }

    public static void SetNonPublicProperty (object target, Type declaringType, string propertyName, object value)
    {
      if (target == null)
        throw new ArgumentNullException ("target");
      SetPropertyInternal (target, declaringType, BindingFlags.Instance | BindingFlags.NonPublic, propertyName, value);
    }

    public static void SetPublicStaticProperty (Type type, string propertyName, object value)
    {
      if (type == null) throw new ArgumentNullException ("type");
      PropertyInfo property = type.GetProperty (propertyName, BindingFlags.Static | BindingFlags.Public);
      SetPropertyInternal (null, type, BindingFlags.Static | BindingFlags.Public, propertyName, value);
    }

    public static void SetNonPublicStaticProperty (Type type, string propertyName, object value)
    {
      if (type == null) throw new ArgumentNullException ("type");
      SetPropertyInternal (null, type, BindingFlags.Static | BindingFlags.NonPublic, propertyName, value);
    }

    private static void SetPropertyInternal (object instance, Type type, BindingFlags bindingFlags, string propertyName, object value)
    {
      PropertyInfo property = GetPropertyRecursive (type, bindingFlags, propertyName);
      try
      {
        property.SetValue (instance, value, new object[] {});
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace ();
      }
    }

    #endregion

    #region GetField methods

    public static object GetPublicField (object target, string fieldName)
    {
      if (target == null) throw new ArgumentNullException ("target");
      return GetFieldInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.Public, fieldName);
    }

    public static object GetNonPublicField (object target, string fieldName)
    {
      if (target == null) throw new ArgumentNullException ("target");
      var declaringType = target.GetType();
      return GetNonPublicField(target, declaringType, fieldName);
    }

    public static object GetNonPublicField(object target, Type declaringType, string fieldName)
    {
      ArgumentUtility.CheckNotNull ("target", target);
      ArgumentUtility.CheckNotNull ("declaringType", declaringType);

      return GetFieldInternal (target, declaringType, BindingFlags.Instance | BindingFlags.NonPublic, fieldName);
    }

    public static object GetPublicStaticField (Type type, string fieldName)
    {
      if (type == null) throw new ArgumentNullException ("type");
      return GetFieldInternal (null, type, BindingFlags.Static | BindingFlags.Public, fieldName);
    }

    public static object GetNonPublicStaticField (Type type, string fieldName)
    {
      if (type == null) throw new ArgumentNullException ("type");
      return GetFieldInternal (null, type, BindingFlags.Static | BindingFlags.NonPublic, fieldName);
    }

    private static object GetFieldInternal (object instance, Type type, BindingFlags bindingFlags, string fieldName)
    {
      FieldInfo field = GetFieldRecursive (type, bindingFlags, fieldName);
      if (field == null)
      {
        throw new ArgumentException ("No field '" + fieldName + "' found on type '" + type.FullName + "' with binding flags '" + bindingFlags + "'.",
                                     "fieldName");
      }

      try
      {
        return field.GetValue (instance);
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace ();
      }
    }

    #endregion

    #region SetField methods

    public static void SetPublicField (object target, string fieldName, object value)
    {
      if (target == null) throw new ArgumentNullException ("target");
      SetFieldInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.Public, fieldName, value);
    }

    public static void SetNonPublicField (object target, string fieldName, object value)
    {
      if (target == null) throw new ArgumentNullException ("target");
      SetFieldInternal (target, target.GetType(), BindingFlags.Instance | BindingFlags.NonPublic, fieldName, value);
    }

    public static void SetPublicStaticField (Type type, string fieldName, object value)
    {
      if (type == null) throw new ArgumentNullException ("type");
      SetFieldInternal (null, type, BindingFlags.Static | BindingFlags.Public, fieldName, value);
    }

    public static void SetNonPublicStaticField (Type type, string fieldName, object value)
    {
      if (type == null) throw new ArgumentNullException ("type");
      SetFieldInternal (null, type, BindingFlags.Static | BindingFlags.NonPublic, fieldName, value);
    }

    private static void SetFieldInternal (object instance, Type type, BindingFlags bindingFlags, string fieldName, object value)
    {
      FieldInfo field = GetFieldRecursive (type, bindingFlags, fieldName);
      try
      {
        field.SetValue (instance, value);
      }
      catch (TargetInvocationException e)
      {
        throw e.InnerException.PreserveStackTrace ();
      }
    }

    #endregion
  }
}
