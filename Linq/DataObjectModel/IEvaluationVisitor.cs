using System.Collections.Generic;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public interface IEvaluationVisitor
  {
    void VisitBinaryEvaluation (BinaryEvaluation binaryEvaluation);
    void VisitComplexCriterion (ComplexCriterion complexCriterion);
    void VisitNotCriterion (NotCriterion notCriterion);
    void VisitConstant (Constant constant);
    void VisitColumn (Column column);
    void VisitBinaryCondition (BinaryCondition binaryCondition);
    void VisitSubQuery (SubQuery subQuery);
    void VisitMethodCallEvaluation (MethodCallEvaluation methodCallEvaluation);
  }
}