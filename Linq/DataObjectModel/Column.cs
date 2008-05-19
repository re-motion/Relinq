using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Column : ICriterion
  {
    public readonly string Name;
    public readonly IColumnSource ColumnSource;

    public Column (IColumnSource columnSource, string name)
    {
      ArgumentUtility.CheckNotNull ("fromSource", columnSource);
      Name = name;
      ColumnSource = columnSource;
    }

    public override string ToString ()
    {
      return (ColumnSource != null ? ColumnSource.AliasString : "<null>") + "." + Name;
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitColumn (this);
    }
  }
}