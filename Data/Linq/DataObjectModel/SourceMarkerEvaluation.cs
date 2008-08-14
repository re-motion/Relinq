namespace Remotion.Data.Linq.DataObjectModel
{
  public class SourceMarkerEvaluation : IEvaluation
  {
    public static readonly SourceMarkerEvaluation Instance = new SourceMarkerEvaluation();

    private SourceMarkerEvaluation ()
    {
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      visitor.VisitSourceMarkerEvaluation (this);
    }
  }
}