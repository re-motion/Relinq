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
