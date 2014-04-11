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
using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using Remotion.Linq.Development.UnitTesting.Sandboxing;

namespace Remotion.Linq.UnitTests.Parsing.Structure.QueryParserIntegrationTests
{
  [TestFixture]
  public class MediumTrustQueryParserIntegrationTest
  {
    [Test]
    public void MediumTrust ()
    {
      var mediumTrust = PermissionSets
          .GetMediumTrust (AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName)
          .Concat (new[] { new ReflectionPermission (ReflectionPermissionFlag.MemberAccess) })
          .ToArray();

      var types = (from t in typeof (MediumTrustQueryParserIntegrationTest).Assembly.GetTypes ()
                   where t.Namespace == typeof (MediumTrustQueryParserIntegrationTest).Namespace
                       && t != typeof (MediumTrustQueryParserIntegrationTest)
                       && !t.IsAbstract && t.IsDefined(typeof(TestFixtureAttribute), false)
                   select t).ToArray ();

      var testFixtureResults = SandboxTestRunner.RunTestFixturesInSandbox (types, mediumTrust, null);
      var testResults = testFixtureResults.SelectMany (r => r.TestResults);

      foreach (var testResult in testResults)
        testResult.EnsureNotFailed ();
      Assert.That (testResults.Count (r => r.Status == SandboxTestStatus.Succeeded), Is.GreaterThan (0));
    }
  }
}