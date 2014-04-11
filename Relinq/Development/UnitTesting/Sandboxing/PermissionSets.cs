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
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.Permissions;
using System.Web;

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  /// <summary>
  /// <see cref="PermissionSets"/> contains static methods to get the permissions for different security levels.
  /// </summary>
  public static class PermissionSets
  {
    public static IPermission[] GetMediumTrust (string appDir, string originHost)
    {
      return new IPermission[]
               {
                   new AspNetHostingPermission (AspNetHostingPermissionLevel.Medium),
                   new DnsPermission (PermissionState.Unrestricted),
                   new EnvironmentPermission (EnvironmentPermissionAccess.Read, "TEMP;TMP;USERNAME;OS;COMPUTERNAME"), 
                   CreateRWAPFileIOPermission (appDir),
                   new IsolatedStorageFilePermission (PermissionState.None)
                     {UsageAllowed = IsolatedStorageContainment.AssemblyIsolationByUser, UserQuota = 9223372036854775807L},
                   new PrintingPermission (PrintingPermissionLevel.DefaultPrinting),
                   new SecurityPermission (SecurityPermissionFlag.Assertion | SecurityPermissionFlag.Execution | SecurityPermissionFlag.ControlThread
                                           | SecurityPermissionFlag.ControlPrincipal | SecurityPermissionFlag.RemotingConfiguration),
                   new SmtpPermission (SmtpAccess.Connect),
                   new SqlClientPermission (PermissionState.Unrestricted),
                   new WebPermission (NetworkAccess.Connect, originHost),
                   new ReflectionPermission (ReflectionPermissionFlag.RestrictedMemberAccess)
               };
    }

    private static FileIOPermission CreateRWAPFileIOPermission (params string[] paths)
    {
      var permission = new FileIOPermission (PermissionState.None);
      permission.AddPathList (FileIOPermissionAccess.Read, paths);
      permission.AddPathList (FileIOPermissionAccess.Write, paths);
      permission.AddPathList (FileIOPermissionAccess.Append, paths);
      permission.AddPathList (FileIOPermissionAccess.PathDiscovery, paths);
      return permission;
    }

  }
}