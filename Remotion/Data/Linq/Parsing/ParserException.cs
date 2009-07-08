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
using System.Runtime.Serialization;

namespace Remotion.Data.Linq.Parsing
{
  [Serializable]
  public class ParserException : Exception
  {
    public ParserException (string message)
        : this (message, null, null)
    {
    }

    public ParserException (string message, Exception inner)
        : base (message, inner)
    {
    }

    public ParserException (string message, object parsedExpression, Exception inner)
        : base (message, inner)
    {
      ParsedExpression = parsedExpression;
    }

    protected ParserException (SerializationInfo info, StreamingContext context)
        : base (info, context)
    {
      ParsedExpression = info.GetValue ("ParserException.ParsedExpression", typeof (object));
    }

    public object ParsedExpression { get; private set; }

    public override void GetObjectData (SerializationInfo info, StreamingContext context)
    {
      base.GetObjectData (info, context);

      info.AddValue ("ParserException.ParsedExpression", ParsedExpression);
    }
  }
}