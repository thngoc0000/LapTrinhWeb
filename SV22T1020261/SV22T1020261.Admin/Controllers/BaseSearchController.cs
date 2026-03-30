using Microsoft.AspNetCore.Mvc;
using SV22T1020261.Models.Common;

namespace SV22T1020261.Admin.Controllers
{
    public class BaseSearchController : Controller
    {
        protected PaginationSearchInput GetSearchInput(string key)
        {
            var input = ApplicationContext.GetSessionData<PaginationSearchInput>(key);
            if (input == null)
            {
                input = new PaginationSearchInput()
                {
                    Page = 1,
                    PageSize = ApplicationContext.PageSize,
                    SearchValue = ""
                };
            }
            return input;
        }

        protected void SaveSearchInput(string key, PaginationSearchInput input)
        {
            ApplicationContext.SetSessionData(key, input);
        }
    }
}
