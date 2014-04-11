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

namespace Remotion.Linq.UnitTests.Linq.Core.Parsing.ExpressionTreeVisitors
{
  public class UnknownExpression : Expression
  {
    private readonly Type _type;
    private readonly ExpressionType _nodeType;

    public UnknownExpression (Type type)
      : this (type, (ExpressionType) (-1))
    {
    }

    public UnknownExpression (Type type, ExpressionType nodeType)
    {
      _type = type;
      _nodeType = nodeType;
    }

    public override ExpressionType NodeType
    {
      get { return _nodeType; }
    }

    public override Type Type
    {
      get { return _type; }
    }
  }
}
