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
using System.Reflection;
using System.Security;
using System.Security.Permissions;
using System.Security.Policy;

namespace Remotion.Data.Linq.Sandboxing
{
  public class Sandbox : IDisposable
  {
    public static Sandbox CreateSandbox (IPermission[] permissions, Assembly[] fullTrustAssemblies)
    {
      var appDomainSetup = AppDomain.CurrentDomain.SetupInformation;

      var permissionSet = new PermissionSet (null);
      foreach (var permission in permissions)
        permissionSet.AddPermission (permission);

      StrongName[] fullTrustStrongNames = null;
      if(fullTrustAssemblies!=null && fullTrustAssemblies.Length>0)
        fullTrustStrongNames = (from asm in fullTrustAssemblies
                                  let name = asm.GetName()
                                  let strongNamePublicKeyBlob = new StrongNamePublicKeyBlob (name.GetPublicKey())
                                  select new StrongName (strongNamePublicKeyBlob, name.Name, name.Version)).ToArray();
      var appDomain = AppDomain.CreateDomain ("Sandbox (" + DateTime.Now + ")", null, appDomainSetup, permissionSet, fullTrustStrongNames);
      return new Sandbox (appDomain);
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