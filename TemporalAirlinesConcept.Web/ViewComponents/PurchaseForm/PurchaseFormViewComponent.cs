using Microsoft.AspNetCore.Mvc;

namespace TemporalAirlinesConcept.Web.ViewComponents.PurchaseForm;

public class PurchaseFormViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(PurchaseFormViewModel? model)
    {
        model ??= new PurchaseFormViewModel();

        return View(model);
    }
}