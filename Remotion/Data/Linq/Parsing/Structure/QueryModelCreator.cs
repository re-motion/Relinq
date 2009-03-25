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
using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class QueryModelCreator
  {
    private readonly Expression _expressionTreeRoot;
    private readonly ParseResultCollector _result;
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();

    private IClause _previousClause;
    private int _currentProjection;
    private OrderByClause _previousOrderByClause;

    public QueryModelCreator (Expression expressionTreeRoot, ParseResultCollector result)
    {
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("result", result);

      _expressionTreeRoot = expressionTreeRoot;
      _result = result;
    }

    public QueryModel CreateQueryModel()
    {
      var mainFromClause = CreateMainFromClause (_result);

      _previousClause = mainFromClause;
      _currentProjection = 0;
      _previousOrderByClause = null;
      

      foreach (BodyExpressionDataBase bodyExpression in _result.BodyExpressions)
      {
        var clause = CreateBodyClause (bodyExpression);
        if (clause != _previousClause)
          _bodyClauses.Add (clause);

        _previousClause = clause;
      }

      var selectClause = CreateSelectClause();
      var queryModel = new QueryModel (_expressionTreeRoot.Type, mainFromClause, selectClause);
      
      foreach (IBodyClause bodyClause in _bodyClauses)
        queryModel.AddBodyClause (bodyClause);

      queryModel.SetExpressionTree (_expressionTreeRoot);

      return queryModel;
    }

    private MainFromClause CreateMainFromClause (ParseResultCollector resultCollector)
    {
      Assertion.IsTrue (resultCollector.BodyExpressions.Count > 0 && resultCollector.BodyExpressions[0] is FromExpressionData);

      FromExpressionData mainFromExpressionData = resultCollector.ExtractMainFromExpression ();
      return new MainFromClause (mainFromExpressionData.Identifier, mainFromExpressionData.TypedExpression);
    }

    private IBodyClause CreateBodyClause (BodyExpressionDataBase expression)
    {
      IBodyClause fromClause = CreateBodyFromClause (expression);
      if (fromClause != null)
        return fromClause;

      WhereClause whereClause = CreateWhereClause(expression);
      if (whereClause != null)
        return whereClause;

      OrderByClause orderByClause = CreateOrderByClause(expression);
      if (orderByClause != null)
        return orderByClause;

      LetClause letClause = CreateLetClause (expression);
      if (letClause != null)
        return letClause;

      throw new ParserException ("The FromLetWhereExpression type " + expression.GetType ().Name + " is not supported.");
    }

    private IBodyClause CreateBodyFromClause (BodyExpressionDataBase expression)
    {
      var fromExpression = expression as FromExpressionData;
      if (fromExpression == null)
        return null;

      if (_currentProjection >= _result.ProjectionExpressions.Count)
      {
        string message = string.Format ("From expression '{0}' ({1}) doesn't have a projection expression.", fromExpression.Identifier,
            fromExpression.TypedExpression);
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      var lambdaExpression = (LambdaExpression) fromExpression.TypedExpression;
      var projectionExpression = _result.ProjectionExpressions[_currentProjection];
      ++_currentProjection;

      return CreateBodyFromClause(fromExpression, lambdaExpression, projectionExpression);
    }

    private IBodyClause CreateBodyFromClause (FromExpressionData fromExpressionData, LambdaExpression lambdaExpression, LambdaExpression projectionExpression)
    {
      SubQueryExpression subQueryExpression = lambdaExpression.Body as SubQueryExpression;
      if (subQueryExpression != null)
      {
        QueryModel subQuery = subQueryExpression.QueryModel;
        return new SubQueryFromClause (_previousClause, fromExpressionData.Identifier, subQuery, projectionExpression);
      }
      else if (lambdaExpression.Body is MemberExpression)
        return new MemberFromClause (_previousClause, fromExpressionData.Identifier, lambdaExpression, projectionExpression);
      else
        return new AdditionalFromClause (_previousClause, fromExpressionData.Identifier, lambdaExpression, projectionExpression);
    }

    private WhereClause CreateWhereClause (BodyExpressionDataBase expression)
    {
      var whereExpression = expression as WhereExpressionData;
      if (whereExpression == null)
        return null;

      var whereClause = new WhereClause (_previousClause, whereExpression.TypedExpression);
      return whereClause;
    }

    private OrderByClause CreateOrderByClause (BodyExpressionDataBase expression)
    {
      var orderExpression = expression as OrderExpressionData;
      if (orderExpression == null)
        return null;

      if (orderExpression.FirstOrderBy)
      {
        var orderByClause = new OrderByClause (_previousClause);
        _previousOrderByClause = orderByClause;
      }
      else if (_previousOrderByClause == null)
      {
        throw ParserUtility.CreateParserException (
            "OrderBy or OrderByDescending", orderExpression, "beginning of an OrderBy clause", _expressionTreeRoot);
      }

      var ordering = new Ordering (_previousOrderByClause, orderExpression.TypedExpression, orderExpression.OrderingDirection);
      _previousOrderByClause.AddOrdering (ordering);
      return _previousOrderByClause;
    }

    private LetClause CreateLetClause (BodyExpressionDataBase expression)
    {
      var letExpression = expression as LetExpressionData;

      if (letExpression == null)
        return null;

      if (_currentProjection >= _result.ProjectionExpressions.Count)
      {
        string message = string.Format ("Let expression '{0}' ({1}) doesn't have a projection expression.", letExpression.Identifier,
            letExpression.TypedExpression);
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      var projectionExpression = _result.ProjectionExpressions[_currentProjection];
      var letClause = new LetClause (_previousClause, letExpression.Identifier, letExpression.TypedExpression, projectionExpression);
      ++_currentProjection;

      return letClause;
    }

    /// <summary>
    /// Create a <see cref="SelectClause"/> based on last entry for project expressions in <see cref="ParseResultCollector"/>.
    /// </summary>
    /// <returns><see cref="SelectClause"/></returns>
    private SelectClause CreateSelectClause ()
    {
      if (_result.ProjectionExpressions.Count == 0)
      {
        string message = "There is no projection for the select clause.";
        throw new ParserException (message, _expressionTreeRoot, _expressionTreeRoot, null);
      }

      // TODO 1096: Change QueryModelCreator.CreateSelectClause not to reuse old projection expressions but instead generate identity projections if the query source did not contain a Select clause
      //LambdaExpression selectProjection;
      //if (_currentProjection < _result.ProjectionExpressions.Count)
      //{
      //  selectProjection = _result.ProjectionExpressions[_currentProjection];
      //  ++_currentProjection;
      //}
      //else
      //{
      //  // If we have no more projections, the compiler optimized away the last select clause. Therefore, we'll create an identity projection
      //  // instead.
      //  selectProjection = CreateIdentityProjection (_result.ProjectionExpressions.Last().Type);
      //}

      LambdaExpression selectProjection = _result.ProjectionExpressions.Last ();
      SelectClause selectClause = new SelectClause (_previousClause, selectProjection);
      
      // TODO MG: Unfinished refactoring: missing test to prove whether previousClause changes
      IClause previousClause = selectClause;
      foreach (var resultModifier in _result.ResultModifierExpression)
      {
        ResultModifierClause resultModifierClause = new ResultModifierClause (previousClause, selectClause, resultModifier);
        selectClause.AddResultModifierData (resultModifierClause);
        previousClause = resultModifierClause;
      }
      return selectClause;
    }
  }
}
