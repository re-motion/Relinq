// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Clauses.StreamedData;
using Remotion.Linq.UnitTests.Linq.Core.Parsing;
using Remotion.Linq.UnitTests.Linq.Core.TestDomain;

namespace Remotion.Linq.UnitTests.Linq.Core
{
  [TestFixture]
  public class QueryModelIntegrationTest
  {
    [Test]
    public void QueryModel_GetOutputDataInfo_WithAssignableItemExpression ()
    {
      var query = from o in ExpressionHelper.CreateCookQueryable()
                  select o;
      var queryModel = ExpressionHelper.ParseQuery (query);

      queryModel.SelectClause.Selector = new TestExtensionExpression (Expression.Convert (queryModel.SelectClause.Selector, typeof (Chef)));

      var outputDataInfo = queryModel.GetOutputDataInfo();

      Assert.That (outputDataInfo, Is.TypeOf<StreamedSequenceInfo>());
      Assert.That (((StreamedSequenceInfo) outputDataInfo).ItemExpression, Is.SameAs (queryModel.SelectClause.Selector));
      Assert.That (((StreamedSequenceInfo) outputDataInfo).ItemExpression.Type, Is.SameAs (typeof (Chef)));
      Assert.That (((StreamedSequenceInfo) outputDataInfo).DataType, Is.SameAs (typeof (IQueryable<Cook>)));
    }
  }
}