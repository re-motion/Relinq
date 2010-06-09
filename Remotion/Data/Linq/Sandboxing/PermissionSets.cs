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
using System.Data.SqlClient;
using System.Drawing.Printing;
using System.Net;
using System.Net.Mail;
using System.Security;
using System.Security.Permissions;
using System.Web;

namespace Remotion.Data.Linq.Sandboxing
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