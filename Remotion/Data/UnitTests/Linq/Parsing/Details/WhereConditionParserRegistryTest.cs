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
using System.Linq;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Data.Linq.Backend;
using Remotion.Data.Linq.Clauses.Expressions;
using Remotion.Data.Linq.Backend.Details;
using Remotion.Data.Linq.Backend.Details.WhereConditionParsing;
using NUnit.Framework.SyntaxHelpers;

namespace Remotion.Data.UnitTests.Linq.Parsing.Details
{
  [TestFixture]
  public class WhereConditionParserRegistryTest
  {
    private IDatabaseInfo _databaseInfo;
    private WhereConditionParserRegistry _whereConditionParserRegistry;

    [SetUp]
    public void SetUp ()
    {
      _databaseInfo = StubDatabaseInfo.Instance;
      _whereConditionParserRegistry = new WhereConditionParserRegistry (_databaseInfo);
    }

    [Test]
    public void Initialization_AddsDefaultParsers ()
    {
      var whereConditionParserRegistry = new WhereConditionParserRegistry (_databaseInfo);

      Assert.That (whereConditionParserRegistry.GetParsers (typeof (BinaryExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (MemberExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (ConstantExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (SubQueryExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (UnaryExpression)).ToArray (), Is.Not.Empty);
      Assert.That (whereConditionParserRegistry.GetParsers (typeof (QuerySourceReferenceExpression)).ToArray (), Is.Not.Empty);      
    }

    [Test]
    public void Initialization_MethodCallParsers ()
    {
      var whereConditionParserRegistry = new WhereConditionParserRegistry (_databaseInfo);

      var parsers = whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).ToArray ();
      Assert.That (parsers.SingleOrDefault (p => p.GetType () == typeof (MethodCallExpressionParser)), Is.Not.Null);
      Assert.That (parsers.SingleOrDefault (p => p.GetType () == typeof (LikeParser)), Is.Not.Null);
      Assert.That (parsers.SingleOrDefault (p => p.GetType () == typeof (ContainsParser)), Is.Not.Null);
      Assert.That (parsers.SingleOrDefault (p => p.GetType () == typeof (ContainsFullTextParser)), Is.Not.Null);
    }

    [Test]
    public void RegisterNewMethodCallExpressionParser_RegisterFirst ()
    {
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (4));
      
      var likeParser = new LikeParser (_whereConditionParserRegistry);
      _whereConditionParserRegistry.RegisterParser (typeof (MethodCallExpression), likeParser);
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).Count (), Is.EqualTo (5));
      Assert.That (_whereConditionParserRegistry.GetParsers (typeof (MethodCallExpression)).First (), Is.SameAs (likeParser));
    }
    
    [Test]
    public void GetParser ()
    {
      ConstantExpression constantExpression = Expression.Constant ("test");
      IWhereConditionParser expectedParser = _whereConditionParserRegistry.GetParsers (typeof (ConstantExpression)).First();

      Assert.AreSame (expectedParser, _whereConditionParserRegistry.GetParser (constantExpression));
    }

  }
}
