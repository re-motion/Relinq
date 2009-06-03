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
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  public class ExpressionNodeWithTooManyCtors : IExpressionNode
  {
    public ExpressionNodeWithTooManyCtors ()
    {
    }

    public ExpressionNodeWithTooManyCtors (int i)
    {
    }

    public IExpressionNode Source
    {
      get { throw new NotImplementedException(); }
    }

    public Expression Resolve (ParameterExpression inputParameter, Expression expressionToBeResolved)
    {
      throw new NotImplementedException();
    }

    public ParameterExpression CreateParameterForOutput ()
    {
      throw new NotImplementedException();
    }

    public IClause CreateClause (IClause previousClause)
    {
      throw new NotImplementedException();
    }
  }
}