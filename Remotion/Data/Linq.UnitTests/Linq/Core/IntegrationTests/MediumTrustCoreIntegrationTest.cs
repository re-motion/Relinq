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
using System.Linq;
using System.Security.Permissions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.UnitTests.Sandboxing;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.IntegrationTests
{
  [TestFixture]
  public class MediumTrustCoreIntegrationTest
  {
    [Test]
    public void MediumTrust ()
    {
      var mediumTrust = PermissionSets.GetMediumTrust (AppDomain.CurrentDomain.BaseDirectory, Environment.MachineName);
      var permissions = mediumTrust.Concat (new[] { new ReflectionPermission (ReflectionPermissionFlag.MemberAccess) }).ToArray ();

      var types = (from t in typeof (MediumTrustCoreIntegrationTest).Assembly.GetTypes ()
                   where (t.Namespace ?? string.Empty).StartsWith(typeof (MediumTrustCoreIntegrationTest).Namespace)
                       && t != typeof (MediumTrustCoreIntegrationTest)
                       && !t.IsAbstract && t.IsDefined (typeof (TestFixtureAttribute), false)
                   select t).ToArray ();

      var testFixtureResults = SandboxTestRunner.RunTestFixturesInSandbox (
          types,
          permissions,
          null);
      var testResults = testFixtureResults.SelectMany (r => r.TestResults);

      foreach (var testResult in testResults)
        testResult.EnsureNotFailed ();
      Assert.That (testResults.Count (r => r.Status == TestStatus.Succeeded), Is.GreaterThan (0));
    }
  }
}