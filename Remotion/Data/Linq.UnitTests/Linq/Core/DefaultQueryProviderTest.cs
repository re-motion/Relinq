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
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;
using Remotion.Data.Linq.UnitTests.Linq.Core.TestDomain;
using Remotion.Data.Linq.Utilities;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core
{
  [TestFixture]
  public class DefaultQueryProviderTest
  {
    private IQueryExecutor _executorStub;

    [SetUp]
    public void SetUp ()
    {
      _executorStub = MockRepository.GenerateStub<IQueryExecutor> ();
    }

    [Test]
    public void Initialization_WithDefaults ()
    {
      var queryProvider = new DefaultQueryProvider (typeof (TestQueryable<>), _executorStub);

      Assert.That (
          queryProvider.QueryParser.NodeTypeRegistry.RegisteredMethodInfoCount,
          Is.EqualTo (MethodCallExpressionNodeTypeRegistry.CreateDefault ().RegisteredMethodInfoCount));

      Assert.That (queryProvider.QueryParser.ProcessingSteps.Count, Is.EqualTo (2));
      Assert.That (queryProvider.QueryParser.ProcessingSteps[0], Is.TypeOf (typeof (PartialEvaluationStep)));
      Assert.That (queryProvider.QueryParser.ProcessingSteps[1], Is.TypeOf (typeof (ExpressionTransformationStep)));
      Assert.That (
          ((ExpressionTransformationStep) queryProvider.QueryParser.ProcessingSteps[1]).Provider,
          Is.TypeOf (typeof (ExpressionTransformerRegistry)));

      var expressionTransformerRegistry =
          ((ExpressionTransformerRegistry) ((ExpressionTransformationStep) queryProvider.QueryParser.ProcessingSteps[1]).Provider);
      Assert.That (
          expressionTransformerRegistry.RegisteredTransformerCount,
          Is.EqualTo (ExpressionTransformerRegistry.CreateDefault ().RegisteredTransformerCount));
    }


    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_NonGeneric ()
    {
      new DefaultQueryProvider (typeof (int), _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_NonGenericTypeDefinition ()
    {
      new DefaultQueryProvider (typeof (TestQueryable<int>), _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_TooManyGenericArguments ()
    {
      new DefaultQueryProvider (typeof (QueryableWithTooManyArguments<,>), _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_NonQueryable ()
    {
      new DefaultQueryProvider (typeof (List<int>), _executorStub);
    }

    [Test]
    [ExpectedException (typeof (ArgumentTypeException))]
    public void Initialization_ParserCtor_AlsoPerformsTypeChecks ()
    {
      new DefaultQueryProvider (typeof (int), _executorStub, QueryParser.CreateDefault());
    }

    [Test]
    public void CreateQueryable ()
    {
      var executorStub = _executorStub;
      var provider = new DefaultQueryProvider (typeof (TestQueryable<>), executorStub);
      var expression = Expression.Constant (new[] {0});

      var queryable = provider.CreateQuery<int> (expression);

      Assert.That (queryable, Is.InstanceOfType (typeof (TestQueryable<int>)));
      Assert.That (queryable.Provider, Is.SameAs (provider));
      Assert.That (queryable.Expression, Is.SameAs (expression));
    }
  }
}