namespace Remotion.Data.Linq.DataObjectModel
{
  public class SourceMarkerEvaluation : IEvaluation
  {
    public void Accept (IEvaluationVisitor visitor)
    {
      visitor.VisitSourceMarkerEvaluation (this);
    }
  }
}