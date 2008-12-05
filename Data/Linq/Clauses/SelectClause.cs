// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
//
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// version 3.0 as published by the Free Software Foundation.
//
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
//
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
//
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
