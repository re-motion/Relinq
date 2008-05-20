using System.Collections.Generic;
using System.Collections.ObjectModel;
using Remotion.Text;
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct FieldSourcePath
  {
    public IColumnSource FirstSource { get; private set; }
    public ReadOnlyCollection<SingleJoin> Joins { get; private set; }

    public FieldSourcePath(IColumnSource firstSource,IEnumerable<SingleJoin> joins) : this()
    {
      ArgumentUtility.CheckNotNull ("firstSource", firstSource);
      ArgumentUtility.CheckNotNull ("joins", joins);

      FirstSource = firstSource;
      Joins = new List<SingleJoin>(joins).AsReadOnly();
    }

    public IColumnSource LastSource
    {
      get
      {
        if (Joins.Count == 0)
          return FirstSource;
        else
          return Joins[Joins.Count - 1].RightSide;
      }
    }

    public override bool Equals (object obj)
    {
      if (!(obj is FieldSourcePath))
        return false;

      FieldSourcePath other = (FieldSourcePath) obj;
      return object.ReferenceEquals (FirstSource, other.FirstSource) && JoinsEqual (Joins, other.Joins);
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
      return FirstSource.GetHashCode() ^ EqualityUtility.GetRotatedHashCode (Joins);
    }

    public override string ToString ()
    {
      bool joinsHasElements = Joins.GetEnumerator ().MoveNext ();
      if (joinsHasElements)
        return FirstSource.AliasString + "." + SeparatedStringBuilder.Build (".", Joins, join => ((Table)join.RightSide).Name);
      else
        return FirstSource.AliasString;
    }
  }
}