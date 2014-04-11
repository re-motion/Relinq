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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;
using Remotion.Utilities;

// Note: This file is originally defined in Remotion.Development.UnitTesting.Sandboxing. It is duplicated by Remotion.Linq.UnitTests.Sandboxing.
// Note: Changes made to this file must be synchronized with all copies.

namespace Remotion.Linq.Development.UnitTesting.Sandboxing
{
  /// <summary>
  /// Provides functionality to run code in a sandboxed <see cref="AppDomain"/>, ie., an <see cref="AppDomain"/> with restricted permissions.
  /// </summary>
  /// <remarks>
  /// <para>
  /// Create a sandbox via <see cref="CreateSandbox"/> and specify the restricted permissions for ordinary assemblies in the sandbox, as well as
  /// a list of assemblies to be fully trusted. Fully-trusted assemblies might still need to call <see cref="CodeAccessPermission.Assert"/>
  /// in order to execute code restricted by the permission set.
  /// </para>
  /// <para>
  /// To execute code in the sandbox, use <see cref="System.AppDomain.DoCallBack"/> or
  /// create an instance of a class derived by <see cref="MarshalByRefObject"/> via <see cref="CreateSandboxedInstance{T}"/>.
  /// </para>
  /// </remarks>
  public class Sandbox : IDisposable
  {
    public static Sandbox CreateSandbox (IPermission[] permissions, Assembly[] fullTrustAssemblies)
    {
      ArgumentUtility.CheckNotNull ("permissions", permissions);

      var appDomainSetup = AppDomain.CurrentDomain.SetupInformation;

      var permissionSet = new PermissionSet (null);
      foreach (var permission in permissions)
        permissionSet.AddPermission (permission);

      StrongName[] fullTrustStrongNames = null;
      if (fullTrustAssemblies != null && fullTrustAssemblies.Length > 0)
      {
        try
        {
          fullTrustStrongNames = (from asm in fullTrustAssemblies
                                  let name = asm.GetName ()
                                  let publicKey = GetNonNullPublicKey (name)
                                  let strongNamePublicKeyBlob = new StrongNamePublicKeyBlob (publicKey)
                                  select new StrongName (strongNamePublicKeyBlob, name.Name, name.Version)).ToArray ();
        }
        catch (InvalidOperationException ex)
        {
          // This clock does not have a unit test because it's impossible/difficult to reference an unsigned assembly from a signed unit test project.
          throw new ArgumentException (ex.Message, "fullTrustAssemblies", ex);
        }
      }

      var appDomain = AppDomain.CreateDomain ("Sandbox (" + DateTime.Now + ")", null, appDomainSetup, permissionSet, fullTrustStrongNames);
      return new Sandbox (appDomain);
    }

    private static byte[] GetNonNullPublicKey (AssemblyName assemblyName)
    {
      var publicKey = assemblyName.GetPublicKey ();
      // This case does not have a unit test because it's impossible/difficult to reference an unsigned assembly from a signed unit test project.
      if (publicKey == null || publicKey.Length == 0)
      {
        var message = string.Format (
            "The assembly '{0}' does not have a public key. Only assemblies with a public key can be fully trusted.",
            assemblyName.Name);
        throw new InvalidOperationException (message);
      }

      return publicKey;
    }

    private readonly AppDomain _appDomain;

    private bool _isDisposed;

    public Sandbox (AppDomain appDomain)
    {
      _appDomain = appDomain;
    }

    public AppDomain AppDomain
    {
      get { return _appDomain; }
    }

    public void Dispose ()
    {
      if (!_isDisposed)
      {
        AppDomain.Unload (_appDomain);
        _isDisposed = true;
      }
    }

    public T CreateSandboxedInstance<T> (params IPermission[] permissions) where T : MarshalByRefObject, new ()
    {
      var instance = (T) AppDomain.CreateInstanceAndUnwrap (typeof (T).Assembly.FullName, typeof (T).FullName);
      return instance;
    }
  }
}