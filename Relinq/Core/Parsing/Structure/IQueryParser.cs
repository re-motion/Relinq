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