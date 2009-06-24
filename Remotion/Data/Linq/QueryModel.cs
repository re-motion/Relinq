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
using Remotion.Data.Linq.Clauses;
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
    private readonly UniqueIdentifierGenerator _uniqueIdentifierGenerator;

    private MainFromClause _mainFromClause;
    private ISelectGroupClause _selectOrGroupClause;

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

      _uniqueIdentifierGenerator = new UniqueIdentifierGenerator ();
      ResultType = resultType;
      MainFromClause = mainFromClause;
      SelectOrGroupClause = selectOrGroupClause;
    }

    public Type ResultType { get; private set; }
    
    public MainFromClause MainFromClause
    {
      get { return _mainFromClause; }
      private set
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _mainFromClause = value;
        _uniqueIdentifierGenerator.AddKnownIdentifier (value.ItemName);
      }
    }

    public ISelectGroupClause SelectOrGroupClause
    {
      get { return _selectOrGroupClause; }
      set 
      {
        ArgumentUtility.CheckNotNull ("value", value);
        _selectOrGroupClause = value;
      }
    }

    /// <summary>
    /// Collection of different clauses of a <see cref="QueryModel"/>
    /// </summary>
    public IList<IBodyClause> BodyClauses
    {
      get { return _bodyClauses; }
    }

    // TODO:       var clauseAsFromClause = clause as FromClauseBase;
    //     if (clauseAsFromClause != null)
    //     _uniqueIdentifierGenerator.AddKnownIdentifier (clauseAsFromClause.ItemName);

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

    public QueryModel Clone ()
    {
      return Clone (new ClonedClauseMapping());
    }

    public QueryModel Clone (ClonedClauseMapping clonedClauseMapping)
    {
      ArgumentUtility.CheckNotNull ("clonedClauseMapping", clonedClauseMapping);

      var cloneContext = new CloneContext (clonedClauseMapping);
      var queryModelBuilder = new QueryModelBuilder();

      queryModelBuilder.AddClause (MainFromClause.Clone (cloneContext));
      foreach (var bodyClause in BodyClauses)
        queryModelBuilder.AddClause (bodyClause.Clone (cloneContext));
      queryModelBuilder.AddClause (SelectOrGroupClause.Clone (cloneContext));

      return queryModelBuilder.Build (ResultType);
    }

    object ICloneable.Clone ()
    {
      return Clone();
    }

    /// <summary>
    /// Returns a new name with the given prefix. The name is different from that of any <see cref="FromClauseBase"/> added
    /// in the <see cref="QueryModel"/>. Note that clause names that are changed after the clause is added as well as names of other clauses
    /// than from clauses are not considered when determining "unique" names. Use names only for readability and debugging, not
    /// for programmatically identifying clauses.
    /// </summary>
    public string GetNewName (string prefix)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("prefix", prefix);
      return _uniqueIdentifierGenerator.GetUniqueIdentifier (prefix);
    }
  }
}
