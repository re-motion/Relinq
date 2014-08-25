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

namespace Remotion.Linq.Parsing.Structure
{
  /// <summary>
  /// <see cref="IQueryParser"/> is implemented by classes taking an <see cref="Expression"/> tree and parsing it into a <see cref="QueryModel"/>.
  /// </summary>
  /// <remarks>
  /// The default implementation of this interface is <see cref="QueryParser"/>. LINQ providers can, however, implement <see cref="IQueryParser"/>
  /// themselves, eg. in order to decorate or replace the functionality of <see cref="QueryParser"/>.
  /// </remarks>
  public interface IQueryParser
  {
    /// <summary>
    /// Gets the <see cref="QueryModel"/> of the given <paramref name="expressionTreeRoot"/>.
    /// </summary>
    /// <param name="expressionTreeRoot">The expression tree to parse.</param>
    /// <returns>A <see cref="QueryModel"/> that represents the query defined in <paramref name="expressionTreeRoot"/>.</returns>
    QueryModel GetParsedQuery (Expression expressionTreeRoot);
  }
}