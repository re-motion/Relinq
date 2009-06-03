// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Remotion.Utilities;
using System.Linq.Expressions;

namespace Remotion.Data.Linq.Clauses
{
  /// <summary>
  /// Represents the select part of a linq query.
  /// example: select expression
  /// </summary>
  public class SelectClause : ISelectGroupClause
  {
    private readonly List<ResultModificationBase> _resultModifications = new List<ResultModificationBase> ();

    private IClause _previousClause;
    private LambdaExpression _selector;

    /// <summary>
    /// Initialize a new instance of <see cref="SelectClause"/>.
    /// </summary>
    /// <param name="previousClause">The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.</param>
    /// <param name="selector">The projection within the select part of the linq query.</param>
    public SelectClause (IClause previousClause, LambdaExpression selector)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("selector", selector);

      PreviousClause = previousClause;
      Selector = selector;
    }

    /// <summary>
    /// The previous clause of type <see cref="IClause"/> in the <see cref="QueryModel"/>.
    /// </summary>
    public IClause PreviousClause
    {
      get { return _previousClause; }
      set { _previousClause = ArgumentUtility.CheckNotNull ("value", value); }
    }

    /// <summary>
    /// The projection within the select part of the linq query.
    /// </summary>
    // TODO 1158: Replace by IEvaluation
    public LambdaExpression Selector
    {
      get { return _selector; }
      set { _selector = ArgumentUtility.CheckNotNull ("value", value); }
    }

    public ReadOnlyCollection<ResultModificationBase> ResultModifications
    {
      get { return _resultModifications.AsReadOnly(); }
    }

    public void AddResultModification (ResultModificationBase resultModification)
    {
      ArgumentUtility.CheckNotNull ("resultModification", resultModification);
      _resultModifications.Add (resultModification);
    }

    public virtual void Accept (IQueryVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSelectClause (this);
    }

    public SelectClause Clone (IClause newPreviousClause)
    {
      var clone = new SelectClause (newPreviousClause, Selector);
      foreach (var resultModification in ResultModifications)
      {
        var resultModificationClone = resultModification.Clone (clone);
        clone.AddResultModification (resultModificationClone);
      }

      return clone;
    }

    ISelectGroupClause ISelectGroupClause.Clone (IClause newPreviousClause)
    {
      return Clone (newPreviousClause);
    }
  }
}
