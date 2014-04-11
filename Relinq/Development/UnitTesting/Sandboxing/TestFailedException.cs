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
using System.Runtime.Serialization;

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  /// <summary>
  /// <see cref="TestFailedException"/> is thrown when <see cref="TestResult.EnsureNotFailed"/> is called and the specific test has been failed.
  /// </summary>
  [Serializable]
  public class TestFailedException : Exception
  {
    private static string CreateMessage (Type declaringType, string testName, SandboxTestStatus status)
    {
      return string.Format ("Test '{0}.{1}' failed. Status: {2}.", declaringType, testName, status);
    }

    public TestFailedException (Type declaringType, string testName, SandboxTestStatus status, Exception exception)
      : base (CreateMessage (declaringType, testName, status), exception)
    {
    }

    protected TestFailedException (SerializationInfo info, StreamingContext context)
        : base (info, context)
    {
    }
  }
}