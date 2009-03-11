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
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Expressions;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Data.Linq.Visitor;
using Remotion.Utilities;

namespace Remotion.Data.Linq
{
  /// <summary>
  /// The class implements an abstraction of the expression tree which is created when executing a linq query. 
  /// The different parts of a linq query are mapped to clauses.
  /// </summary>
  public class QueryModel : IQueryElement
  {
    private readonly List<IBodyClause> _bodyClauses = new List<IBodyClause> ();

    private readonly Dictionary<string, IResolveableClause> _clausesByIdentifier = new Dictionary<string, IResolveableClause> ();

    private Expression _expressionTree;

    /// <summary>
    /// Initializes a new instance of <see cref="QueryModel"/>
    /// </summary>
    /// <param name="resultType">The type of the result of a executed linq.</param>
    /// <param name="mainFromClause">The first from in a linq query mapped to <see cref="MainFromClause"/></param>
    /// <param name="selectOrGroupClause">The Select mapped to <see cref="SelectClause"/> or Group clause mapped to <see cref="GroupClause"/> depending to liqn query.</param>
    /// <param name="expressionTree">The expression tree of a linq query.</param>
    public QueryModel (Type resultType, MainFromClause mainFromClause, ISelectGroupClause selectOrGroupClause, Expression expressionTree)
    {
      ArgumentUtility.CheckNotNull ("resultType", resultType);
      ArgumentUtility.CheckNotNull ("mainFromClause", mainFromClause);
      ArgumentUtility.CheckNotNull ("SelectOrGroupClause", selectOrGroupClause);

      ResultType = resultType;
      MainFromClause = mainFromClause;
      SelectOrGroupClause = selectOrGroupClause;
      _expressionTree = expressionTree;
    }

    /// <summary>
    /// Initializes a new instance of <see cref="QueryModel"/>
    /// </summary>
    /// <param name="resultType">The type of the result of a executed linq query.</param>
    /// <param name="fromClause">The first from in a linq query mapped to <see cref="MainFromClause"/></param>
    /// <param name="selectOrGroupClause">The Select mapped to <see cref="SelectClause"/> or Group clause mapped to <see cref="GroupClause"/> depending to liqn query.</param>
    public QueryModel (Type resultType, MainFromClause fromClause, ISelectGroupClause selectOrGroupClause)
        : this (resultType, fromClause, selectOrGroupClause, null)
    {
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
      if (ParentQuery != null)
        throw new InvalidOperationException ("The query already has a parent query.");

      ParentQuery = parentQuery;
    }

    public MainFromClause MainFromClause { get; private set; }
    public ISelectGroupClause SelectOrGroupClause { get; private set; }

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

      var clauseAsLetClause = clause as LetClause;
      if (clauseAsLetClause != null)
        RegisterClause (clauseAsLetClause.Identifier, clauseAsLetClause);

      clause.SetQueryModel (this);
      _bodyClauses.Add (clause);
    }

    public IClause GetMainFromClause ()
    {
      return MainFromClause;
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
    /// Method to get <see cref="IResolveableClause"/>
    /// </summary>
    /// <param name="identifierName">The name of the identifier.</param>
    /// <param name="identifierType">The type of the identifier.</param>
    /// <returns>The <see cref="IResolveableClause"/> for the given identifier.</returns>
    public IResolveableClause GetResolveableClause (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      if (identifierName == MainFromClause.Identifier.Name)
      {
        CheckResolvedIdentifierType (MainFromClause.Identifier, identifierType);
        return MainFromClause;
      }
      else
        return GetBodyClauseByIdentifier (identifierName, identifierType);
    }

    private IResolveableClause GetBodyClauseByIdentifier (string identifierName, Type identifierType)
    {
      ArgumentUtility.CheckNotNull ("identifierName", identifierName);
      ArgumentUtility.CheckNotNull ("identifierType", identifierType);

      IResolveableClause clause;
      if (_clausesByIdentifier.TryGetValue (identifierName, out clause))
      {
        CheckResolvedIdentifierType (clause.Identifier,identifierType);
        return clause;
      }
      return null;
    }


    public void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      visitor.VisitQueryModel (this);
    }

    public override string ToString ()
    {
      var sv = new StringVisitor();
      sv.VisitQueryModel (this);
      return sv.ToString();
    }

    public string PrintQueryModel ()
    {
      var qv = new QueryModelVisitor ();
      qv.VisitQueryModel (this);
      return qv.ToString();
    }

    // Once we have a working ExpressionTreeBuildingVisitor, we could use it to build trees for constructed models. For now, we just create
    // a special ConstructedExpression node.
    public Expression GetExpressionTree ()
    {
      if (_expressionTree == null)
        return new ConstructedQueryExpression (this);
      else
        return _expressionTree;
    }

    public FieldDescriptor ResolveField (ClauseFieldResolver resolver, Expression fieldAccessExpression, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      ArgumentUtility.CheckNotNull ("fieldAccessExpression", fieldAccessExpression);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);
      
      return new QueryModelFieldResolver (this).ResolveField (resolver, fieldAccessExpression, joinedTableContext);
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
  }
}
