// This file is part of the re-linq project (relinq.codeplex.com)
// Copyright (C) 2005-2009 rubicon informationstechnologie gmbh, www.rubicon.eu
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
using System.Linq.Expressions;
using System.Reflection;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Detects expressions calling the Information.IsNothing (...) method used by Visual Basic .NET, and replaces them with 
  /// <see cref="BinaryExpression"/> instances comparing with <see langword="null" />. Providers use this transformation to be able to 
  /// handle queries using IsNothing (...) more easily.
  /// </summary>
  public class VBInformationIsNothingExpressionTransformer : IExpressionTransformer<MethodCallExpression>
  {
    private const string c_vbInformationClassName = "Microsoft.VisualBasic.Information";
    private const string c_vbIsNothingMethodName = "IsNothing";

    public ExpressionType[] SupportedExpressionTypes
    {
      get { return new[] { ExpressionType.Call }; }
    }

    public Expression Transform (MethodCallExpression expression)
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      if (IsVBIsNothing (expression.Method))
        return Expression.Equal (expression.Arguments[0], Expression.Constant (null));
      
      return expression;
    }

    private bool IsVBIsNothing (MethodInfo operatorMethod)
    {
      return operatorMethod.DeclaringType.FullName == c_vbInformationClassName && operatorMethod.Name == c_vbIsNothingMethodName;
    }
  }
}