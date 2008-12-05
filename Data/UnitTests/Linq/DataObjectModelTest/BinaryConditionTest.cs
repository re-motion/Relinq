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
using NUnit.Framework;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Data.UnitTests.Linq.DataObjectModelTest
{
  [TestFixture]
  public class BinaryConditionTest
  {
    [Test]
    public void ContainsWithSubQuery ()
    {
      SubQuery subQuery = new SubQuery (ExpressionHelper.CreateQueryModel (), ParseMode.SubQueryInSelect, null);
      BinaryCondition binaryCondition = new BinaryCondition(subQuery, new Constant(0), BinaryCondition.ConditionKind.Contains);
      Assert.AreSame (subQuery, binaryCondition.Left);
    }

    //[Test]
    //[ExpectedException (typeof (ArgumentTypeException), ExpectedMessage = "Argument left has type Remotion.Data.Linq.DataObjectModel.Constant when " 
    //    + "type Remotion.Data.Linq.DataObjectModel.SubQuery was expected.\r\nParameter name: left")]
    //public void ContainsWithInvalidLeftSide ()
    //{
    //  new BinaryCondition (new Constant (0), new Constant (0), BinaryCondition.ConditionKind.Contains);
    //}
  }
}
