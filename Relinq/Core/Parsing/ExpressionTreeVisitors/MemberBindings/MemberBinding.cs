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

namespace Remotion.Linq.Parsing.ExpressionTreeVisitors.MemberBindings
{
  /// <summary>
  /// Represents a <see cref="MemberInfo"/> being bound to an associated <see cref="Expression"/> instance. This is used by the 
  /// <see cref="TransparentIdentifierRemovingExpressionTreeVisitor"/> to represent assignments in constructor calls such as <c>new AnonymousType (a = 5)</c>, 
  /// where <c>a</c> is the member of <c>AnonymousType</c> and <c>5</c> is the associated expression.
  /// The <see cref="MatchesReadAccess"/> method can be used to check whether the member bound to an expression matches a given <see cref="MemberInfo"/>
  /// (considering read access). See the subclasses for details.
  /// </summary>
  public abstract class MemberBinding
  {
    public static MemberBinding Bind (MemberInfo boundMember, Expression associatedExpression)
    {
      ArgumentUtility.CheckNotNull ("boundMember", boundMember);
      ArgumentUtility.CheckNotNull ("associatedExpression", associatedExpression);

      var methodInfo = boundMember as MethodInfo;
      if (methodInfo != null)
        return new MethodInfoBinding (methodInfo, associatedExpression);

      var propertyInfo = boundMember as PropertyInfo;
      if (propertyInfo != null)
        return new PropertyInfoBinding (propertyInfo, associatedExpression);

      return new FieldInfoBinding ((FieldInfo) boundMember, associatedExpression);
    }

    private readonly MemberInfo _boundMember;
    private readonly Expression _associatedExpression;

    public MemberInfo BoundMember
    {
      get { return _boundMember; }
    }

    public Expression AssociatedExpression
    {
      get { return _associatedExpression; }
    }

    public MemberBinding (MemberInfo boundMember, Expression associatedExpression)
    {
      ArgumentUtility.CheckNotNull ("boundMember", boundMember);
      ArgumentUtility.CheckNotNull ("associatedExpression", associatedExpression);

      _boundMember = boundMember;
      _associatedExpression = associatedExpression;
    }

    public abstract bool MatchesReadAccess (MemberInfo member);
  }
}
