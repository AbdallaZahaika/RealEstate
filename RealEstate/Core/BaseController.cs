using RealEstate.DBContext;
using RealEstate.Models;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.Text.RegularExpressions;
using System.Drawing;
using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;

namespace RealEstate.Core
{
    public class BaseController : Controller
    {
        protected readonly IConfiguration _configuration;
        protected readonly AppDbContext _dbContext;
        public BaseController(IConfiguration configuration, AppDbContext dbContext)
        {
            _configuration = configuration;
            _dbContext = dbContext;
        }

        protected bool CheckValidateUserName(string userName)
        {
            if (!string.IsNullOrEmpty(userName) && userName is string)
            {
                string userNameRegex = "^[A-Za-z][A-Za-z0-9_]{2,31}$";

                // Use the Regex.IsMatch method to test the userName against the regex
                if (Regex.IsMatch(userName, userNameRegex))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected bool CheckIsraeliPhoneNumber(string phoneNumber)
        {
            if (!string.IsNullOrEmpty(phoneNumber) && phoneNumber is string)
            {
                string phoneRegex = @"^(\+972|0)[\-]?([23489]{1}\d{7}|[5]{1}\d{8})$";

                // Use the Regex.IsMatch method to test the phoneNumber against the regex
                if (Regex.IsMatch(phoneNumber, phoneRegex))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }
        protected bool CheckValidateEmail(string email)
        {
            if (!string.IsNullOrEmpty(email) && email is string)
            {
                string emailRegex = @"^(([^<>()[\]\\.,;:\s@\""]+(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$";

                // Use the Regex.IsMatch method to test the email against the regex
                if (Regex.IsMatch(email, emailRegex))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected bool CheckValidatePassword(string password)
        {
            if (!string.IsNullOrEmpty(password) && password is string)
            {
                string passwordRegex = @"^(?=.*\d)(?=.*[a-z])(?=.*[A-Z])[a-zA-Z0-9]{1,100}$";

                // Use the Regex.IsMatch method to test the password against the regex
                if (Regex.IsMatch(password, passwordRegex))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                return false;
            }
        }

        protected User? GetUser()
        {
            var userJson = HttpContext.Session.GetString("user");

            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<User>(userJson);

                if (user != null)
                {
                    return user;
                }
            }
            return null;
        }

        protected int GetUserId()
        {
            var userJson = HttpContext.Session.GetString("user");

            if (!string.IsNullOrEmpty(userJson))
            {
                var user = JsonConvert.DeserializeObject<User>(userJson);

                if (user != null)
                {
                    return user.Id;
                }
            }
            return 0;
        }
        protected string GetUserName(int id)
        {
            User? RowData = _dbContext.Users.Where(row => row.Id == id && row.IsDeleted == 0).FirstOrDefault();

            if (RowData != null)
            {
                return RowData.Name;
            }
            return "";
        }
        protected string GetPropertyName(int id)
        {
            Property? RowData = _dbContext.Properties.Where(row => row.Id == id && row.IsDeleted == 0).FirstOrDefault();

            if (RowData != null)
            {
                return RowData.Title;
            }
            return "";
        }
        protected string GetCityName(int id)
        {
            City? RowData = _dbContext.Cities.Where(row => row.Id == id && row.IsDeleted == 0).FirstOrDefault();

            if (RowData != null)
            {
                return RowData.Title;
            }
            return "";
        }
        protected string GetRegionName(int id)
        {
            Models.Region? RowData = _dbContext.Regions.Where(region => region.Id == id && region.IsDeleted == 0).FirstOrDefault();

            if (RowData != null)
            {
                return RowData.Title;
            }
            return "";
        }
    }


}
