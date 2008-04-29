using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Column : ICriterion
  {
    public readonly string Name;
    public readonly IColumnSource _columnSource;

    public Column (IColumnSource columnSource, string name)
    {
      ArgumentUtility.CheckNotNull ("fromSource", columnSource);
      Name = name;
      _columnSource = columnSource;
    }

    public override string ToString ()
    {
      return (_columnSource != null ? _columnSource.AliasString : "<null>") + "." + Name;
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitColumn (this);
    }
  }
}