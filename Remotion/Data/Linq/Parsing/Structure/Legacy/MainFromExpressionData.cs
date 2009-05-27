// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using System.Linq.Expressions;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Structure.Legacy
{
  public class MainFromExpressionData : BodyExpressionDataBase<Expression>
  {
    public MainFromExpressionData (Expression expression)
        : base(expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

    }
  }
}