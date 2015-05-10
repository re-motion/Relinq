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
using Remotion.Linq.Parsing;
#endif

namespace Remotion.Linq.UnitTests.Clauses.Expressions.TestDomain
{
  public class ReducibleExtensionExpression
#if !NET_3_5
    : Expression
#else
    : ExtensionExpression
#endif
  {
#if !NET_3_5
    private readonly Type _type;

    public ReducibleExtensionExpression (Type type)
    {
      _type = type;
    }
#else
    public ReducibleExtensionExpression (Type type)
        : base(type)
    {
    }
#endif

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

    public override bool CanReduce
    {
      get { return true; }
    }

    public override Expression Reduce ()
    {
      return Constant (0);
    }

    protected override Expression VisitChildren (ExpressionVisitor visitor)
    {
      return this;
    }
  }
}