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
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Represents an <see cref="Expression"/> that holds a subquery. The subquery is held by <see cref="QueryModel"/> in its parsed form.
  /// </summary>
  public class SubQueryExpression : Expression
  {
    private readonly Type _type;
    public const ExpressionType ExpressionType = (ExpressionType) 100002;
    
    public SubQueryExpression (QueryModel queryModel)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);

      QueryModel = queryModel;
      _type = queryModel.GetOutputDataInfo().DataType;
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType; }
    }

    public override Type Type
    {
      get { return _type; }
    }

    public QueryModel QueryModel { get; private set; }
  }
}
