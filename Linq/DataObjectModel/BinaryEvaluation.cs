using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public struct BinaryEvaluation : IEvaluation
  {
    public enum EvaluationKind { Add, Divide, Modulo, Multiply, Subtract }

    public BinaryEvaluation (IEvaluation left, IEvaluation right, EvaluationKind kind) : this()
    {
      ArgumentUtility.CheckNotNull ("left", left);
      ArgumentUtility.CheckNotNull ("right", right);
      ArgumentUtility.CheckNotNull ("kind", kind);

      Left = left;
      Right = right;
      Kind = kind;
    }

    public IEvaluation Left { get; private set; }
    public IEvaluation Right { get; private set; }
    public EvaluationKind Kind { get; private set; }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitBinaryEvaluation (this);
    }
  }
}