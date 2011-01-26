// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// <see cref="IExpressionTreeProcessingStep"/> is implemented by classes that represent steps in the process of parsing the structure
  /// of an <see cref="Expression"/> tree. <see cref="ExpressionTreeParser"/> applies a series of these steps to the <see cref="Expression"/>
  /// tree before analyzing the query operators and creating a <see cref="QueryModel"/>.
  /// </summary>
  /// <remarks>
  /// <para>
  /// There are predefined implementations of <see cref="IExpressionTreeProcessingStep"/> that should only be left out when parsing an 
  /// <see cref="Expression"/> tree when there are very good reasons to do so.
  /// </para>
  /// <para>
  /// <see cref="IExpressionTreeProcessingStep"/> can be implemented to provide custom, complex transformations on an <see cref="Expression"/>
  /// tree. For performance reasons, avoid adding too many steps each of which visits the whole tree. For
  /// simple transformations, consider using <see cref="IExpressionTransformer{T}"/> and <see cref="ExpressionTransformationStep"/> - which can
  /// batch several transformations into a single expression tree visiting run - rather than implementing a dedicated 
  /// <see cref="IExpressionTreeProcessingStep"/>.
  /// </para>
  /// </remarks>
  public interface IExpressionTreeProcessingStep
  {
    Expression Process (Expression expressionTree);
  }
}