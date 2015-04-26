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

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation
{
  /// <summary>
  /// Transforms a given <see cref="Expression"/>. If the <see cref="ExpressionTransformation"/> can handle the <see cref="Expression"/>,
  /// it should return a new, transformed <see cref="Expression"/> instance. Otherwise, it should return the input <paramref name="expression"/> 
  /// instance.
  /// </summary>
  /// <param name="expression">The expression to be transformed.</param>
  /// <returns>The result of the transformation, or <paramref name="expression"/> if no transformation was applied.</returns>
  public delegate Expression ExpressionTransformation (Expression expression);
}