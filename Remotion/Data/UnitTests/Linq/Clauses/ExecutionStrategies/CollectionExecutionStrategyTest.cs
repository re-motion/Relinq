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
using System;
using System.Collections.Generic;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Remotion.Data.Linq.EagerFetching;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses.ExecutionStrategies
{
  [TestFixture]
  public class CollectionExecutionStrategyTest
  {
    [Test]
    public void GetExecutionExpression ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel();
      var fetchRequests = new FetchRequestBase[0];

      var lambda = CollectionExecutionStrategy.Instance.GetExecutionExpression<IEnumerable<int>> (queryModel, fetchRequests);

      var executorMock = MockRepository.GenerateMock<IQueryExecutor>();
      executorMock.Expect (mock => mock.ExecuteCollection2<int> (queryModel, fetchRequests)).Return (new[] { 1, 2, 3 });
      IEnumerable<int> result = lambda.Compile() (executorMock);

      Assert.That (result, Is.EqualTo (new[] { 1, 2, 3 }));
      executorMock.VerifyAllExpectations();
    }
  }
}