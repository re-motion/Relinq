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
using Remotion.Linq.Parsing;
using Remotion.Utilities;

namespace Remotion.Linq.Clauses.Expressions
{
  /// <summary>
  /// Acts as a base class for custom extension expressions, providing advanced visitor support. Also allows extension expressions to be reduced to 
  /// a tree of standard expressions with equivalent semantics.
  /// </summary>
  /// <remarks>
  /// Custom extension expressions can specify their own <see cref="ExpressionType"/> or use a default one. re-linq reserves 
  /// <see cref="ExpressionType"/> values from 100000 to 150000 for its own expressions. Custom LINQ providers can use 150001 and above.
  /// </remarks>
  public abstract partial class ExtensionExpression : Expression
  {
    /// <summary>
    /// Defines a standard <see cref="ExpressionType"/> value that is used by all <see cref="ExtensionExpression"/> subclasses unless they specify
    /// their own <see cref="ExpressionType"/> value.
    /// </summary>
    public const ExpressionType DefaultExtensionExpressionNodeType = (ExpressionType) 150000;

#if !NET_3_5
    private readonly Type _type;
    private readonly ExpressionType _nodeType;
#endif

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionExpression"/> class with a default <see cref="ExpressionType"/> value.
    /// </summary>
    /// <param name="type">The type of the value represented by the <see cref="ExtensionExpression"/>.</param>
    protected ExtensionExpression (Type type)
        : this (ArgumentUtility.CheckNotNull ("", type), DefaultExtensionExpressionNodeType)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ExtensionExpression"/> class with a custom <see cref="ExpressionType"/> value.
    /// </summary>
    /// <param name="type">The type of the value represented by the <see cref="ExtensionExpression"/>.</param>
    /// <param name="nodeType">The <see cref="ExpressionType"/> value to use as this expression's <see cref="Expression.NodeType"/> value.
    /// LINQ providers should use values starting from 150001 and above.</param>
    protected ExtensionExpression (Type type, ExpressionType nodeType)
#if NET_3_5
        : base (nodeType, ArgumentUtility.CheckNotNull ("", type))
#endif
    {
      ArgumentUtility.CheckNotNull ("type", type);

#if !NET_3_5
      _type = type;
      _nodeType = nodeType;
#endif
    }

#if !NET_3_5
    public override ExpressionType NodeType
    {
      get { return _nodeType; }
    }

    public override Type Type
    {
      get { return _type; }
    }
#endif

    /// <summary>
    /// Accepts the specified visitor, by default dispatching to <see cref="ExpressionTreeVisitor.VisitExtensionExpression"/>. 
    /// Inheritors of the <see cref="ExtensionExpression"/> class can override this method in order to dispatch to a specific Visit method.
    /// </summary>
    /// <param name="visitor">The visitor whose Visit method should be invoked.</param>
    /// <returns>The <see cref="Expression"/> returned by the visitor.</returns>
    /// <remarks>
    /// Overriders can test the <paramref name="visitor"/> for a specific interface. If the visitor supports the interface, the extension expression 
    /// can dispatch to the respective strongly-typed Visit method declared in the interface. If it does not, the extension expression should call 
    /// the base implementation of <see cref="Accept"/>, which will dispatch to <see cref="ExpressionTreeVisitor.VisitExtensionExpression"/>.
    /// </remarks>
    public virtual Expression Accept (ExpressionTreeVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);
      
      return visitor.VisitExtensionExpression (this);
    }

    /// <summary>
    /// Must be overridden by <see cref="ExtensionExpression"/> subclasses by calling <see cref="ExpressionTreeVisitor.VisitExpression"/> on all 
    /// children of this extension node. 
    /// </summary>
    /// <param name="visitor">The visitor to visit the child nodes with.</param>
    /// <returns>This <see cref="ExtensionExpression"/>, or an expression that should replace it in the surrounding tree.</returns>
    /// <remarks>
    /// If the visitor replaces any of the child nodes, a new <see cref="ExtensionExpression"/> instance should
    /// be returned holding the new child nodes. If the node has no children or the visitor does not replace any child node, the method should
    /// return this <see cref="ExtensionExpression"/>. 
    /// </remarks>
    protected internal abstract Expression VisitChildren (ExpressionTreeVisitor visitor);
  }
}