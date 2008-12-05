// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2008 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details.WhereConditionParsing
{
  public class MemberExpressionParser : IWhereConditionParser
  {
    private readonly ClauseFieldResolver _resolver;

    public MemberExpressionParser (ClauseFieldResolver resolver)
    {
      ArgumentUtility.CheckNotNull ("resolver", resolver);
      _resolver = resolver;
    }

    public virtual ICriterion Parse (MemberExpression memberExpression, ParseContext parseContext)
    {
      FieldDescriptor fieldDescriptor = parseContext.QueryModel.ResolveField (_resolver, memberExpression, parseContext.JoinedTableContext);
      parseContext.FieldDescriptors.Add (fieldDescriptor);
      return fieldDescriptor.GetMandatoryColumn ();
    }

    ICriterion IWhereConditionParser.Parse (Expression expression, ParseContext parseContext)
    {
      return Parse ((MemberExpression) expression, parseContext);
    }

    public bool CanParse(Expression expression)
    {
      return expression is MemberExpression;
    }
  }
}
