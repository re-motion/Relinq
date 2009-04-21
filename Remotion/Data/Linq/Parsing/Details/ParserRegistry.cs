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
