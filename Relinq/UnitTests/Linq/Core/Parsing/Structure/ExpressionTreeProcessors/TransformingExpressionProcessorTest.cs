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
using System.Linq.Expressions;
using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;
using Remotion.Data.Linq.Parsing.ExpressionTreeVisitors.Transformation;
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessors;
using Rhino.Mocks;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.ExpressionTreeProcessors
{
  [TestFixture]
  public class TransformingExpressionProcessorTest
  {
    [Test]
    public void Process ()
    {
      var inputExpression = Expression.Constant (1);
      var transformedExpression = Expression.Constant (2);
      var transformationProviderMock = MockRepository.GenerateStrictMock<IExpressionTranformationProvider>();
      transformationProviderMock
          .Expect (mock => mock.GetTransformations (inputExpression))
          .Return (new ExpressionTransformation[] { expr => transformedExpression });
      transformationProviderMock
          .Expect (mock => mock.GetTransformations (transformedExpression))
          .Return (new ExpressionTransformation[0]);
      transformationProviderMock.Replay();
      
      var processor = new TransformingExpressionTreeProcessor (transformationProviderMock);

      var result = processor.Process (inputExpression);

      transformationProviderMock.VerifyAllExpectations();
      Assert.That (result, Is.SameAs (transformedExpression));
    }
  }
}