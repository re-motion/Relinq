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
using System.Reflection;

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  public enum SandboxTestStatus { Succeeded, Ignored, Failed, FailedInSetUp, FailedInTearDown };

  /// <summary>
  /// <see cref="TestResult"/> represents a single result for a test method.
  /// </summary>
  [Serializable]
  public struct TestResult
  {
    public static TestResult CreateSucceeded (MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      return new TestResult (methodInfo, SandboxTestStatus.Succeeded, null);
    }

    public static TestResult CreateIgnored (MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      return new TestResult (methodInfo, SandboxTestStatus.Ignored, null);
    }

    public static TestResult CreateFailed (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      
      return new TestResult (methodInfo, SandboxTestStatus.Failed, exception);
    }

    public static TestResult CreateFailedInSetUp (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      if (exception == null)
        throw new ArgumentNullException ("exception"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      return new TestResult (methodInfo, SandboxTestStatus.FailedInSetUp, exception);
    }

    public static TestResult CreateFailedInTearDown (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      if (exception == null)
        throw new ArgumentNullException ("exception"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      return new TestResult (methodInfo, SandboxTestStatus.FailedInTearDown, exception);
    }

    public readonly MethodInfo MethodInfo;
    public readonly SandboxTestStatus Status;
    public readonly Exception Exception;

    private TestResult (MethodInfo methodInfo, SandboxTestStatus status, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      MethodInfo = methodInfo;
      Status = status;
      Exception = exception;
    }

    public void EnsureNotFailed ()
    {
      if (Status > SandboxTestStatus.Ignored)
        throw new TestFailedException (MethodInfo.DeclaringType, MethodInfo.Name, Status, Exception);
    }
  }
}