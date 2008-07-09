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
using System.Linq.Expressions;
using Remotion.Data.Linq.Expressions;

namespace Remotion.Data.Linq.Parsing.TreeEvaluation
{
  public class PartialEvaluationData
  {
    public PartialEvaluationData ()
    {
      UsedParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
      DeclaredParameters = new Dictionary<Expression, HashSet<ParameterExpression>> ();
      SubQueries = new Dictionary<Expression, HashSet<SubQueryExpression>> ();
    }

    public Dictionary<Expression, HashSet<ParameterExpression>> UsedParameters { get; private set; }
    public Dictionary<Expression, HashSet<ParameterExpression>> DeclaredParameters { get; private set; }
    public Dictionary<Expression, HashSet<SubQueryExpression>> SubQueries { get; private set; }
  }
}
