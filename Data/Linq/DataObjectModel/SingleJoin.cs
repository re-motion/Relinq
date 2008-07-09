/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
