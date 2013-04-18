// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (c) rubicon IT GmbH, www.rubicon.eu
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
using System.Linq.Expressions;
using System.Runtime.Serialization;
using System.Security;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing
{
  [Serializable]
  public class ParserException : Exception
  {
    private static string CreateMessage (object expected, object expression, string context)
    {
      ArgumentUtility.CheckNotNull ("expected", expected);
      ArgumentUtility.CheckNotNull ("expression", expression);
      ArgumentUtility.CheckNotNull ("context", context);

      if (expression is Expression)
        return string.Format ("Expected {0} for {1}, found '{2}' ({3}).", expected, context, expression, expression.GetType ().Name);
      else
        return string.Format ("Expected {0} for {1}, found '{2}'.", expected, context, expression);
    }

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

    public ParserException (object expected, object expression, string context)
      : this (CreateMessage (expected, expression, context), expression, null)
    {
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
