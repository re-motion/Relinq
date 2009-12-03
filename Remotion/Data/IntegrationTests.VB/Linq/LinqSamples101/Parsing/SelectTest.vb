Imports NUnit.Framework
Imports Remotion.Data.Linq.UnitTests.IntegrationTests
Imports Remotion.Data.Linq.UnitTests.IntegrationTests.LinqSamples101.TestDomain

Namespace Remotion.Data.IntegrationTests.VB.Linq.LinqSamples101.Parsing

  ''' <summary>
  ''' http://msdn.microsoft.com/en-us/bb737936.aspx
  ''' </summary>
  <TestFixture()> _
  Public Class SelectTest
    Inherits TestBase

    <Test()> _
    Public Sub Test_Simple()
      CheckParsedQuery( _
          (Function() From c In QuerySource.Customers Select c.ContactName), _
          "from Customer c in Customers select [c].ContactName")
    End Sub

  End Class

End Namespace
