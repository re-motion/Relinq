using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct Constant : IValue, ICriterion
  {
    public readonly object Value;

    public Constant (object value)
    {
      Value = value;
    }

    public override string ToString ()
    {
      return Value != null ? Value.ToString() : "<null>";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitConstant (this);
    }
  }
}