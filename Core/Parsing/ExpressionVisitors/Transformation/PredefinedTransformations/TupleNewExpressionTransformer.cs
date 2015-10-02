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
using System.Collections.ObjectModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Remotion.Linq.Parsing.ExpressionVisitors.Transformation.PredefinedTransformations
{
  /// <summary>
  /// Detects <see cref="NewExpression"/> nodes for the .NET tuple types and adds <see cref="MemberInfo"/> metadata to those nodes.
  /// This allows LINQ providers to match member access and constructor arguments more easily.
  /// </summary>
  public class TupleNewExpressionTransformer : MemberAddingNewExpressionTransformerBase
  {
    protected override bool CanAddMembers (Type instantiatedType, ReadOnlyCollection<Expression> arguments)
    {
      return instantiatedType.Namespace == "System" && instantiatedType.Name.StartsWith ("Tuple`");
    }

    protected override MemberInfo[] GetMembers (ConstructorInfo constructorInfo, ReadOnlyCollection<Expression> arguments)
    {
      return arguments.Select ((expr, i) => GetMemberForNewExpression (constructorInfo.DeclaringType, "Item" + (i + 1))).ToArray ();
    }
  }
}