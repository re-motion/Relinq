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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Parsing.Details;
using Rhino.Mocks;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class ParameterExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new WhereFieldAccessPolicy (StubDatabaseInfo.Instance));

      var fromSource = QueryModel.MainFromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      FieldDescriptor expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (fromSource, "IDColumn"));
      ICriterion expectedCriterion = expectedFieldDescriptor.Column;

      ParameterExpressionParser parser = new ParameterExpressionParser (resolver);

      ICriterion actualCriterion = parser.Parse (parameter, ParseContext);
      Assert.AreEqual (expectedCriterion, actualCriterion);
      Assert.That (ParseContext.FieldDescriptors, Is.EqualTo (new[] { expectedFieldDescriptor }));
    }
  }
}
