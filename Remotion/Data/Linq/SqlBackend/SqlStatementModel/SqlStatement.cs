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
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using Remotion.Data.Linq.Utilities;
using System.Collections.Generic;
using System.Linq;
using Remotion.Data.Linq.Clauses;

namespace Remotion.Data.Linq.SqlBackend.SqlStatementModel
{
  /// <summary>
  /// <see cref="SqlStatement"/> represents a SQL database query. The <see cref="QueryModel"/> is translated to this model, and the 
  /// <see cref="SqlStatement"/> is transformed several times until it can easily be translated to SQL text.
  /// </summary>
  public class SqlStatement
  {
    private readonly SqlTable[] _fromExpressions; // TODO Review: Rename to _sqlTables

    private Expression _selectProjection;
    private Expression _whereCondition;
    private List<Ordering> _orderByClauses; // TODO Review 2401: Store as Ordering[], initialize via ctor as an IEnumerable<Ordering> (similar to _fromExpressions)

    public SqlStatement (Expression selectProjection, IEnumerable<SqlTable> fromExpressions) // TODO Review: Rename to sqlTables
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);
      ArgumentUtility.CheckNotNull ("fromExpressions", fromExpressions);

      _selectProjection = selectProjection;
      _fromExpressions = fromExpressions.ToArray();
      _orderByClauses = new List<Ordering>();
    }

    public bool IsCountQuery { get; set; }
    public bool IsDistinctQuery { get; set; }
    
    public Expression TopExpression { get; set;}
    
    public Expression SelectProjection
    {
      get { return _selectProjection; }
      set { _selectProjection = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public ReadOnlyCollection<SqlTable> FromExpressions // TODO Review: Rename to SqlTables
    {
      get { return Array.AsReadOnly (_fromExpressions); }
    }

    public Expression WhereCondition
    {
      get { return _whereCondition; }
      set 
      {
        if (value != null && value.Type != typeof (bool))
          throw new ArgumentTypeException ("whereCondition", typeof (bool), value.Type);

        _whereCondition = value;
      }
    }

    // TODO Review 2401: Remove setter, rename to "Orderings", expose as ReadOnlyCollection<Ordering> (more symmetric to FromExpressions).
    public List<Ordering> OrderByClauses
    {
      get { return _orderByClauses; }
      set {
        ArgumentUtility.CheckNotNull ("value", value);
        _orderByClauses = value; 
      }
    }
  }
}