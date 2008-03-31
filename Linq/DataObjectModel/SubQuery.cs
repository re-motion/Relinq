using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class SubQuery : IFromSource
  {
    public SubQuery (QueryModel queryModel, string alias)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);
      ArgumentUtility.CheckNotNull ("alias", alias);

      QueryModel = queryModel;
      Alias = alias;
    }

    public QueryModel QueryModel { get; private set; }
    public string Alias { get; private set; }
    

    public string AliasString
    {
      get { return Alias; }
    }

    
  }
}