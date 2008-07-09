/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using Remotion.Utilities;

namespace Remotion.Data.Linq.DataObjectModel
{
  public class SubQuery : IColumnSource, ICriterion
  {
    public SubQuery (QueryModel queryModel, string alias)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      QueryModel = queryModel;
      Alias = alias;
    }

    public QueryModel QueryModel { get; private set; }
    public string Alias { get; private set; }
    

    public string AliasString
    {
      get { return Alias; }
    }

    public bool IsTable
    {
      get { return true; }
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
