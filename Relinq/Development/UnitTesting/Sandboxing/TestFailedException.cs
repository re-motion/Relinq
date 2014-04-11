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