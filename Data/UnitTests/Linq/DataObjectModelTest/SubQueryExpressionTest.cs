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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Expressions;

namespace Remotion.Data.UnitTests.Linq.DataObjectModelTest
{
  [TestFixture]
  public class SubQueryExpressionTest
  {
    [Test]
    public void Initialize_DontOverwriteExpressionType ()
    {
      Assert.IsFalse(Enum.IsDefined (typeof (ExpressionType), new SubQueryExpression (ExpressionHelper.CreateQueryModel()).NodeType));
    }

    [Test]
    public void Initialize_CorrectType ()
    {
      QueryModel model = new QueryModel (typeof (int), ExpressionHelper.CreateMainFromClause(), ExpressionHelper.CreateSelectClause());
      SubQueryExpression sqe = new SubQueryExpression (model);

      Assert.AreEqual (typeof (int), sqe.Type);
    }


  }
}
