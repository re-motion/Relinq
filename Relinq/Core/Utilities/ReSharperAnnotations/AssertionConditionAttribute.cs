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

namespace Remotion.Linq.Utilities.ReSharperAnnotations
{
  /// <summary>
  /// Indicates the condition parameter of the assertion method. 
  /// The method itself should be marked by <see cref="AssertionMethodAttribute"/> attribute.
  /// The mandatory argument of the attribute is the assertion type.
  /// </summary>
  /// <seealso cref="AssertionConditionType"/>
  [AttributeUsage (AttributeTargets.Parameter, AllowMultiple = false, Inherited = true)]
  public sealed class AssertionConditionAttribute : Attribute
  {
    private readonly AssertionConditionType myConditionType;

    /// <summary>
    /// Initializes new instance of AssertionConditionAttribute
    /// </summary>
    /// <param name="conditionType">Specifies condition type</param>
    public AssertionConditionAttribute (AssertionConditionType conditionType)
    {
      myConditionType = conditionType;
    }

    /// <summary>
    /// Gets condition type
    /// </summary>
    public AssertionConditionType ConditionType
    {
      get { return myConditionType; }
    }
  }
}