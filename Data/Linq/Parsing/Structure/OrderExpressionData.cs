/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Linq.Expressions;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure
{
  public class OrderExpressionData : BodyExpressionDataBase<LambdaExpression>
  {
    public OrderExpressionData (bool firstOrderBy, OrderDirection orderDirection, LambdaExpression expression)
        : base (expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      FirstOrderBy = firstOrderBy;
      OrderDirection = orderDirection;
    }

    public bool FirstOrderBy { get; private set; }
    public OrderDirection OrderDirection { get; private set; }

    public override string ToString ()
    {
      if (FirstOrderBy)
        return string.Format ("orderby {0} {1}", Expression, OrderDirection);
      else
        return string.Format ("thenby {0} {1}", Expression, OrderDirection);
    }
  }
}
