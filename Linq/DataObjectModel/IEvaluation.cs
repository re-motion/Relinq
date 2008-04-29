using System;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public interface IEvaluation
  {
    void Accept (IEvaluationVisitor visitor);
  }
}