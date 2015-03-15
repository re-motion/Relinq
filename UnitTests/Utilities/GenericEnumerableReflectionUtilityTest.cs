using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq.Expressions;
using NUnit.Framework;
using Remotion.Linq.Utilities;

namespace Remotion.Linq.UnitTests.Utilities
{
  [TestFixture]
  public class GenericEnumerableReflectionUtilityTest
  {
    private class GenericWithIEnumerable<T> : ArrayList
    {
    }

    private class ClosedGenericList : List<int>
    {
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsArray ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (int[])), Is.SameAs (typeof (int)));
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (double[])), Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsArray_Strange ()
    {
      Expression<Func<int, IEnumerable<double>>> collectionSelector = x => new double[1];
      Assert.That (
          GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (collectionSelector.Body.Type),
          Is.SameAs (typeof (double)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (List<int>)), Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerableOnBaseClass ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (ClosedGenericList)), Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentIsIEnumerable ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<int>)), Is.SameAs (typeof (int)));
      Assert.That (
          GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (IEnumerable<IEnumerable<string>>)),
          Is.SameAs (typeof (IEnumerable<string>)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_NonGeneric_ReturnsNull ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (ArrayList)), Is.Null);
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_Generic_ReturnsNull ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (GenericWithIEnumerable<int>)), Is.Null);
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_ArgumentImplementsIEnumerable_BothGenericAndNonGeneric ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (int[])), Is.SameAs (typeof (int)));
    }

    [Test]
    public void TryGetItemTypeOfClosedGenericIEnumerable_InvalidType_ReturnsNull ()
    {
      Assert.That (GenericEnumerableReflectionUtility.TryGetItemTypeOfClosedGenericIEnumerable (typeof (int)), Is.Null);
    }
  }
}