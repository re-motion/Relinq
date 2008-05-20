using System.Collections.Specialized;
using System.Reflection;
using Remotion.Collections;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.FieldResolving
{
  public class JoinedTableContext
  {
    private readonly OrderedDictionary _tables = new OrderedDictionary ();
    private int _generatedAliasCount = 0;

    public int Count
    {
      get { return _tables.Count; }
    }

    public Table GetJoinedTable (IDatabaseInfo databaseInfo, FieldSourcePath fieldSourcePath, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldSourcePath", fieldSourcePath);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<FieldSourcePath, MemberInfo> key = Tuple.NewTuple (fieldSourcePath, relationMember);

      if (!_tables.Contains (key))
        _tables.Add (key, DatabaseInfoUtility.GetRelatedTable (databaseInfo, relationMember));
      return (Table) _tables[key];
    }

    public void CreateAliases ()
    {
      for (int i = 0; i < Count; ++i)
      {
        Table table = (Table) _tables[i];
        if (table.Alias == null)
        {
          table.SetAlias ("j" + _generatedAliasCount);
          ++_generatedAliasCount;
        }
      }
    }
  }
}