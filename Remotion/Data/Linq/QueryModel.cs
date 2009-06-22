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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.StringBuilding;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The class implements an abstraction of the expression tree which is created when executing a linq query. 
  /// The different parts of a linq query are mapped to clauses.
  /// </summary>
  public class QueryModel : ICloneable
  {
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();
    private readonly Dictionary<string, IResolveableClause> _clausesByIdentifier = new Dictionary<string, IResolveableClause> ();
    private readonly QueryModelUniqueIdentifierGenerator _uniqueIdentifierGenerator;

    private ISelectGroupClause _selectOrGroupClause;
    private Expression _expressionTree;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryModel"/>
    /// </summary>
    /// <param name="resultType">The type of the underlying LINQ query, usually a type implementing <see cref="IQueryable{T}"/>.</param>
    /// <param name="mainFromClause">The first from in a linq query mapped to <see cref="MainFromClause"/></param>
    /// <param name="selectOrGroupClause">The Select mapped to <see cref="SelectClause"/> or Group clause mapped to <see cref="GroupClause"/> depending to liqn query.</param>
    public QueryModel (Type resultType, MainFromClause mainFromClause, ISelectGroupClause selectOrGroupClause)
    {
      ArgumentUtility.CheckNotNull ("resultType", resultType);
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);

      _uniqueIdentifierGenerator = new QueryModelUniqueIdentifierGenerator (this);

      ResultType = resultType;
      MainFromClause = mainFromClause;
      SelectOrGroupClause = selectOrGroupClause;
    }

    public Type ResultType { get; private set; }
    public QueryModel ParentQuery { get; private set; }

    /// <summary>
    /// Set parent <see cref="QueryModel"/> e.g. needed in a subquery.
    /// </summary>
    /// <param name="parentQuery">The parent <see cref="QueryModel"/> of another <see cref="QueryModel"/></param>
    public void SetParentQuery(QueryModel parentQuery)
    {
      ArgumentUtility.CheckNotNull ("parentQueryExpression", parentQuery);
      if (ParentQuery != null && ParentQuery != parentQuery)
        throw new InvalidOperationException ("The query already has a parent query.");

      ParentQuery = parentQuery;
    }

    public MainFromClause MainFromClause { get; private set; }
    
    public ISelectGroupClause SelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
      set 
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _selectOrGroupClause = value;
        InvalidateExpressionTree ();
      }
    }

    /// <summary>
    /// Collection of different clauses of a <see cref="QueryModel"/>
    /// </summary>
    public ReadOnlyCollection<IBodyClause> BodyClauses
    {
      get { return _bodyClauses.AsReadOnly (); }
    }

    /// <summary>
    /// Method to add <see cref="IBodyClause"/> to a <see cref="QueryModel"/>
    /// </summary>
    /// <param name="clause"><see cref="IBodyClause"/></param>
    public void AddBodyClause (IBodyClause clause)
    {
      ArgumentUtility.CheckNotNull ("clause", clause);
      var clauseAsFromClause = clause as FromClauseBase;
      if (clauseAsFromClause != null)
        RegisterClause(clauseAsFromClause.Identifier, clauseAsFromClause);

      clause.SetQueryModel (this);
      _bodyClauses.Add (clause);

      InvalidateExpressionTree ();
    }

    /// <summary>
    /// Method to register clauses via <see cref="ParameterExpression"/> 
    /// </summary>
    /// <param name="identifier"><see cref="ParameterExpression"/></param>
    /// <param name="clauseToBeRegistered"><see cref="IResolveableClause"/></param>
    private void RegisterClause (ParameterExpression identifier, IResolveableClause clauseToBeRegistered)
    {
      if (MainFromClause.Identifier.Name == identifier.Name || _clausesByIdentifier.ContainsKey (identifier.Name))
      {
        string message = string.Format ("Multiple clauses with the same identifier name ('{0}') are not supported.",
            identifier.Name);
        throw new InvalidOperationException (message);
      }
      _clausesByIdentifier.Add (identifier.Name, clauseToBeRegistered);
    }

    /// <summary>
    /// Method to get <see cref="IResolveableClause"/> by identifier.
    /// </summary>
    /// <param name="identifierName">The name of the identifier.</param>
    /// <param name="identifierType">The type of the identifier.</param>
    /// <returns>The <see cref="IResolveableClause"/> for the given identifier.</returns>
    public IResolveableClause GetResolveableClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      IResolveableClause clause = GetResolveableClauseWithoutTypeCheck(identifierName);
      if (clause != null)
        CheckResolvedIdentifierType (clause.Identifier, identifierType);
      return clause;
    }

    /// <summary>
    /// Method to get <see cref="IResolveableClause"/> by identifier name, without considering the type of the identifier.
    /// </summary>
    /// <param name="identifierName">The name of the identifier.</param>
    /// <returns>The <see cref="IResolveableClause"/> for the given identifier name.</returns>
    public IResolveableClause GetResolveableClauseWithoutTypeCheck (string identifierName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("identifierName", identifierName);

      if (identifierName == MainFromClause.Identifier.Name)
        return MainFromClause;
      else
        return GetBodyClauseByIdentifier (identifierName);
    }

    private IResolveableClause GetBodyClauseByIdentifier (string identifierName)
    {
      IResolveableClause clause;
      _clausesByIdentifier.TryGetValue (identifierName, out clause);
      return clause;
    }

    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      visitor.VisitQueryModel (this);
    }

    public override string ToString ()
    {
      var sv = new StringBuildingQueryVisitor();
      sv.VisitQueryModel (this);
      return sv.ToString();
    }

    // Once we have a working ExpressionTreeBuildingVisitor, we could use it to build trees for constructed models. For now, we just create
    // a special ConstructedQueryExpression node.
    // TODO 1067: Changes made to the query model's clauses should cause this Expression to become invalid.
    public Expression GetExpressionTree ()
    {
      if (_expressionTree == null)
        _expressionTree = new ConstructedQueryExpression (this);
      return _expressionTree;
    }

    /// <summary>
    /// Set expression tree whenever the query model changes.
    /// </summary>
    /// <param name="expressionTree">The expression of a linq query.</param>
    public void SetExpressionTree (Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("expressionTree", expressionTree);
      if (_expressionTree != null && _expressionTree != expressionTree)
        throw new InvalidOperationException  ("This QueryModel already has an expression tree. Call InvalidateExpressionTree before setting a new one.");

      _expressionTree = expressionTree;
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);
      
      return new QueryModelFieldResolver (this).ResolveField (resolver, fieldAccessExpression, joinedTableContext);
    }

    public QueryModel Clone ()
    {
      return Clone (new ClonedClauseMapping());
    }

    public QueryModel Clone (ClonedClauseMapping clonedClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("clonedClauseMapping", clonedClauseMapping);

      var cloneContext = new CloneContext (clonedClauseMapping, new SubQueryRegistry());
      var queryModelBuilder = new QueryModelBuilder();

      queryModelBuilder.AddClause (MainFromClause.Clone (cloneContext));
      foreach (var bodyClause in BodyClauses)
        queryModelBuilder.AddClause (bodyClause.Clone (cloneContext));
      queryModelBuilder.AddClause (SelectOrGroupClause.Clone (cloneContext));

      var queryModel = queryModelBuilder.Build (ResultType);
      
      cloneContext.SubQueryRegistry.UpdateAllParentQueries (queryModel);

      if (_expressionTree != null)
        queryModel.SetExpressionTree (_expressionTree);

      return queryModel;
    }

    object ICloneable.Clone ()
    {
      return Clone();
    }

    private void CheckResolvedIdentifierType (ParameterExpression identifier,Type expectedType)
    {
      if (identifier.Type != expectedType)
      {
        string message = string.Format ("The from clause with identifier '{0}' has type '{1}', but '{2}' was requested.", identifier.Name,
            identifier.Type, expectedType);
        throw new ClauseLookupException (message);
      }
    }

    public void InvalidateExpressionTree ()
    {
      _expressionTree = null;
    }

    public string GetUniqueIdentifier (string prefix)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("prefix", prefix);
      return _uniqueIdentifierGenerator.GetUniqueIdentifier (prefix);
    }
  }
}
