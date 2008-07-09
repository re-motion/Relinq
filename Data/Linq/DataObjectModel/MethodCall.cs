/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
