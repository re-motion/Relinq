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
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct SingleJoin
  {
    public Column RightColumn { get; private set; }
    public Column LeftColumn { get; private set; }

    public SingleJoin (Column leftColumn, Column rightColumn) : this()
    {
      ArgumentUtility.CheckNotNull ("leftColumn", leftColumn);
      ArgumentUtility.CheckNotNull ("rightColumn", rightColumn);

      LeftColumn = leftColumn;
      RightColumn = rightColumn;
    }

    public IColumnSource LeftSide
    {
      get { return LeftColumn.ColumnSource; }
    }

    public IColumnSource RightSide
    {
      get { return RightColumn.ColumnSource; }
    }

    public override string ToString ()
    {
      return string.Format ("({0} left join {1} on {2} = {3})", RightSide, LeftSide, RightColumn, LeftColumn);
    }
  }
}
