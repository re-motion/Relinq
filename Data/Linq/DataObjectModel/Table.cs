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
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public class Table : IColumnSource
  {
    public Table ()
    {
    }

    public Table(string name, string alias)
    {
      ArgumentUtility.CheckNotNull ("name", name);
      Name = name;
      Alias = alias;
    }

    public string Name { get; private set; }
    public string Alias { get; private set; }

    public override string ToString ()
    {
      return Name + " " + AliasString;
    }

    public string AliasString
    {
      get { return Alias ?? "_"; }
    }

    public bool IsTable
    {
      get { return true; }
    }

    public override bool Equals (object obj)
    {
      Table other = obj as Table;
      return other != null && other.Name == Name && other.Alias == Alias;
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (Name, Alias);
    }

    public void SetAlias (string newAlias)
    {
      ArgumentUtility.CheckNotNull ("newAlias", newAlias);

      if (Alias != null)
      {
        string message = string.Format ("Table '{0}' already has alias '{1}', new alias '{2}' cannot be set.", Name, Alias, newAlias);
        throw new InvalidOperationException (message);
      }
      Alias = newAlias;
    }
  }
}
