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
using Remotion.Linq.Parsing;
using Remotion.Linq.Utilities;

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
  public abstract class ExtensionExpression : Expression
  {
    private readonly Type _type;
    private readonly ExpressionType _nodeType;

    /// <summary>
    /// Defines a standard <see cref="ExpressionType"/> value that is used by all <see cref="ExtensionExpression"/> subclasses unless they specify
    /// their own <see cref="ExpressionType"/> value.
    /// </summary>
    public const ExpressionType DefaultExtensionExpressionNodeType = (ExpressionType) 150000;

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
    {
      ArgumentUtility.CheckNotNull ("type", type);
      
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

    ///// <summary>
    ///// Gets a value indicating whether this instance can be reduced to a tree of standard expressions.
    ///// </summary>
    ///// <value>
    ///// 	<see langword="true"/> if this instance can be reduced; otherwise, <see langword="false"/>.
    ///// </value>
    ///// <remarks>
    ///// <para>
    ///// If this method returns <see langword="true"/>, the <see cref="Reduce"/> method can be called in order to produce a new 
    ///// <see cref="Expression"/> that has the same semantics as this <see cref="ExtensionExpression"/> but consists of 
    ///// expressions of standard node types.
    ///// </para>
    ///// <para>
    ///// Subclasses overriding the <see cref="CanReduce"/> property to return <see langword="true" /> must also override the <see cref="Reduce"/> 
    ///// method and cannot call its base implementation.
    ///// </para>
    ///// </remarks>
    //public virtual bool CanReduce
    //{
    //  get { return false; }
    //}

    ///// <summary>
    ///// Reduces this instance to a tree of standard expressions. If this instance cannot be reduced, the same <see cref="ExtensionExpression"/>
    ///// is returned.
    ///// </summary>
    ///// <returns>If <see cref="CanReduce"/> is <see langword="true" />, a reduced version of this <see cref="ExtensionExpression"/>; otherwise,
    ///// this <see cref="ExtensionExpression"/>.</returns>
    ///// <remarks>
    ///// <para>
    ///// This method can be called in order to produce a new <see cref="Expression"/> that has the same semantics as this 
    ///// <see cref="ExtensionExpression"/> but consists of expressions of standard node types. The reduction need not be complete, nodes can be 
    ///// returned that themselves must be reduced.
    ///// </para>
    ///// <para>
    ///// Subclasses overriding the <see cref="CanReduce"/> property to return <see langword="true" /> must also override this method and cannot
    ///// call the base implementation.
    ///// </para>
    ///// </remarks>
    //public virtual Expression Reduce ()
    //{
    //  if (CanReduce)
    //    throw new InvalidOperationException ("Reducible nodes must override the Reduce method.");
    //  return this;
    //}

    ///// <summary>
    ///// Calls the <see cref="Reduce"/> method and checks certain invariants before returning the result. This method can only be called when
    ///// <see cref="CanReduce"/> returns <see langword="true" />.
    ///// </summary>
    ///// <returns>A reduced version of this <see cref="ExtensionExpression"/>.</returns>
    ///// <exception cref="InvalidOperationException">This <see cref="ExtensionExpression"/> is not reducible - or - the <see cref="Reduce"/> method 
    ///// violated one of the invariants (see Remarks).</exception>
    ///// <remarks>
    /////   This method checks the following invariants:
    /////   <list type="bullet">
    /////     <item><see cref="Reduce"/> must not return <see langword="null" />.</item>
    /////     <item><see cref="Reduce"/> must not return the original <see cref="ExtensionExpression"/>.</item>
    /////     <item>
    /////       The new expression returned by <see cref="Reduce"/> must be assignment-compatible with the type of the original 
    /////       <see cref="ExtensionExpression"/>.
    /////     </item>
    /////   </list>
    /////   </remarks>
    //public Expression ReduceAndCheck ()
    //{
    //  if (!CanReduce)
    //    throw new InvalidOperationException ("Reduce and check can only be called on reducible nodes.");

    //  var result = Reduce();
      
    //  if (result == null)
    //    throw new InvalidOperationException ("Reduce cannot return null.");
    //  if (result == this)
    //    throw new InvalidOperationException ("Reduce cannot return the original expression.");
    //  if (!Type.IsAssignableFrom (result.Type))
    //    throw new InvalidOperationException ("Reduce must produce an expression of a compatible type.");

    //  return result;
    //}

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