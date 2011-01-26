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
using Remotion.Data.Linq.Parsing.Structure.ExpressionTreeProcessingSteps;

namespace Remotion.Data.Linq.UnitTests.Linq.Core.Parsing.Structure.ExpressionTreeProcessingSteps
{
  [TestFixture]
  public class PartialEvaluationStepTest
  {
    [Test]
    public void Process ()
    {
      var expression = Expression.Add (Expression.Constant (1), Expression.Constant (1));
      var step = new PartialEvaluationStep();

      var result = step.Process (expression);

      Assert.That (result, Is.TypeOf (typeof (ConstantExpression)));
      Assert.That (((ConstantExpression) result).Value, Is.EqualTo(2));
    }
  }
}