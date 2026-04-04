using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using SV22T1020261.Models.Security;
using SV22T1020261.Shop;

public class CustomerAuthorizeAttribute : ActionFilterAttribute
{
    public override void OnActionExecuting(ActionExecutingContext context)
    {
        var customer = ApplicationContext.GetSessionData<CustomerAccount>(
            ApplicationContext.CustomerSessionKey);

        if (customer == null)
        {
            context.Result = new RedirectToActionResult(
                "Login",
                "Account",
                new { ReturnUrl = context.HttpContext.Request.Path }
            );
        }
    }
}