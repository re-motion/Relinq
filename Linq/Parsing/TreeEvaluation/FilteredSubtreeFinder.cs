using System.Collections.Generic;
using System.Linq.Expressions;
using Rubicon.Data.Linq.Visitor;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.TreeEvaluation
{
  public class FilteredSubtreeFinder : ExpressionTreeVisitor
  {
    private readonly Expression _startNode;
    private readonly Func<Expression, bool> _filter;

    private readonly HashSet<Expression> _parameterlessSubtrees;
    private bool _allChildrenSubtrees;

    public FilteredSubtreeFinder (Expression startNode, Func<Expression, bool> filter)
    {
      ArgumentUtility.CheckNotNull ("startNode", startNode);
      ArgumentUtility.CheckNotNull ("filter", filter);

      _startNode = startNode;
      _filter = filter;

      _parameterlessSubtrees = new HashSet<Expression> ();
      _allChildrenSubtrees = true;

      FindSubtrees();
    }

    private void FindSubtrees()
    {
      VisitExpression (_startNode);

      if (_allChildrenSubtrees)
      {
        if (_filter (_startNode))
          _parameterlessSubtrees.Add (_startNode);
      }
    }

    public HashSet<Expression> GetFilteredSubtrees ()
    {
      return _parameterlessSubtrees;
    }

    protected override Expression VisitExpression (Expression expression)
    {
      if (expression == null)
        return null;
      else if (expression == _startNode)
        return base.VisitExpression (expression);
      else
      {
        FilteredSubtreeFinder childFinder = new FilteredSubtreeFinder(expression, _filter);
        HashSet<Expression> childTrees = childFinder.GetFilteredSubtrees ();
        if (!childTrees.Contains (expression))
          _allChildrenSubtrees = false;
        _parameterlessSubtrees.UnionWith (childTrees);
        return expression;
      }
    }
  }
}