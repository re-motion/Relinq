Imports NUnit.Framework
Imports Remotion.Data.Linq.UnitTests.IntegrationTests
Imports Remotion.Data.Linq.UnitTests.IntegrationTests.LinqSamples101.TestDomain

Namespace Remotion.Data.IntegrationTests.VB.Linq.LinqSamples101.Parsing

  ''' <summary>
  ''' http://msdn.microsoft.com/en-us/bb737944.aspx
  ''' </summary>
  <TestFixture()> _
  Public Class WhereTest
    Inherits TestBase

    <Test()> _
    <Ignore("Should be 'from Customer c in Customers where ([c].City = ""London"") select [c]'")> _
    Public Sub Test_01()
      CheckParsedQuery( _
          (Function() From c In QuerySource.Customers Where c.City = "London"), _
          "from Customer c in Customers where (CompareString([c].City, ""London"", False) = 0) select [c]")
    End Sub

  End Class

End Namespace
