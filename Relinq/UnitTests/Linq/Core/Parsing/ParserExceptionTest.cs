// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) rubicon IT GmbH, www.rubicon.eu
// 
// re-linq is free software; you can redistribute it and/or modify it under 
// the terms of the GNU Lesser General Public License as published by the 
// Free Software Foundation; either version 2.1 of the License, 
// or (at your option) any later version.
// 
// re-linq is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-linq; if not, see http://www.gnu.org/licenses.
// 
using System;
using NUnit.Framework;
using Remotion.Linq.UnitTests.Linq.Core.TestUtilities;
using Remotion.Linq.Parsing;

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing
{
  [TestFixture]
  public class ParserExceptionTest
  {
    [Test]
    public void Serialization ()
    {
      var exception = new ParserException ("test", "expr", new Exception ("test2"));

      var deserializedException = Serializer.SerializeAndDeserialize (exception);
      Assert.That (deserializedException.Message, Is.EqualTo (exception.Message));
      Assert.That (deserializedException.ParsedExpression, Is.EqualTo (exception.ParsedExpression));
      Assert.That (deserializedException.InnerException.Message, Is.EqualTo (exception.InnerException.Message));
    }
  }
}
