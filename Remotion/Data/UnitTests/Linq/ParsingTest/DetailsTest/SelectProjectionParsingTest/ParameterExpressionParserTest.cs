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
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ParameterExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause (parameter, ExpressionHelper.CreateQuerySource ());
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);

      ClauseFieldResolver resolver =
          new ClauseFieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy());

      List<FieldDescriptor> fieldDescriptorCollection = new List<FieldDescriptor> ();
      var fromSource = fromClause.GetFromSource (StubDatabaseInfo.Instance);
      FieldSourcePath path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      FieldDescriptor expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (fromSource, "*"));

      IEvaluation expectedEvaluation = expectedFieldDescriptor.Column;

      ParameterExpressionParser parser = new ParameterExpressionParser (resolver);

      IEvaluation actualEvaluation = parser.Parse (parameter, ParseContext);
      Assert.AreEqual (expectedEvaluation, actualEvaluation);
    }
  }
}
