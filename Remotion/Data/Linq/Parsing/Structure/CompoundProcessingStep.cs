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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.Parsing.Structure
{
  /// <summary>
  /// Implements <see cref="IExpressionTreeProcessingStep"/> by storing a list of inner <see cref="IExpressionTreeProcessingStep"/> instances.
  /// The <see cref="Process"/> method calls each inner instance in the order defined by the <see cref="InnerSteps"/> property.
  /// </summary>
  public class CompoundProcessingStep : IExpressionTreeProcessingStep
  {
    private readonly List<IExpressionTreeProcessingStep> _innerSteps;

    public CompoundProcessingStep (IEnumerable<IExpressionTreeProcessingStep> innerSteps)
    {
      ArgumentUtility.CheckNotNull ("innerSteps", innerSteps);
      _innerSteps = new List<IExpressionTreeProcessingStep> (innerSteps);
    }

    public IList<IExpressionTreeProcessingStep> InnerSteps
    {
      get { return _innerSteps; }
    }
    
    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      return _innerSteps.Aggregate (expressionTree, (expr, step) => step.Process (expr));
    }
  }
}