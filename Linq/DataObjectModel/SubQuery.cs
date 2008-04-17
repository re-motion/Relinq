using System;
using Rubicon.Utilities;

namespace Rubicon.Data.Linq.DataObjectModel
{
  public class SubQuery : IColumnSource, ICriterion
  {
    public SubQuery (QueryModel queryModel, string alias)
    {
      ArgumentUtility.CheckNotNull ("queryExpression", queryModel);

      QueryModel = queryModel;
      Alias = alias;
    }

    public QueryModel QueryModel { get; private set; }
    public string Alias { get; private set; }
    

    public string AliasString
    {
      get { return Alias; }
    }

    public override bool Equals (object obj)
    {
      SubQuery other = obj as SubQuery;
      return other != null && object.Equals (QueryModel, other.QueryModel) && object.Equals (Alias, other.Alias);
    }

    public override int GetHashCode ()
    {
      return EqualityUtility.GetRotatedHashCode (Alias, QueryModel);
    }

    public void Accept (IEvaluationVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      visitor.VisitSubQuery (this);
    }
  }
}