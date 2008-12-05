// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing.TreeEvaluation;
using Remotion.Data.Linq.Visitor;

namespace Remotion.Data.Linq.Parsing.TreeEvaluation
{
  public sealed class PartialTreeEvaluator : ExpressionTreeVisitor
  {
    public static ConstantExpression EvaluateSubtree (Expression subtree)
    {
      if (subtree.NodeType == ExpressionType.Constant)
        return (ConstantExpression) subtree;
      else
      {
        LambdaExpression lambdaWithoutParameters = Expression.Lambda (subtree);
        object value = lambdaWithoutParameters.Compile ().DynamicInvoke ();
        return Expression.Constant (value, subtree.Type);
      }
    }

    // _partialEvaluationData contains a list of the used parameters and a list of the declared parameters for each expression in the tree. We will 
    // evaluate an expression if it only uses parameters declared within or below the same expression.
    private readonly PartialEvaluationData _partialEvaluationData;
    private readonly Expression _evaluatedTree;

    public PartialTreeEvaluator (Expression treeRoot)
    {
      PartialEvaluationPreAnalyzer analyzer = new PartialEvaluationPreAnalyzer();
      analyzer.Analyze (treeRoot);
      _partialEvaluationData = analyzer.EvaluationData;
      _evaluatedTree = VisitExpression (treeRoot);
    }

    public Expression GetEvaluatedTree ()
    {
      return _evaluatedTree;
    }

    protected override Expression VisitExpression (Expression expression)
    {
      // Only evaluate expressions for which the set of used parameters is a subset of the set of declared parameters. Don't evaluate
      // lambda expressions, we need to analyze those later on.
      // (Invocations of lambda expressions are ok.)
      if (expression == null)
        return null;
      else if (expression.NodeType != ExpressionType.Lambda && IsEvaluatableExpression(expression))
        return EvaluateSubtree (expression);
      else
        return base.VisitExpression (expression);
    }

    private bool IsEvaluatableExpression (Expression expression)
    {
      if (!_partialEvaluationData.UsedParameters.ContainsKey (expression)) 
        return false;
      else
        return _partialEvaluationData.DeclaredParameters[expression].IsSupersetOf (_partialEvaluationData.UsedParameters[expression])
            && _partialEvaluationData.SubQueries[expression].Count == 0;
    }
  }
}
