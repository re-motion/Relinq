using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class SubQuery : IFromSource
  {
    public SubQuery (QueryExpression queryExpression, string alias)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryExpression);
      ArgumentUtility.CheckNotNull ("alias", alias);

      QueryExpression = queryExpression;
      Alias = alias;
    }

    public QueryExpression QueryExpression { get; private set; }
    public string Alias { get; private set; }
    

    public string AliasString
    {
      get { return Alias; }
    }

    
  }
}