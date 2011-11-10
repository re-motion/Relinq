// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  public static class ExpressionNodeObjectMother
  {
    public static MainSourceExpressionNode CreateMainSource ()
    {
      return new MainSourceExpressionNode ("x", Expression.Constant (new[] { 1, 2, 3 }));
    }

    public static SelectManyExpressionNode CreateSelectMany (IExpressionNode source)
    {
      var p1 = Expression.Parameter (typeof (Cook), "s");
      var p2 = Expression.Parameter (typeof (Kitchen), "sd");
      var resultSelector = Expression.Lambda (Expression.Constant (null), p1, p2);
      var collectionSelector = Expression.Lambda (Expression.Constant (null), p1);

      var parseInfo = new MethodCallExpressionParseInfo ("trans", source, ExpressionHelper.CreateMethodCallExpression ());
      return new SelectManyExpressionNode (parseInfo, collectionSelector, resultSelector);
    }

    //public static SelectManyExpressionNode CreateSelectMany (FromClauseBase source)
    //{
    //  var p1 = Expression.Parameter (typeof (Cook), "s");
    //  var p2 = Expression.Parameter (typeof (MainKitchen), "sd");
    //  var resultSelector = Expression.Lambda (Expression.Constant (null), p1, p2);
    //  var collectionSelector = Expression.Lambda (Expression.Constant (null), p1);

    //  var parseInfo = new MethodCallExpressionParseInfo ("trans", source, ExpressionHelper.CreateMethodCallExpression ());
    //  return new SelectManyExpressionNode (parseInfo, collectionSelector, resultSelector);
    //}
  }
}
