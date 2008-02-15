using System;
using System.Collections.Generic;
using System.Reflection;
using Rubicon.Collections;
using Rubicon.Data.Linq.DataObjectModel;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.Parsing.FieldResolving
{
  public class JoinedTableContext
  {
    private readonly Dictionary<Tuple<IFieldSourcePath, MemberInfo>, Table> _tables = new Dictionary<Tuple<IFieldSourcePath, MemberInfo>, Table>();

    public Table GetJoinedTable (IDatabaseInfo databaseInfo, IFieldSourcePath fieldSourcePath, MemberInfo relationMember)
    {
      ArgumentUtility.CheckNotNull ("databaseInfo", databaseInfo);
      ArgumentUtility.CheckNotNull ("fieldSourcePath", fieldSourcePath);
      ArgumentUtility.CheckNotNull ("relationMember", relationMember);

      Tuple<IFieldSourcePath, MemberInfo> key = Tuple.NewTuple (fieldSourcePath, relationMember);

      if (!_tables.ContainsKey (key))
        _tables.Add (key,DatabaseInfoUtility.GetRelatedTable (databaseInfo, relationMember));
      return _tables[key];
    }
  }
}