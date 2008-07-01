using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public struct NotCriterion : ICriterion
  {
    public readonly ICriterion NegatedCriterion;

    public NotCriterion (ICriterion negatedCriterion)
    {
      ArgumentUtility.CheckNotNull ("negatedCriterion", negatedCriterion);
      NegatedCriterion = negatedCriterion;
    }


    public override string ToString ()
    {
      return "NOT (" + NegatedCriterion + ")";
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitNotCriterion (this);
    }

  }
}