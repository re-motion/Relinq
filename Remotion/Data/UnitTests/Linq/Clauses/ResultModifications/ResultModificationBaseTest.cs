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
using NUnit.Framework;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Clauses.ExecutionStrategies;
using Rhino.Mocks;

namespace Remotion.Data.UnitTests.Linq.Clauses.ResultModifications
{
  [TestFixture]
  public class ResultModificationBaseTest
  {
    ResultModificationBase _resultModification;

    [SetUp]
    public void SetUp ()
    {
      _resultModification = new TestResultModification (CollectionExecutionStrategy.Instance);
    }

    [Test]
    public void Accept ()
    {
      var queryModel = ExpressionHelper.CreateQueryModel ();
      var selectClause = ExpressionHelper.CreateSelectClause ();

      var visitorMock = MockRepository.GenerateMock<IQueryModelVisitor> ();
      _resultModification.Accept (visitorMock, queryModel, selectClause, 1);

      visitorMock.AssertWasCalled (mock => mock.VisitResultModification (_resultModification, queryModel, selectClause, 1));
    }
  }
}