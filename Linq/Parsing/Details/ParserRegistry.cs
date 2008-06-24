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
      throw new ParseException ("Cannot parse " + expression.NodeType + ", no appropriate parser found");
    }
  }
}