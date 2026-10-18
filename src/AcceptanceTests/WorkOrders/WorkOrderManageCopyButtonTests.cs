using System.Text.RegularExpressions;
using ClearMeasure.Bootcamp.AcceptanceTests.Extensions;
using ClearMeasure.Bootcamp.Core.Queries;
using ClearMeasure.Bootcamp.UI.Shared.Pages;
using Microsoft.Playwright;

namespace ClearMeasure.Bootcamp.AcceptanceTests.WorkOrders;

public class WorkOrderManageCopyButtonTests : AcceptanceTestBase
{
    [Test, Retry(2)]
    public async Task ShouldRenderCopyButton_WhenViewingExistingWorkOrder()
    {
        await LoginAsCurrentUser();

        var order = await CreateAndSaveNewWorkOrder();

        await ClickWorkOrderNumberFromSearchPage(order);

        var copyButton = Page.GetByTestId(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));
        await Expect(copyButton).ToBeVisibleAsync();
        await Expect(copyButton).ToHaveAttributeAsync("aria-label", "Copy work order number");
    }

    [Test, Retry(2)]
    public async Task ShouldCopyWorkOrderNumber_ToClipboard()
    {
        await LoginAsCurrentUser();

        var order = await CreateAndSaveNewWorkOrder();

        await ClickWorkOrderNumberFromSearchPage(order);

        var copyButton = Page.GetByTestId(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));
        await Expect(copyButton).ToBeVisibleAsync();

        await Click(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));

        // The copy runs in a JavaScript module the page loads on first use: on a deployed app that takes longer than
        // a fixed half second, so wait for the clipboard to hold the number (up to ten seconds).
        var clipboardText = string.Empty;
        for (var attempt = 0; attempt < 40 && clipboardText != order.Number; attempt++)
        {
            await Task.Delay(250);
            clipboardText = await Page.EvaluateAsync<string>("navigator.clipboard.readText()");
        }

        clipboardText.ShouldBe(order.Number);
    }

    [Test, Retry(2)]
    public async Task ShouldShowCheckIconAndAriaLiveMessage_OnCopySuccess()
    {
        await LoginAsCurrentUser();

        var order = await CreateAndSaveNewWorkOrder();

        await ClickWorkOrderNumberFromSearchPage(order);

        var copyButton = Page.GetByTestId(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));
        await Expect(copyButton).ToBeVisibleAsync();

        await Click(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));

        // Check that icon changed to check2
        var icon = copyButton.Locator("i");
        await Expect(icon).ToHaveClassAsync(new Regex("bi-check2"));

        // Check aria-live message
        var statusRegion = Page.Locator("[role='status'][aria-live='polite']");
        await Expect(statusRegion).ToContainTextAsync("Copied to clipboard");

        // Wait for revert (2 seconds)
        await Task.Delay(2500);

        // Icon should revert to clipboard
        await Expect(icon).ToHaveClassAsync(new Regex("bi-clipboard"));
    }

    [Test, Retry(2)]
    public async Task ShouldNotRenderCopyButton_WhenCreatingNewWorkOrder()
    {
        await LoginAsCurrentUser();

        await Page.GotoAsync("/workorder/manage?mode=new");
        await WaitForNewWorkOrderFormReadyAsync();

        var copyButton = Page.GetByTestId(nameof(WorkOrderManage.Elements.CopyWorkOrderNumber));
        await Expect(copyButton).ToHaveCountAsync(0);
    }
}
