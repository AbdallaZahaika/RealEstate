using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using RealEstate.DBContext;

namespace RealEstate.Core
{
    public class ProviderCoreController : BaseController
    {
        public ProviderCoreController(IConfiguration configuration, AppDbContext dbContext) : base(configuration, dbContext)
        {
        }
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var User = GetUser();
            if (User == null)
            {
                HttpContext.Session.Clear();
                context.Result = RedirectToAction("Login", "Account", new { area = "Customer" });
                return;
            }

            bool check_user = _dbContext.Users.Any(u => u.Id == User.Id && u.Level == 2 && u.Status == Models.User.StatusType.Active);
            if (!check_user)
            {
                context.Result = RedirectToAction("Login", "Account", new { area = "Customer" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
