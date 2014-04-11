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
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using Remotion.Utilities;

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  /// <summary>
  /// <see cref="SandboxTestRunner"/> executes unit tests for the given types.
  /// </summary>
  public class SandboxTestRunner : MarshalByRefObject
  {
    public static TestFixtureResult[] RunTestFixturesInSandbox (IEnumerable<Type> testFixtureTypes, IPermission[] permissions, Assembly[] fullTrustAssemblies)
    {
      ArgumentUtility.CheckNotNull ("testFixtureTypes", testFixtureTypes);
      ArgumentUtility.CheckNotNull ("permissions", permissions);

      using (var sandbox = Sandbox.CreateSandbox (permissions, fullTrustAssemblies))
      {
        var runner = sandbox.CreateSandboxedInstance<SandboxTestRunner> (permissions);
        return runner.RunTestFixtures (testFixtureTypes);
      }
    }

    public TestFixtureResult[] RunTestFixtures (IEnumerable<Type> testFixtureTypes)
    {
      if (testFixtureTypes == null)
        throw new ArgumentNullException ("testFixtureTypes"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      return testFixtureTypes.Select (t => RunTestFixture (t)).ToArray ();
    }

    public TestFixtureResult RunTestFixture (Type type)
    {
      if (type == null)
        throw new ArgumentNullException ("type"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      var testFixtureInstance = Activator.CreateInstance (type);

      var setupMethod = type.GetMethods ().Where (m => IsDefined (m, "NUnit.Framework.SetUpAttribute")).SingleOrDefault ();
      var tearDownMethod = type.GetMethods ().Where (m => IsDefined (m, "NUnit.Framework.TearDownAttribute")).SingleOrDefault ();
      var testMethods = type.GetMethods ().Where (m => IsDefined (m, "NUnit.Framework.TestAttribute"));

      var testResults = testMethods.Select (testMethod => RunTestMethod (testFixtureInstance, testMethod, setupMethod, tearDownMethod)).ToArray ();
      return new TestFixtureResult (type, testResults);
    }

    public TestResult RunTestMethod (object testFixtureInstance, MethodInfo testMethod, MethodInfo setupMethod, MethodInfo tearDownMethod)
    {
      Exception exception;
      if (IsDefined (testMethod, "NUnit.Framework.IgnoreAttribute") || IsDefined (testMethod.DeclaringType, "NUnit.Framework.IgnoreAttribute"))
        return TestResult.CreateIgnored (testMethod);

      if (setupMethod != null && !(TryInvokeMethod (setupMethod, testFixtureInstance, out exception)))
        return TestResult.CreateFailedInSetUp (setupMethod, exception);

      TestResult result;
      if (IsDefined (testMethod, "NUnit.Framework.ExpectedExceptionAttribute"))
      {
        var exceptionType = (Type) GetAttribute (testMethod, "NUnit.Framework.ExpectedExceptionAttribute").ConstructorArguments[0].Value;
        if (!TryInvokeMethod (testMethod, testFixtureInstance, out exception) && exception.GetType () == exceptionType)
          result = TestResult.CreateSucceeded (testMethod);
        else
          result = TestResult.CreateFailed (testMethod, exception);
      }
      else
      {
        if (TryInvokeMethod (testMethod, testFixtureInstance, out exception))
          result = TestResult.CreateSucceeded (testMethod);
        else
          result = TestResult.CreateFailed (testMethod, exception);
      }

      if (tearDownMethod != null && !TryInvokeMethod (tearDownMethod, testFixtureInstance, out exception))
        return TestResult.CreateFailedInTearDown (tearDownMethod, exception);
      else
        return result;
    }

    private bool TryInvokeMethod (MethodInfo method, object instance, out Exception exception)
    {
      exception = null;
      try
      {
        method.Invoke (instance, null);
      }
      catch (TargetInvocationException ex)
      {
        exception = ex.InnerException;
      }
      return exception == null;
    }

    private bool IsDefined (MemberInfo memberInfo, string attributeFullName)
    {
      var data = CustomAttributeData.GetCustomAttributes (memberInfo);
      return data.Any (attributeData => attributeData.Constructor.DeclaringType.FullName == attributeFullName);
    }

    private CustomAttributeData GetAttribute (MemberInfo memberInfo, string attributeName)
    {
      return CustomAttributeData.GetCustomAttributes (memberInfo).SingleOrDefault (ad => ad.Constructor.DeclaringType.FullName == attributeName);
    }
  }
}