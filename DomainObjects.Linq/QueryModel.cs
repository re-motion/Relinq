using System;
using System.Collections.Generic;
using Rubicon.Utilities;

namespace Rubicon.Data.DomainObjects.Linq
{
  public class QueryModel
  {
    private readonly Query _from;
    private readonly List<SelectedColumn> _queryColumns = new List<SelectedColumn> ();

    public QueryModel (Query from)
    {
      ArgumentUtility.CheckNotNull ("from", from);
      _from = from;
    }

    public Query From
    {
      get { return _from; }
    }

    public IEnumerable<SelectedColumn> SelectedColumns
    {
      get { return _queryColumns; }
    }

    public void AddSelectedColumn (string columnName)
    {
      ArgumentUtility.CheckNotNullOrEmpty ("columnName", columnName);
      AddSelectedColumn (columnName, null);
    }

    public void AddSelectedColumn (string columnName, string alias)
    {
      ArgumentUtility.CheckNotNull ("columnName", columnName);
      _queryColumns.Add (new SelectedColumn (columnName, alias));
    }
  }
}
