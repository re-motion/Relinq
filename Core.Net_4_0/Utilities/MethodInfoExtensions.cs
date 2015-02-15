using System;
using JetBrains.Annotations;

// ReSharper disable once CheckNamespace
namespace System.Reflection
{
  internal static class MethodInfoExtensions
  {
    [NotNull]
    public static Delegate CreateDelegate ([NotNull] this MethodInfo methodInfo, [NotNull] Type delegateType, [NotNull] object target)
    {
      return Delegate.CreateDelegate (delegateType, target, methodInfo);
    }

    [NotNull] 
    public static MethodInfo GetMethodInfo ([NotNull] this Delegate @delegate)
    {
      return @delegate.Method;
    }
  }
}