// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Clauses;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.IntermediateModel;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.IntermediateModel
{
  [TestFixture]
  public class QuerySourceExpressionNodeUtilityTest
  {
    private IQuerySourceExpressionNode _node;
    private IQuerySource _querySource;
    private ClauseGenerationContext _context;

    [SetUp]
    public void SetUp ()
    {
      _context = new ClauseGenerationContext (new NodeTypeRegistry());
      _node = new MainSourceExpressionNode ("x", Expression.Constant (new int[0]));
      _querySource = new MainFromClause ("x", typeof (int), Expression.Constant (new int[0]));
    }

    [Test]
    public void GetQuerySourceForNode ()
    {
      _context.AddContextInfo (_node, _querySource);

      Assert.That (QuerySourceExpressionNodeUtility.GetQuerySourceForNode (_node, _context), Is.SameAs (_querySource));
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. " 
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void GetQuerySourceForNode_NoClauseRegistered ()
    {
      QuerySourceExpressionNodeUtility.GetQuerySourceForNode (_node, _context);
    }

    [Test]
    public void ReplaceParameterWithReference ()
    {
      _context.AddContextInfo (_node, _querySource);

      var parameter = Expression.Parameter (typeof (int), "x");
      var expression = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (1), parameter);
      var result = QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (_node, parameter, expression, _context);

      var expected = ExpressionHelper.Resolve<int, int> (_querySource, (i => 1 + i));

      ExpressionTreeComparer.CheckAreEqualTrees (expected, result);
    }

    [Test]
    [ExpectedException (typeof (InvalidOperationException), ExpectedMessage = "Cannot retrieve an IQuerySource for the given MainSourceExpressionNode. "
        + "Be sure to call Apply before calling methods that require IQuerySources, and pass in the same QuerySourceClauseMapping to both.")]
    public void ReplaceParameterWithReference_NoClauseRegistered ()
    {
      var parameter = Expression.Parameter (typeof (int), "x");
      var expression = Expression.MakeBinary (ExpressionType.Add, Expression.Constant (1), parameter);
      QuerySourceExpressionNodeUtility.ReplaceParameterWithReference (_node, parameter, expression, _context);
    }
  }
}
