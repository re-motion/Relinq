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
using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Clauses
{
  // TODO MG: Unfinished Refactoring: test
  public class ResultModifierClause : IClause
  {
    public ResultModifierClause (IClause previousClause, SelectClause selectClause, MethodCallExpression resultModifier)
    {
      ArgumentUtility.CheckNotNull ("previousClause", previousClause);
      ArgumentUtility.CheckNotNull ("selectClause", selectClause);
      ArgumentUtility.CheckNotNull ("resultModifier", resultModifier);

      PreviousClause = previousClause;
      SelectClause = selectClause;
      ResultModifier = resultModifier;
    }

    public IClause PreviousClause { get; private set; }
    public SelectClause SelectClause { get; private set; }
    // TODO 1158: Replace with MethodCall
    public MethodCallExpression ResultModifier { get; private set; }


    // TODO MG: Unfinished Refactoring: test, implement, and adapt IQueryVisitor and its implementations
    public void Accept (IQueryVisitor visitor)
    {
      //ArgumentUtility.CheckNotNull ("visitor", visitor);
      //visitor.VisitResultModifierClause (this);
      throw new System.NotImplementedException();
    }

    public ResultModifierClause Clone (IClause newPreviousClause, SelectClause newSelectClause)
    {
      return new ResultModifierClause (newPreviousClause, newSelectClause, ResultModifier);
    }
  }
}
