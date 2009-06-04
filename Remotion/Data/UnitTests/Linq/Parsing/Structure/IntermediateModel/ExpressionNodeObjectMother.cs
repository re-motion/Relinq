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
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  public static class ExpressionNodeObjectMother
  {
    public static ConstantExpressionNode CreateConstant ()
    {
      return new ConstantExpressionNode ("x", typeof (int[]), new[] { 1, 2, 3 });
    }

    public static SelectManyExpressionNode CreateSelectMany (IExpressionNode source)
    {
      var p1 = Expression.Parameter (typeof (Student), "s");
      var p2 = Expression.Parameter (typeof (Student_Detail), "sd");
      var resultSelector = Expression.Lambda (Expression.Constant (null), p1, p2);
      var collectionSelector = Expression.Lambda (Expression.Constant (null), p1);
      return new SelectManyExpressionNode ("TODO", source, collectionSelector, resultSelector);
    }
  }
}