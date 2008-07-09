/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.Linq.UnitTests.ParsingTest.StructureTest
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
      Assert.AreSame (expression, fromExpressionData.Expression);
      Assert.AreSame (identifier, fromExpressionData.Identifier);
    }

    [Test]
    public void WhereExpression_Initialize ()
    {
      LambdaExpression expression = ExpressionHelper.CreateLambdaExpression ();
      WhereExpressionData whereExpressionData = new WhereExpressionData (expression);
      Assert.AreSame (expression, whereExpressionData.Expression);

    }
    
  }
}
