/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

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
