using System.Collections.Generic;
using System.Reflection;
using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public class MethodCall : IEvaluation
  {
    public MethodCall (MethodInfo evaluationMethodInfo, IEvaluation evaluationParameter, List<IEvaluation> evaluationArguments)
    {
      ArgumentUtility.CheckNotNull ("evaluationMethodInfo", evaluationMethodInfo);

      EvaluationMethodInfo = evaluationMethodInfo;
      EvaluationParameter = evaluationParameter;
      EvaluationArguments = evaluationArguments;
    }

    public MethodInfo EvaluationMethodInfo { get; private set; }
    public IEvaluation EvaluationParameter { get; private set; }
    public List<IEvaluation> EvaluationArguments { get; private set; }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMethodCallEvaluation (this);
    }
  }
}