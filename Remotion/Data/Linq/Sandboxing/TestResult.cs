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
using System.Reflection;

namespace Remotion.Data.Linq.Sandboxing
{
  public enum TestStatus { Succeeded, Ignored, Failed, FailedInSetUp, FailedInTearDown };

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
      return new TestResult (methodInfo, TestStatus.Succeeded, null);
    }

    public static TestResult CreateIgnored (MethodInfo methodInfo)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      return new TestResult (methodInfo, TestStatus.Ignored, null);
    }

    public static TestResult CreateFailed (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      
      return new TestResult (methodInfo, TestStatus.Failed, exception);
    }

    public static TestResult CreateFailedInSetUp (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      if (exception == null)
        throw new ArgumentNullException ("exception"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      return new TestResult (methodInfo, TestStatus.FailedInSetUp, exception);
    }

    public static TestResult CreateFailedInTearDown (MethodInfo methodInfo, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM
      if (exception == null)
        throw new ArgumentNullException ("exception"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      return new TestResult (methodInfo, TestStatus.FailedInTearDown, exception);
    }

    public readonly MethodInfo MethodInfo;
    public readonly TestStatus Status;
    public readonly Exception Exception;

    private TestResult (MethodInfo methodInfo, TestStatus status, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      MethodInfo = methodInfo;
      Status = status;
      Exception = exception;
    }

    public void EnsureNotFailed ()
    {
      if (Status > TestStatus.Ignored)
        throw new TestFailedException (MethodInfo.DeclaringType, MethodInfo.Name, Status, Exception);
    }
  }
}