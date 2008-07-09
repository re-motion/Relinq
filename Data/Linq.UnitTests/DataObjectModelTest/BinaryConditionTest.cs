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
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Utilities;

namespace Remotion.Data.Linq.UnitTests.DataObjectModelTest
{
  [TestFixture]
  public class BinaryConditionTest
  {
    [Test]
    public void ContainsWithSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), null);
      BinaryCondition binaryCondition = new BinaryCondition(subQuery, new Constant(0), BinaryCondition.ConditionKind.Contains);
      Assert.AreSame (subQuery, binaryCondition.Left);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument left has type Remotion.Data.Linq.DataObjectModel.Constant when " 
        + "type Remotion.Data.Linq.DataObjectModel.SubQuery was expected.\r\nParameter name: left")]
    public void ContainsWithInvalidLeftSide ()
    {
      new BinaryCondition (new Constant (0), new Constant (0), BinaryCondition.ConditionKind.Contains);
    }
  }
}
