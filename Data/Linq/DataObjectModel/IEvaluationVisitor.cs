/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

namespace Remotion.Data.Linq.DataObjectModel
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
    void VisitMethodCall (MethodCall methodCall);
    void VisitNewObjectEvaluation (NewObject newObject);
    void VisitSourceMarkerEvaluation (SourceMarkerEvaluation sourceMarkerEvaluation);
  }
}
