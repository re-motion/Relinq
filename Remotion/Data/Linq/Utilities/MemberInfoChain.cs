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
using System.Reflection;

namespace Remotion.Data.Linq.Utilities
{
  /// <summary>
  /// Represents a chain of members leading over several joined members to an accessed member.
  /// This models the following expression: start.P1.P2.P3 - P1 and P2 are joined members, P3 is the accessed member.
  /// </summary>
  public struct MemberInfoChain
  {
    public MemberInfoChain (MemberInfo[] joinedMembers, MemberInfo accessedMember) : this()
    {
      AccessedMember = accessedMember;
      JoinedMembers = joinedMembers;
    }

    public MemberInfo[] JoinedMembers { get; private set; }
    public MemberInfo AccessedMember { get; private set; }
  }
}