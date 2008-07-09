/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class ParserRegistry
  {
    private readonly MultiDictionary<Type, IParser> _parsers;

    public ParserRegistry()
    {
      _parsers = new MultiDictionary<Type, IParser> ();
    }

    public void RegisterParser (Type expressionType, IParser parser)
    {
      _parsers[expressionType].Insert (0, parser);
    }

    public IEnumerable<IParser> GetParsers (Type expressionType)
    {
      return _parsers[expressionType];
    }

    public IParser GetParser (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      foreach (IParser parser in GetParsers (expression.GetType()))
      {
        if (parser.CanParse (expression))
          return parser;
      }
      throw new ParseException ("Cannot parse " + expression + ", no appropriate parser found");
    }
  }
}
