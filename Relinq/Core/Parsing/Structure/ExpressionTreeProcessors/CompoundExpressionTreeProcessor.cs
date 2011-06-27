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
using System.Linq;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.Structure.ExpressionTreeProcessors
{
  /// <summary>
  /// Implements <see cref="IExpressionTreeProcessor"/> by storing a list of inner <see cref="IExpressionTreeProcessor"/> instances.
  /// The <see cref="Process"/> method calls each inner instance in the order defined by the <see cref="InnerProcessors"/> property. This is an
  /// implementation of the Composite Pattern.
  /// </summary>
  public class CompoundExpressionTreeProcessor : IExpressionTreeProcessor
  {
    private readonly List<IExpressionTreeProcessor> _innerProcessors;

    public CompoundExpressionTreeProcessor (IEnumerable<IExpressionTreeProcessor> innerProcessors)
    {
      ArgumentUtility.CheckNotNull ("innerProcessors", innerProcessors);
      _innerProcessors = new List<IExpressionTreeProcessor> (innerProcessors);
    }

    public IList<IExpressionTreeProcessor> InnerProcessors
    {
      get { return _innerProcessors; }
    }
    
    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      return _innerProcessors.Aggregate (expressionTree, (expr, processor) => processor.Process (expr));
    }
  }
}