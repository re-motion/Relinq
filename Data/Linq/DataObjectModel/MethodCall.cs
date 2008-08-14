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
using Remotion.Text;
using Remotion.Utilities;
using System.Linq;

namespace Remotion.Data.Linq.DataObjectModel
{
  public class MethodCall : IEvaluation,ICriterion
  {
    public MethodCall (MethodInfo evaluationMethodInfo, IEvaluation evaluationObject, List<IEvaluation> evaluationArguments)
    {
      ArgumentUtility.CheckNotNull ("evaluationMethodInfo", evaluationMethodInfo);
      ArgumentUtility.CheckNotNull ("evaluationArguments", evaluationArguments);

      EvaluationMethodInfo = evaluationMethodInfo;
      EvaluationObject = evaluationObject;
      EvaluationArguments = evaluationArguments;
    }

    public MethodInfo EvaluationMethodInfo { get; private set; }
    public IEvaluation EvaluationObject { get; private set; }
    public List<IEvaluation> EvaluationArguments { get; private set; }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitMethodCall (this);
    }

    public override string ToString ()
    {
      string argumentString = SeparatedStringBuilder.Build (", ", EvaluationArguments);
      if (EvaluationObject != null)
        return string.Format ("{0}.{1}({2})", EvaluationObject, EvaluationMethodInfo.Name, argumentString);
      else
        return string.Format ("{0}({1})", EvaluationMethodInfo.Name, argumentString);
    }

    public override bool Equals (object obj)
    {
      MethodCall other = obj as MethodCall;
      return other != null 
          && Equals (EvaluationMethodInfo, other.EvaluationMethodInfo) 
          && Equals (EvaluationObject, other.EvaluationObject)
          && EvaluationArguments.SequenceEqual (other.EvaluationArguments);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (EvaluationMethodInfo, EvaluationObject, EqualityUtility.GetRotatedHashCode (EvaluationArguments));
    }
  }
}
