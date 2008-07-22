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
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.WhereConditionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.WhereConditionParsingTest
{
  [TestFixture]
  public class MemberExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse()
    {
      ParameterExpression parameter = Expression.Parameter (typeof (Student), "s");
      MainFromClause fromClause = ExpressionHelper.CreateMainFromClause(parameter, ExpressionHelper.CreateQuerySource ());;
      QueryModel queryModel = ExpressionHelper.CreateQueryModel (fromClause);
      JoinedTableContext context = new JoinedTableContext ();
      ClauseFieldResolver resolver = 
        new ClauseFieldResolver(StubDatabaseInfo.Instance,new WhereFieldAccessPolicy(StubDatabaseInfo.Instance));
      MemberExpressionParser parser = new MemberExpressionParser (resolver);

      MemberExpression memberExpression = Expression.MakeMemberAccess (parameter, typeof (Student).GetProperty ("ID"));
      parser.Parse (memberExpression, ParseContext);
      Assert.That (ParseContext.FieldDescriptors, Is.Not.Empty);
    }
  }
}
