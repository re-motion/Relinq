// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 
using System;
using System.Linq.Expressions;
using Remotion.Linq.Development.UnitTesting;
using Remotion.Linq.Parsing.Structure.IntermediateModel;
using Remotion.Linq.UnitTests.TestDomain;

namespace Remotion.Linq.UnitTests.Parsing.Structure.IntermediateModel
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

      var parseInfo = new MethodCallExpressionParseInfo ("trans", source, ExpressionHelper.CreateMethodCallExpression<Cook> ());
      return new SelectManyExpressionNode (parseInfo, collectionSelector, resultSelector);
    }

    //public static SelectManyExpressionNode CreateSelectMany (FromClauseBase source)
    //{
    //  var p1 = Expression.Parameter (typeof (Cook), "s");
    //  var p2 = Expression.Parameter (typeof (MainKitchen), "sd");
    //  var resultSelector = Expression.Lambda (Expression.Constant (null), p1, p2);
    //  var collectionSelector = Expression.Lambda (Expression.Constant (null), p1);

    //  var parseInfo = new MethodCallExpressionParseInfo ("trans", source, ExpressionHelper.CreateMethodCallExpression<Cook> ());
    //  return new SelectManyExpressionNode (parseInfo, collectionSelector, resultSelector);
    //}
  }
}
