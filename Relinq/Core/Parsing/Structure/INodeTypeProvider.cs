// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// Provides a common interface for classes mapping a <see cref="MethodInfo"/> to the respective <see cref="IExpressionNode"/>
  /// type. Implementations are used by <see cref="ExpressionTreeParser"/> when a <see cref="MethodCallExpression"/> is encountered to 
  /// instantiate the right <see cref="IExpressionNode"/> for the given method.
  /// </summary>
  public interface INodeTypeProvider
  {
    /// <summary>
    /// Determines whether a node type for the given <see cref="MethodInfo"/> can be returned by this 
    /// <see cref="INodeTypeProvider"/>.
    /// </summary>
    bool IsRegistered (MethodInfo method);

    /// <summary>
    /// Gets the type of <see cref="IExpressionNode"/> that matches the given <paramref name="method"/>, returning <see langword="null" /> 
    /// if none can be found.
    /// </summary>
    Type GetNodeType (MethodInfo method);
  }
}