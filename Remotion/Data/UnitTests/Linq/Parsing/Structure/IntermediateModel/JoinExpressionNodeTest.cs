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
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;
using System.Linq;

namespace Remotion.Data.UnitTests.Linq.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class JoinExpressionNodeTest : ExpressionNodeTestBase
  {
    private JoinExpressionNode _node;
    private List<string> _innerSequence;

    [SetUp]
    public override void SetUp ()
    {
      base.SetUp ();

      _innerSequence = new List<string> { "1", "2", "3" };
      var outerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (i => i.ToString ());
      var innerKeySelector = ExpressionHelper.CreateLambdaExpression<string, string> (o => o.ToString ());
      var resultSelector = ExpressionHelper.CreateLambdaExpression<string, string, string> ((i, o) => o.ToString());
      _node = new JoinExpressionNode (CreateParseInfo(), _innerSequence, outerKeySelector, innerKeySelector, resultSelector);
    }

    [Test]
    public void SupportedMethods ()
    {
      AssertSupportedMethod_Generic (
        JoinExpressionNode.SupportedMethods,
        q => q.Join (_innerSequence, i => i.ToString (), o => o, (i, o) => o),
        e => e.Join (_innerSequence, i => i.ToString (), o => o, (i, o) => o));      
    }

  }
}