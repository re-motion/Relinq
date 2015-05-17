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
using Remotion.Linq.Parsing.ExpressionVisitors.Transformation;
using Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors;

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// <see cref="IExpressionTreeProcessor"/> is implemented by classes that represent steps in the process of parsing the structure
  /// of an <see cref="Expression"/> tree. <see cref="ExpressionTreeParser"/> applies a series of these steps to the <see cref="Expression"/>
  /// tree before analyzing the query operators and creating a <see cref="QueryModel"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// There are predefined implementations of <see cref="IExpressionTreeProcessor"/> that should only be left out when parsing an 
  /// <see cref="Expression"/> tree when there are very good reasons to do so.
  /// </para>
  /// <para>
  /// <see cref="IExpressionTreeProcessor"/> can be implemented to provide custom, complex transformations on an <see cref="Expression"/>
  /// tree. For performance reasons, avoid adding too many steps each of which visits the whole tree. For
  /// simple transformations, consider using <see cref="IExpressionTransformer{T}"/> and <see cref="TransformingExpressionTreeProcessor"/> - which can
  /// batch several transformations into a single expression tree visiting run - rather than implementing a dedicated 
  /// <see cref="IExpressionTreeProcessor"/>.
  /// </para>
  /// </remarks>
  public interface IExpressionTreeProcessor
  {
    Expression Process (Expression expressionTree);
  }
}