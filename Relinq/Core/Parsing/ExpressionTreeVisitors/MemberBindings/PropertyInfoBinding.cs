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
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.MemberBindings
{
  /// <summary>
  /// Represents a <see cref="PropertyInfo"/> being bound to an associated <see cref="Expression"/> instance. 
  /// <seealso cref="System.Linq.Expressions.MemberBinding"/>
  /// This binding's 
  /// <see cref="MatchesReadAccess"/> method returns <see langword="true"/> for the same <see cref="PropertyInfo"/> the expression is bound to 
  /// or for its getter method's <see cref="MethodInfo"/>.
  /// </summary>
  public class PropertyInfoBinding : MemberBinding
  {
    public PropertyInfoBinding (PropertyInfo boundMember, Expression associatedExpression)
        : base (boundMember, associatedExpression)
    {
    }

    public override bool MatchesReadAccess (MemberInfo member)
    {
      if (member == BoundMember)
        return true;

      var methodInfo = member as MethodInfo;
      if (methodInfo != null && ((PropertyInfo) BoundMember).CanRead && methodInfo == ((PropertyInfo) BoundMember).GetGetMethod())
        return true;

      return false;
    }
  }
}
