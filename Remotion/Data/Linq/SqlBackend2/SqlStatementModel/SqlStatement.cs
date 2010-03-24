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
    private readonly SqlTableBase[] _sqlTables;
    private readonly Ordering[] _orderings;

    private Expression _selectProjection;
    private Expression _whereCondition;
    
    public SqlStatement (Expression selectProjection, IEnumerable<SqlTableBase> sqlTables, IEnumerable<Ordering> orderings)
    {
      ArgumentUtility.CheckNotNull ("selectProjection", selectProjection);
      ArgumentUtility.CheckNotNull ("sqlTables", sqlTables);
      ArgumentUtility.CheckNotNull ("orderings", orderings);
      ArgumentUtility.CheckNotEmpty ("sqlTables", sqlTables);

      _selectProjection = selectProjection;
      _sqlTables = sqlTables.ToArray();
      _orderings = orderings.ToArray();
    }

    public bool IsCountQuery { get; set; }
    public bool IsDistinctQuery { get; set; }
    
    public Expression TopExpression { get; set;}
    
    public Expression SelectProjection
    {
      get { return _selectProjection; }
      set { _selectProjection = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public ReadOnlyCollection<SqlTableBase> SqlTables
    {
      get { return Array.AsReadOnly (_sqlTables); }
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

    public ReadOnlyCollection<Ordering> Orderings
    {
      get { return Array.AsReadOnly(_orderings); }
    }
  }
}