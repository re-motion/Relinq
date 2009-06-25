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
using System.Linq.Expressions;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel.TestDomain
{
  public class ExpressionNodeWithTooManyCtors : IExpressionNode
  {
    public ExpressionNodeWithTooManyCtors ()
    {
    }

// ReSharper disable UnusedParameter.Local
    public ExpressionNodeWithTooManyCtors (int i)
// ReSharper restore UnusedParameter.Local
    {
    }

    public IExpressionNode Source
    {
      get { throw new NotImplementedException(); }
    }

    public Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public IClause CreateClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public SelectClause CreateSelectClause (IClause previousClause, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }

    public void Apply (QueryModel queryModel, ClauseGenerationContext clauseGenerationContext)
    {
      throw new NotImplementedException();
    }
  }
}