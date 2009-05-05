// Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh 
// All rights reserved.
//

using System;
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.WhereExpressionParserTest;
using Remotion.Data.UnitTests.Linq.TestQueryGenerators;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest.SelectExpressionParserTest
{
  [TestFixture]
  public class MainFromExpressionParserTest
  {
    private IQueryable<Student> _querySource;
    private MethodCallExpression _expression;
    private ParseResultCollector _result;
    private BodyHelper _bodyHelper;

    [SetUp]
    public void SetUp ()
    {
      _querySource = ExpressionHelper.CreateQuerySource ();
      _expression = SelectTestQueryGenerator.CreateSubQueryInMainFrom (_querySource);
      _result = new ParseResultCollector (_expression);
      new LetExpressionParser ().Parse (_result, _expression);
      _bodyHelper = new BodyHelper (_result.BodyExpressions);

      
    }

    [Test]
    [Ignore("check this test")]
    public void ParseMainFromExpressionData ()
    {
      Assert.IsNotNull (_bodyHelper.FromExpressions);
      Assert.That (_bodyHelper.FromExpressions, Is.EqualTo (new object[] { _expression.Arguments[0] }));
      //Assert.IsInstanceOfType (typeof (MainFromExpressionData), _bodyHelper.FromExpressions[0]);
    }
  }
}