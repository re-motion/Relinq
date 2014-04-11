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

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Represents a clause within the <see cref="QueryModel"/>. Implemented by <see cref="MainFromClause"/>, <see cref="SelectClause"/>, 
  /// <see cref="IBodyClause"/>, and <see cref="JoinClause"/>.
  /// </summary>
  public interface IClause
  {
    /// <summary>
    /// Transforms all the expressions in this clause and its child objects via the given <paramref name="transformation"/> delegate.
    /// </summary>
    /// <param name="transformation">The transformation object. This delegate is called for each <see cref="Expression"/> within this 
    /// clause, and those expressions will be replaced with what the delegate returns.</param>
    void TransformExpressions (Func<Expression, Expression> transformation);
  }
}
