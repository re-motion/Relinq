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
  public abstract partial class ExtensionExpression
  {
    /// <summary>
    /// Gets a value indicating whether this instance can be reduced to a tree of standard expressions.
    /// </summary>
    /// <value>
    /// 	<see langword="true"/> if this instance can be reduced; otherwise, <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// If this method returns <see langword="true"/>, the <see cref="Reduce"/> method can be called in order to produce a new 
    /// <see cref="Expression"/> that has the same semantics as this <see cref="ExtensionExpression"/> but consists of 
    /// expressions of standard node types.
    /// </para>
    /// <para>
    /// Subclasses overriding the <see cref="CanReduce"/> property to return <see langword="true" /> must also override the <see cref="Reduce"/> 
    /// method and cannot call its base implementation.
    /// </para>
    /// </remarks>
    public virtual bool CanReduce
    {
      get { return false; }
    }

    /// <summary>
    /// Reduces this instance to a tree of standard expressions. If this instance cannot be reduced, the same <see cref="ExtensionExpression"/>
    /// is returned.
    /// </summary>
    /// <returns>If <see cref="CanReduce"/> is <see langword="true" />, a reduced version of this <see cref="ExtensionExpression"/>; otherwise,
    /// this <see cref="ExtensionExpression"/>.</returns>
    /// <remarks>
    /// <para>
    /// This method can be called in order to produce a new <see cref="Expression"/> that has the same semantics as this 
    /// <see cref="ExtensionExpression"/> but consists of expressions of standard node types. The reduction need not be complete, nodes can be 
    /// returned that themselves must be reduced.
    /// </para>
    /// <para>
    /// Subclasses overriding the <see cref="CanReduce"/> property to return <see langword="true" /> must also override this method and cannot
    /// call the base implementation.
    /// </para>
    /// </remarks>
    public virtual Expression Reduce ()
    {
      if (CanReduce)
        throw new InvalidOperationException ("Reducible nodes must override the Reduce method.");
      return this;
    }

    /// <summary>
    /// Calls the <see cref="Reduce"/> method and checks certain invariants before returning the result. This method can only be called when
    /// <see cref="CanReduce"/> returns <see langword="true" />.
    /// </summary>
    /// <returns>A reduced version of this <see cref="ExtensionExpression"/>.</returns>
    /// <exception cref="InvalidOperationException">This <see cref="ExtensionExpression"/> is not reducible - or - the <see cref="Reduce"/> method 
    /// violated one of the invariants (see Remarks).</exception>
    /// <remarks>
    ///   This method checks the following invariants:
    ///   <list type="bullet">
    ///     <item><see cref="Reduce"/> must not return <see langword="null" />.</item>
    ///     <item><see cref="Reduce"/> must not return the original <see cref="ExtensionExpression"/>.</item>
    ///     <item>
    ///       The new expression returned by <see cref="Reduce"/> must be assignment-compatible with the type of the original 
    ///       <see cref="ExtensionExpression"/>.
    ///     </item>
    ///   </list>
    ///   </remarks>
    public Expression ReduceAndCheck ()
    {
      if (!CanReduce)
        throw new InvalidOperationException ("Reduce and check can only be called on reducible nodes.");

      var result = Reduce();
      
      if (result == null)
        throw new InvalidOperationException ("Reduce cannot return null.");
      if (result == this)
        throw new InvalidOperationException ("Reduce cannot return the original expression.");
      if (!Type.IsAssignableFrom (result.Type))
        throw new InvalidOperationException ("Reduce must produce an expression of a compatible type.");

      return result;
    }

    /// <summary>
    /// Accepts the specified visitor, by default dispatching to <see cref="ExpressionVisitor.VisitExtension"/>. 
    /// Inheritors of the <see cref="ExtensionExpression"/> class can override this method in order to dispatch to a specific Visit method.
    /// </summary>
    /// <param name="visitor">The visitor whose Visit method should be invoked.</param>
    /// <returns>The <see cref="Expression"/> returned by the visitor.</returns>
    /// <remarks>
    /// Overriders can test the <paramref name="visitor"/> for a specific interface. If the visitor supports the interface, the extension expression 
    /// can dispatch to the respective strongly-typed Visit method declared in the interface. If it does not, the extension expression should call 
    /// the base implementation of <see cref="Accept"/>, which will dispatch to <see cref="ExpressionVisitor.VisitExtension"/>.
    /// </remarks>
    protected virtual Expression Accept (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return visitor.VisitExtension (this);
    }

    internal Expression AcceptInternal (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return Accept (visitor);
    }

    internal Expression VisitChildrenInternal (ExpressionVisitor visitor)
    {
      ArgumentUtility.CheckNotNull ("visitor", visitor);

      return VisitChildren (visitor);
    }
  }
}