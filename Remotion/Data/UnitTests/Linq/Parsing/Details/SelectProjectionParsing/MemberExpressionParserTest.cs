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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Backend.DataObjectModel;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.Linq.Parsing.FieldResolving;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details.SelectProjectionParsing
{
  [TestFixture]
  public class MemberExpressionParserTest : DetailParserTestBase
  {
    [Test]
    public void Parse ()
    {
      var resolver = new FieldResolver (StubDatabaseInfo.Instance, new SelectFieldAccessPolicy());

      var parser = new MemberExpressionParser (resolver);
      var fieldDescriptorCollection = new List<FieldDescriptor>();
      MemberExpression memberExpression = Expression.MakeMemberAccess (StudentReference, typeof (Student).GetProperty ("ID"));
      IColumnSource fromSource = ParseContext.JoinedTableContext.GetColumnSource (StudentClause);
      var path = new FieldSourcePath (fromSource, new SingleJoin[0]);
      var expectedFieldDescriptor = new FieldDescriptor (null, path, new Column (fromSource, "IDColumn"));

      IEvaluation actualEvaluation = parser.Parse (memberExpression, ParseContext);
      IEvaluation expectedEvaluation = expectedFieldDescriptor.Column;

      Assert.That (fieldDescriptorCollection, Is.Not.Null);
      Assert.That (actualEvaluation, Is.EqualTo (expectedEvaluation));
    }
  }
}
