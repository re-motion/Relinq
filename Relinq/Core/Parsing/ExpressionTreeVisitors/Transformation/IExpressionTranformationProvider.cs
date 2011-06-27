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
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation
{
  /// <summary>
  /// <see cref="IExpressionTranformationProvider"/> defines an API for classes returning <see cref="ExpressionTransformation"/> instances for specific 
  /// <see cref="Expression"/> objects. Usually, the <see cref="ExpressionTransformerRegistry"/> will be used when an implementation of this
  /// interface is needed.
  /// </summary>
  public interface IExpressionTranformationProvider
  {
    /// <summary>
    /// Gets the transformers for the given <see cref="Expression"/>.
    /// </summary>
    /// <param name="expression">The <see cref="Expression"/> to be transformed.</param>
    /// <returns>
    /// A sequence containing <see cref="ExpressionTransformation"/> objects that should be applied to the <paramref name="expression"/>. Must not
    /// be <see langword="null" />.
    /// </returns>
    IEnumerable<ExpressionTransformation> GetTransformations (Expression expression);
  }
}