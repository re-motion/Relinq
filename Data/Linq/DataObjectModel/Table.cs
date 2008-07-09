/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
