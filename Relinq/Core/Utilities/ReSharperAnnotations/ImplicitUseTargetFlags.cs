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

namespace JetBrains.Annotations
{
  /// <summary>
  /// Specify what is considered used implicitly when marked with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>
  /// </summary>
  [Flags]
  internal enum ImplicitUseTargetFlags
  {
    Default = Itself,

    Itself = 1,

    /// <summary>
    /// Members of entity marked with attribute are considered used
    /// </summary>
    Members = 2,

    /// <summary>
    /// Entity marked with attribute and all its members considered used
    /// </summary>
    WithMembers = Itself | Members
  }
}