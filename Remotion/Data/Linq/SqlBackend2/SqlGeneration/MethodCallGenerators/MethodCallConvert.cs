// This file is part of the re-motion Core Framework (www.re-motion.org)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
// 
// The re-motion Core Framework is free software; you can redistribute it 
// and/or modify it under the terms of the GNU Lesser General Public License 
// as published by the Free Software Foundation; either version 2.1 of the 
// License, or (at your option) any later version.
// 
// re-motion is distributed in the hope that it will be useful, 
// but WITHOUT ANY WARRANTY; without even the implied warranty of 
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the 
// GNU Lesser General Public License for more details.
// 
// You should have received a copy of the GNU Lesser General Public License
// along with re-motion; if not, see http://www.gnu.org/licenses.
// 
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Remotion.Data.Linq.Parsing;
using Remotion.Data.Linq.Utilities;

namespace Remotion.Data.Linq.SqlBackend.SqlGeneration.MethodCallGenerators
{
  /// <summary>
  /// <see cref="MethodCallConvert"/> implements <see cref="IMethodCallSqlGenerator"/> for the convert method.
  /// </summary>
  public class MethodCallConvert : IMethodCallSqlGenerator
  {
    private Dictionary<Type, string> _mappingTypes;

    public MethodCallConvert ()
    {
      _mappingTypes = new Dictionary<Type, string>();
      _mappingTypes.Add (typeof (string), "nvarchar(max)");
      _mappingTypes.Add (typeof (bool), "bit");
      _mappingTypes.Add (typeof (Int64), "bigint");
      _mappingTypes.Add (typeof (DateTime), "date");
      _mappingTypes.Add (typeof (double), "FLOAT");
      _mappingTypes.Add (typeof (int), "int");
      _mappingTypes.Add (typeof (decimal), "numeric");
      _mappingTypes.Add (typeof (char), "nvarchar(1)");
      _mappingTypes.Add (typeof (byte), "tinyint");
    }

    public void GenerateSql (MethodCallExpression methodCallExpression, SqlCommandBuilder commandBuilder, ExpressionTreeVisitor expressionTreeVisitor)
    {
      ArgumentUtility.CheckNotNull ("methodCallExpression", methodCallExpression);
      ArgumentUtility.CheckNotNull ("commandBuilder", commandBuilder);
      ArgumentUtility.CheckNotNull ("expressionTreeVisitor", expressionTreeVisitor);

      if (methodCallExpression.Arguments.Count != 1)
        throw new ArgumentException ("Wrong number of arguments in evaluation");

      Type type = methodCallExpression.Type;
      string exp;
      if (_mappingTypes.TryGetValue (type, out exp))
      {
        commandBuilder.Append ("CONVERT(" + exp + ",");
        expressionTreeVisitor.VisitExpression (methodCallExpression.Arguments[0]);
        commandBuilder.Append (")");
      }
      else
        throw new NotSupportedException ("TypeCast is not supported by linq parser.");
    }
  }
}