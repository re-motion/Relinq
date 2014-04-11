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