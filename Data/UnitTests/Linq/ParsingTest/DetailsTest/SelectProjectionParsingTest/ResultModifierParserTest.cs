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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Details;
using Remotion.Data.Linq.Parsing.Details.SelectProjectionParsing;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;
using System.Collections.Generic;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.DetailsTest.SelectProjectionParsingTest
{
  [TestFixture]
  public class ResultModifierParserTest : DetailParserTestBase
  {
    private SelectProjectionParserRegistry _registry;
    private IQueryable<Student> _source;

    public override void SetUp ()
    {
      base.SetUp ();
      _registry = new SelectProjectionParserRegistry (StubDatabaseInfo.Instance, new ParseMode ());
      _source = null;
    }

    [Test]
    public void MethodCallEvaluation_Count ()
    {
      var methodInfo = ParserUtility.GetMethod (() => Enumerable.Count (_source));
      MethodCallExpression resultModifierExpression = Expression.Call (methodInfo, Expression.Constant (null, typeof (IQueryable<Student>)));

      ResultModifierParser parser = new ResultModifierParser (_registry);
      MethodCall result = parser.Parse (resultModifierExpression, ParseContext);

      MethodCall expected = new MethodCall (methodInfo, null, new List<IEvaluation> { SourceMarkerEvaluation.Instance });
      Assert.That (result, Is.EqualTo (expected));
    }
  }
}
