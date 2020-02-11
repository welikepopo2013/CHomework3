using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Web;
using System.Web.Mvc;
using CHomework2.Models;

namespace CHomework2.Controllers
{
    public class HomeController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();
        // GET: Home
        public ActionResult Index()
        {
            //to check if the user get into the page through the log in function if not redirect them back to login page
            if (Session["LoginID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                
                try
                {
                    int loginAccount = (int)Session["LoginID"];
                    LoginInfo loginInfo = new LoginInfo();
                    loginInfo = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
                    return View(loginInfo);

                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                    return RedirectToAction("Index", "Login");
                }
            }
        }

        //to access the password change page with the correct login user info 
        public ActionResult ChangePassword()
        {
            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            loginInfo = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            loginInfo.LoginPassword = "";
            return View("ChangePassword", loginInfo);
        }

        //delete the session when user logout to prevent user access the main page without login 
        public ActionResult Logout()
        {
            Session.Abandon();
            return RedirectToAction("Index", "Login");
        }

        [HttpPost]
        public ActionResult ModifyPassword(LoginInfo loginModel)
        {
            int loginAccount = (int)Session["LoginID"];
            string LoginName = (string)Session["LoginName"];

            try
            {
                var md5_password = md5Encrypt(loginModel.LoginPassword);

                var loginDetails = db.LoginInfoes.Where(x => x.LoginID == loginAccount).FirstOrDefault();
                if (loginDetails.LoginPassword != md5_password)
                {
                    loginModel = loginDetails;
                    loginModel.ModifyMessage = "Wrong password.";
                }
                else if (loginModel.NewPassword1 == null)
                {
                    loginModel = loginDetails;
                    loginModel.ModifyMessage = "New Password is required";
                }
                else if (loginModel.LoginPassword == loginModel.NewPassword1)
                {
                    loginModel = loginDetails;
                    loginModel.ModifyMessage = "Please enter a different password";
                }
                else if (loginModel.NewPassword1 != loginModel.NewPassword2)
                {
                    loginModel = loginDetails;
                    loginModel.ModifyMessage = "New Password doesn't match";
                }
                else if (loginDetails.LoginPassword == md5_password)
                {
                    LoginInfo LoginInfo_to_update = db.LoginInfoes.SingleOrDefault(u => u.LoginName == LoginName);

                    var md5_newpassword = md5Encrypt(loginModel.NewPassword1);

                    LoginInfo_to_update.LoginPassword = md5_newpassword;
                    db.SaveChanges();

                    loginModel = loginDetails;
                    loginModel.LoginPassword = "";
                    loginModel.ModifyMessage = "Password Changed";
                    return View("ChangePassword", loginModel);
                }
                loginModel.LoginPassword = "";
                return View("ChangePassword", loginModel);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            loginModel = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            loginModel.LoginPassword = "";
            return View("ChangePassword", loginModel);
        }

        //MD5 Encryption
        public String md5Encrypt(String text)
        {
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(text));
            byte[] result = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            var md5_text = strBuilder.ToString();
            return md5_text;
        }
    }
}