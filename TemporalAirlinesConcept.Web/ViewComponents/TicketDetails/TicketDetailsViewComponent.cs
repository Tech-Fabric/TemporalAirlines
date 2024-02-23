using Microsoft.AspNetCore.Mvc;

namespace TemporalAirlinesConcept.Web.ViewComponents.TicketDetails;

public class TicketDetailsViewComponent : ViewComponent
{
    public async Task<IViewComponentResult> InvokeAsync(TicketDetailsViewModel? model)
    {
        model ??= new TicketDetailsViewModel();

        return View(model);
    }
}