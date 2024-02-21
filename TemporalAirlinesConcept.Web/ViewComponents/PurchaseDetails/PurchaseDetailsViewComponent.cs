using Microsoft.AspNetCore.Mvc;

namespace TemporalAirlinesConcept.Web.ViewComponents.PurchaseDetails;

public class PurchaseDetailsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(PurchaseDetailsViewModel? model)
    {
        model ??= new PurchaseDetailsViewModel();

        return View(model);
    }
}