// Copyright (c) rubicon IT GmbH, www.rubicon.eu
//
// See the NOTICE file distributed with this work for additional information
// regarding copyright ownership.  rubicon licenses this file to you under 
// the Apache License, Version 2.0 (the "License"); you may not use this 
// file except in compliance with the License.  You may obtain a copy of the 
// License at
//
//   http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, WITHOUT 
// WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.  See the 
// License for the specific language governing permissions and limitations
// under the License.
// 

using System;
using System.Linq.Expressions;
#if NET_3_5
using Remotion.Linq.Clauses.Expressions;
#endif
using Remotion.Linq.Utilities;
#if NET_3_5
using Remotion.Linq.Parsing;
#endif
using Remotion.Utilities;

namespace Remotion.Linq.Development.UnitTesting.Clauses.Expressions
{
  public sealed class ReducibleExtensionExpression
#if !NET_3_5
    : Expression
#else
    : ExtensionExpression
#endif
  {
    private readonly Expression _expression;
#if !NET_3_5
    private readonly Type _type;
#endif

    public ReducibleExtensionExpression (Expression expression, Type type = null)
#if NET_3_5
        : base(_type ?? expression.Type)
#endif
    {
      ArgumentUtility.CheckNotNull ("expression", expression);

      _expression = expression;
#if !NET_3_5
      _type = type ?? expression.Type;
#endif

      Assertion.IsTrue (CanReduce);
    }

#if !NET_3_5
    public override Type Type
    {
      get { return _type; }
    }

    public override ExpressionType NodeType
    {
      get { return ExpressionType.Extension; }
    }
#endif

    public Expression Expression
    {
      get { return _expression; }
    }

    public override bool CanReduce
    {
      get
      {
        return true;
      }
    }

    public override Expression Reduce ()
    {
      return _expression;
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      var result = visitor.Visit (_expression);
      if (result != _expression)
        return new ReducibleExtensionExpression (result);

      return this;
    }

    public override string ToString ()
    {
      return "Reducible(" + _expression.BuildString() + ")";
    }
  }
}