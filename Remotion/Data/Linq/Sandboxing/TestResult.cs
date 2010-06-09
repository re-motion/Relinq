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
    public TestResult (MethodInfo methodInfo, TestStatus status, Exception exception)
    {
      if (methodInfo == null)
        throw new ArgumentNullException ("methodInfo"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      MethodInfo = methodInfo;
      Status = status;
      Exception = exception;
    }

    public readonly MethodInfo MethodInfo;
    public readonly TestStatus Status;
    public readonly Exception Exception;

    public void EnsureNotFailed ()
    {
      if ((int)Status > 1)
        throw new TestFailedException (MethodInfo.DeclaringType.FullName + "." + MethodInfo.Name + " failed. Exception: " + Exception, Exception);
    }
  }
}