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
using JetBrains.Annotations;

namespace Remotion.Linq.Clauses
{
  /// <summary>
  /// Common interface for from clauses (<see cref="AdditionalFromClause"/> and <see cref="MainFromClause"/>). From clauses define query sources that
  /// provide data items to the query which are filtered, ordered, projected, or otherwise processed by the following clauses.
  /// </summary>
  public interface IFromClause : IClause, IQuerySource
  {
    /// <summary>
    /// The expression generating the data items for this from clause.
    /// </summary>
    Expression FromExpression { get; }

    /// <summary>
    /// Copies the <paramref name="source"/>'s attributes, i.e. the <see cref="IQuerySource.ItemName"/>, <see cref="IQuerySource.ItemType"/>, and
    /// <see cref="FromExpression"/>.
    /// </summary>
    /// <param name="source"></param>
    void CopyFromSource ([NotNull] IFromClause source);
  }
}