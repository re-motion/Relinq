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
using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class OrderExpressionData : BodyExpressionDataBase<LambdaExpression>
  {
    public OrderExpressionData (bool firstOrderBy, OrderingDirection orderingDirection, LambdaExpression expression)
        : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      FirstOrderBy = firstOrderBy;
      OrderingDirection = orderingDirection;
    }

    public bool FirstOrderBy { get; private set; }
    public OrderingDirection OrderingDirection { get; private set; }

    public override string ToString ()
    {
      if (FirstOrderBy)
        return string.Format ("orderby {0} {1}", TypedExpression, OrderingDirection);
      else
        return string.Format ("thenby {0} {1}", TypedExpression, OrderingDirection);
    }
  }
}
