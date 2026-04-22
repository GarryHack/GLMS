using GLMS.Web.Models;

namespace GLMS.Tests;

public class ContractWorkflowTests
{
    private static bool CanCreateServiceRequest(ContractStatus status) =>
        status != ContractStatus.Expired && status != ContractStatus.OnHold;

    [Fact]
    public void ActiveContract_CanCreateServiceRequest()
    {
        Assert.True(CanCreateServiceRequest(ContractStatus.Active));
    }

    [Fact]
    public void ExpiredContract_CannotCreateServiceRequest()
    {
        Assert.False(CanCreateServiceRequest(ContractStatus.Expired));
    }

    [Fact]
    public void OnHoldContract_CannotCreateServiceRequest()
    {
        Assert.False(CanCreateServiceRequest(ContractStatus.OnHold));
    }

    [Fact]
    public void DraftContract_CanCreateServiceRequest()
    {
        Assert.True(CanCreateServiceRequest(ContractStatus.Draft));
    }

    [Theory]
    [InlineData(ContractStatus.Active,  true)]
    [InlineData(ContractStatus.Draft,   true)]
    [InlineData(ContractStatus.Expired, false)]
    [InlineData(ContractStatus.OnHold,  false)]
    public void ContractStatus_DeterminesServiceRequestEligibility(
        ContractStatus status, bool expectedEligible)
    {
        Assert.Equal(expectedEligible, CanCreateServiceRequest(status));
    }
}
