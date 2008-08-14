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
using System.Collections.ObjectModel;
using Remotion.Data.Linq.Clauses;
using Remotion.Utilities;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Clauses
{
  public class SelectClause : ISelectGroupClause
  {
    private readonly LambdaExpression _projectionExpression;
    private List<ResultModifierClause> _resultModifierData = new List<ResultModifierClause>();
    
    //delete after change
    public SelectClause (IClause previousClause, LambdaExpression projectionExpression, List<MethodCallExpression> resultModifiers)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      
      PreviousClause = previousClause;
      _projectionExpression = projectionExpression;
      ResultModifiers = resultModifiers;
    }

    public SelectClause (IClause previousClause, LambdaExpression projectionExpression)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);

      PreviousClause = previousClause;
      _projectionExpression = projectionExpression;
    }
    
    public IClause PreviousClause { get; private set; }

    public LambdaExpression ProjectionExpression
    {
      get { return _projectionExpression; }
    }

    public List<MethodCallExpression> ResultModifiers { get; private set; }

    public ReadOnlyCollection<ResultModifierClause> ResultModifierData
    {
      get { return _resultModifierData.AsReadOnly(); }
    }

    public void AddResultModifierData (ResultModifierClause resultModifierData)
    {
      _resultModifierData.Add (resultModifierData);
      if (ResultModifiers == null)
        ResultModifiers = new List<MethodCallExpression>();
      ResultModifiers.Add (resultModifierData.ResultModifier);
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSelectClause (this);
    }
  }
}
