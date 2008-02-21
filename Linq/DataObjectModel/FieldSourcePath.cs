using System.Collections.Generic;
using System.Collections.ObjectModel;
using Rubicon.Text;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct FieldSourcePath
  {
    public Table SourceTable { get; private set; }
    public ReadOnlyCollection<SingleJoin> Joins { get; private set; }

    public FieldSourcePath(Table sourceTable,IEnumerable<SingleJoin> joins) : this()
    {
      ArgumentUtility.CheckNotNull ("sourceTable", sourceTable);
      ArgumentUtility.CheckNotNull ("joins", joins);

      SourceTable = sourceTable;
      Joins = new List<SingleJoin>(joins).AsReadOnly();
    }

    public Table LastTable
    {
      get
      {
        if (Joins.Count == 0)
          return SourceTable;
        else
          return Joins[Joins.Count - 1].RightSide;
      }
    }

    public override bool Equals (object obj)
    {
      if (!(obj is FieldSourcePath))
        return false;

      FieldSourcePath other = (FieldSourcePath) obj;
      return object.ReferenceEquals (SourceTable, other.SourceTable) && JoinsEqual (Joins, other.Joins);
    }

    private bool JoinsEqual (ReadOnlyCollection<SingleJoin> joins1, ReadOnlyCollection<SingleJoin> joins2)
    {
      if (joins1.Count != joins2.Count)
        return false;

      for (int i = 0; i < joins1.Count; ++i)
      {
        if (!joins1[i].Equals (joins2[i]))
          return false;
      }
      return true;
    }

    public override int GetHashCode ()
    {
      return SourceTable.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (Joins);
    }

    public override string ToString ()
    {
      bool joinsHasElements = Joins.GetEnumerator ().MoveNext ();
      if (joinsHasElements)
        return SourceTable.AliasString + "." + SeparatedStringBuilder.Build (".", Joins, join => join.RightSide.Name);
      else
        return SourceTable.AliasString;
    }
  }
}