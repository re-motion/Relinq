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
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct Column : ICriterion
  {
    private readonly IColumnSource _columnSource;
    // If Name is null, the column represents access to the whole ColumnSource. For tables, this would be the whole table; for let clauses either a
    // table, a column, or a computed value.
    private readonly string _name;

    public Column (IColumnSource columnSource, string name)
    {
      ArgumentUtility.CheckNotNull ("fromSource", columnSource);
      _name = name;
      _columnSource = columnSource;
    }

    public IColumnSource ColumnSource
    {
      get { return _columnSource; }
    }

    public string Name
    {
      get { return _name; }
    }

    public override string ToString ()
    {
      return (_columnSource != null ? _columnSource.AliasString : "<null>") + "." + _name;
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitColumn (this);
    }
  }
}
