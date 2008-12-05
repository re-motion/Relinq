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
using System.Reflection;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Parsing.Structure;

namespace Remotion.Data.UnitTests.Linq.ParsingTest.StructureTest
{
  [TestFixture]
  public class CallParserDispatcherTest
  {
    private CallParserDispatcher _dispatcher;
    private IQueryable<Student> _source;
    private MethodInfo _method;
    private Expression<Func<object>> _wrappedCall;
    private System.Action<ParseResultCollector, MethodCallExpression> _emptyParser;

    [SetUp]
    public void SetUp ()
    {
      _dispatcher = new CallParserDispatcher ();
      _source = ExpressionHelper.CreateQuerySource ();
      _wrappedCall = (() => _source.Select (s => s.First));
      _method = ParserUtility.GetMethod (_wrappedCall);
      _emptyParser = delegate { };
    }

    [Test]
    public void CanParse_True()
    {
      _dispatcher.RegisterParser ("Select", _emptyParser);
      Assert.IsTrue (_dispatcher.CanParse (_method));
    }

    [Test]
    public void CanParse_False ()
    {
      Assert.IsFalse (_dispatcher.CanParse (_method));
    }

    [Test]
    public void GetParser_Registered ()
    {
      System.Action<ParseResultCollector, MethodCallExpression> parser = _emptyParser;
      _dispatcher.RegisterParser ("Select", parser);

      Assert.That (_dispatcher.GetParser (_method), Is.SameAs (parser));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected OrderBy, Where for dispatching a method call to a parser, found Select.")]
    public void GetParser_Unregistered ()
    {
      _dispatcher.RegisterParser ("OrderBy", _emptyParser);
      _dispatcher.RegisterParser ("Where", _emptyParser);
      
      _dispatcher.GetParser (_method);
    }

    [Test]
    public void Dispatch_Registered ()
    {
      ParseResultCollector actualCollector = null;
      Expression actualExpression = null;

      _dispatcher.RegisterParser ("Select", (collector, expression) => { actualCollector = collector; actualExpression = expression; });

      ParseResultCollector expectedCollector = new ParseResultCollector (Expression.Constant (0));
      MethodCallExpression expectedExpression = (MethodCallExpression) _wrappedCall.Body;
      
      _dispatcher.Dispatch (expectedCollector, expectedExpression, null);

      Assert.That (actualCollector, Is.SameAs (expectedCollector));
      Assert.That (actualExpression, Is.SameAs (expectedExpression));
    }

    [Test]
    public void Dispatch_WithPotentialFromIdentifier ()
    {
      ParseResultCollector actualCollector = null;
      Expression actualExpression = null;
      ParameterExpression actualPotentialFromIdentifier = null;

      _dispatcher.RegisterParser ("Select", (collector, expression, potentialFromIdentifier) =>
      {
        actualCollector = collector;
        actualExpression = expression;
        actualPotentialFromIdentifier = potentialFromIdentifier;
      });

      ParseResultCollector expectedCollector = new ParseResultCollector (Expression.Constant (0));
      MethodCallExpression expectedExpression = (MethodCallExpression) _wrappedCall.Body;
      ParameterExpression expectedPotentialFromIdentifier = ExpressionHelper.CreateParameterExpression();
      _dispatcher.Dispatch (expectedCollector, expectedExpression, expectedPotentialFromIdentifier);

      Assert.That (actualCollector, Is.SameAs (expectedCollector));
      Assert.That (actualExpression, Is.SameAs (expectedExpression));
      Assert.That (actualPotentialFromIdentifier, Is.SameAs (expectedPotentialFromIdentifier));
    }

    [Test]
    [ExpectedException (typeof (ParserException), ExpectedMessage = "Expected OrderBy, Where for dispatching a method call to a parser, found Select.")]
    public void Dispatch_Unregistered ()
    {
      _dispatcher.RegisterParser ("OrderBy", _emptyParser);
      _dispatcher.RegisterParser ("Where", _emptyParser);

      _dispatcher.Dispatch (null, (MethodCallExpression) _wrappedCall.Body, null);
    }
  }
}
