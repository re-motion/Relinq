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
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps
{
  /// <summary>
  /// Analyzes an <see cref="Expression"/> tree for sub-trees that are evaluatable in-memory, and evaluates those sub-trees.
  /// </summary>
  /// <remarks>
  /// The <see cref="PartialEvaluationStep"/> uses the <see cref="PartialEvaluatingExpressionTreeVisitor"/> for partial evaluation.
  /// It performs two visiting runs over the <see cref="Expression"/> tree.
  /// </remarks>
  public class PartialEvaluationStep : IExpressionTreeProcessingStep
  {
    public Expression Process (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);

      return PartialEvaluatingExpressionTreeVisitor.EvaluateIndependentSubtrees (expressionTree);
    }
  }
}