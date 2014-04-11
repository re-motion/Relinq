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

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  /// <summary>
  /// <see cref="TestFixtureResult"/> holds the type of the test class and the result of the test methods.
  /// </summary>
  [Serializable]
  public struct TestFixtureResult
  {
    public readonly Type Type;
    public readonly TestResult[] TestResults;

    public TestFixtureResult (Type type, TestResult[] testResults)
    {
      if (type == null)
        throw new ArgumentNullException ("type"); // avoid ArgumentUtility, it doesn't support partial trust ATM

      Type = type;
      TestResults = testResults;
    }
  }
}