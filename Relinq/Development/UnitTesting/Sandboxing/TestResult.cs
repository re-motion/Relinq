// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
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