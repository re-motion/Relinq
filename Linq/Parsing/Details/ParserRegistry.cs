using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Remotion.Collections;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class ParserRegistry
  {
    private MultiDictionary<Type, object> _parsers;

    public ParserRegistry()
    {
      _parsers = new MultiDictionary<Type, object> ();
    }

    public void RegisterParser<TExpression> (IParser<TExpression> parser) where TExpression : Expression
    {
      _parsers[typeof (TExpression)].Insert (0, parser);
    }

    public IEnumerable<IParser<TExpression>> GetParsers<TExpression> () where TExpression : Expression
    {
      return _parsers[typeof (TExpression)].Cast<IParser<TExpression>> ();
    }

    public IParser<TExpression> GetParser<TExpression> (TExpression expression) where TExpression : Expression
    {
      foreach (IParser<TExpression> parser in GetParsers<TExpression> ())
      {
        if (parser.CanParse (expression))
          return parser;
      }
      throw new ParseException ("Cannot parse " + expression.NodeType + ", no appropriate parser found");
    }

    public IParser GetParser (Expression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      IParser parser = GetParserWithType (expression.GetType ());
      if ( parser != null) 
        return parser;
      throw new ParseException ("Cannot parse " + expression.NodeType + ", no appropriate parser found");
    }

    private IParser GetParserWithType (Type type)
    {
      return (IParser) _parsers[type].First();    
    }

  }
}