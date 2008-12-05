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
using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class FromLetWhereExpressionTest
  {
    [Test]
    public void FromExpression_Initialize()
    {
      Expression expression = ExpressionHelper.CreateExpression();
      ParameterExpression identifier = ExpressionHelper.CreateParameterExpression();
      FromExpressionData fromExpressionData = new FromExpressionData(expression,identifier);
      Assert.AreSame (expression, fromExpressionData.TypedExpression);
      Assert.AreSame (identifier, fromExpressionData.Identifier);
    }

    [Test]
    public void WhereExpression_Initialize ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      WhereExpressionData whereExpressionData = new WhereExpressionData (expression);
      Assert.AreSame (expression, whereExpressionData.TypedExpression);

    }
    
  }
}
