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
  // LetColumnSource
  public struct LetColumnSource : IColumnSource
  {
    public LetColumnSource (string alias, bool isTable) : this()
    {
      ArgumentUtility.CheckNotNull ("alias", alias);
      ArgumentUtility.CheckNotNull ("isTable", isTable);
      Alias = alias;
      IsTable = isTable;
    }

    public bool IsTable { get; private set; }

    public string Alias {get; private set; }
    public string AliasString { get { return Alias; }
    }
  }
}
