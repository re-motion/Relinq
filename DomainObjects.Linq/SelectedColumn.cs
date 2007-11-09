using Rubicon.Utilities;
using System;

namespace Rubicon.Data.DomainObjects.Linq
{
  public struct SelectedColumn
  {
    private readonly string _name;
    private readonly string _alias;

    public SelectedColumn (string columnName, string alias)
    {
      ArgumentUtility.CheckNotNull ("columnName", columnName);
      _name = columnName;
      _alias = alias;
    }

    public string Name
    {
      get { return _name; }
    }

    public string Alias
    {
      get { return _alias; }
    }
  }
}