/* Copyright (C) 2005 - 2008 rubicon informationstechnologie gmbh
 *
 * This program is free software: you can redistribute it and/or modify it under 
 * the terms of the re:motion license agreement in license.txt. If you did not 
 * receive it, please visit http://www.re-motion.org/licensing.
 * 
 * Unless otherwise provided, this software is distributed on an "AS IS" basis, 
 * WITHOUT WARRANTY OF ANY KIND, either express or implied. 
 */

using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.DataObjectModel;
using Remotion.Data.Linq.Parsing.FieldResolving;
using Remotion.Utilities;

namespace Remotion.Data.Linq.Parsing.Details
{
  public class ParseContext
  {
    public QueryModel QueryModel { get; private set; }
    public Expression ExpressionTreeRoot { get; private set; }
    public List<FieldDescriptor> FieldDescriptors { get; private set; }
    public JoinedTableContext JoinedTableContext { get; private set; }

    public ParseContext (QueryModel queryModel, Expression expressionTreeRoot, List<FieldDescriptor> fieldDescriptors, JoinedTableContext joinedTableContext)
    {
      ArgumentUtility.CheckNotNull ("queryModel", queryModel);
      ArgumentUtility.CheckNotNull ("expressionTreeRoot", expressionTreeRoot);
      ArgumentUtility.CheckNotNull ("fieldDescriptors", fieldDescriptors);
      ArgumentUtility.CheckNotNull ("joinedTableContext", joinedTableContext);

      QueryModel = queryModel;
      ExpressionTreeRoot = expressionTreeRoot;
      FieldDescriptors = fieldDescriptors;
      JoinedTableContext = joinedTableContext;
    }
  }
}
