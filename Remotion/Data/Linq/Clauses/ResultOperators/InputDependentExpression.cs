// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
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
using Remotion.Data.Linq.Clauses.ExpressionTreeVisitors;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses.ResultOperators
{
  /// <summary>
  /// Represents an expression that depends on what it provided by the previous operation in a LINQ expression.
  /// </summary>
  /// <remarks>
  /// <para>
  /// While naturally all expressions in a LINQ expression tree depend on their input, these dependencies can usually be resolved away 
  /// without any problems because within a <see cref="QueryModel"/>, every clause always has access to all the data produced by all the
  /// <see cref="FromClauseBase"/> instances in the query. Therefore, <see cref="IBodyClause"/> and <see cref="SelectClause"/> instances 
  /// have fully resolved expressions. 
  /// </para>
  /// <para>
  /// However, <see cref="ResultOperatorBase"/> instances are applied to the query's result set after all the clauses have been executed,
  /// and that in a linear fashion. Therefore, their expressions (if any) cannot access all the data produced by the query, but only
  /// what is given by the previous result operator. <see cref="ResultOperatorBase"/> subclasses that need to hold expressions should
  /// therefore use <see cref="InputDependentExpression"/> instead of <see cref="Expression"/> because <see cref="InputDependentExpression"/>
  /// represents that constraint by holding a <see cref="DependentExpression"/> in addition to the <see cref="ResolvedExpression"/>.
  /// </para>
  /// </remarks>
  public class InputDependentExpression
  {
    /// <summary>
    /// Initializes a new instance of the <see cref="InputDependentExpression"/> class.
    /// </summary>
    /// <param name="dependentExpression">The dependent expression; that is a <see cref="LambdaExpression"/> that transforms an input
    /// item as needed by this <see cref="InputDependentExpression"/>.</param>
    /// <param name="expectedInput">The expected input. This is needed for calculating the <see cref="ResolvedExpression"/> of this 
    /// <see cref="InputDependentExpression"/>.</param>
    public InputDependentExpression (LambdaExpression dependentExpression, Expression expectedInput)
    {
      ArgumentUtility.CheckNotNull ("dependentExpression", dependentExpression);
      ArgumentUtility.CheckNotNull ("expectedInput", expectedInput);

      if (dependentExpression.Parameters.Count != 1)
        throw new ArgumentException ("Dependent expression must have exactly one input parameter.", "dependentExpression");

      if (dependentExpression.Parameters[0].Type != expectedInput.Type)
        throw new ArgumentException ("The expected input must match the parameter of the dependent expression.", "expectedInput");

      DependentExpression = dependentExpression;
      ExpectedInput = expectedInput;

      ResolvedExpression = ReplacingExpressionTreeVisitor.Replace (InputParameter, ExpectedInput, DependentExpression.Body);
    }

    public LambdaExpression DependentExpression { get; private set; }
    public Expression ExpectedInput { get; private set; }
    public Expression ResolvedExpression { get; private set; }

    public ParameterExpression InputParameter
    {
      get { return DependentExpression.Parameters[0]; }
    }

    public override string ToString ()
    {
      return FormattingExpressionTreeVisitor.Format (ResolvedExpression);
    }
  }
}