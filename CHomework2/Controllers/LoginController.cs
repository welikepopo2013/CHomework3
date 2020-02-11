using System;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using CHomework2.Models;
using System.Security.Cryptography;
using System.Text;

namespace CHomework2.Controllers
{
    public class LoginController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();
        // GET: Login
        public ActionResult Index()
        {
            HttpCookie cookie = Request.Cookies["LoginInfo"];
            if (cookie != null)
            {
                try
                {
                    ViewBag.LoginName = cookie["LoginName"];
                    ViewBag.LoginPassword = cookie["LoginPassword"];
                }
                catch (Exception e)
                {
                    System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                    System.Diagnostics.Debug.WriteLine(e.ToString());
                    System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                    ViewBag.LoginName = "";
                    ViewBag.LoginPassword = "";
                }
            }
            return View();
        }

        [HttpPost]
        public ActionResult Autherize(LoginInfo loginModel)
        {
            HttpCookie cookie = new HttpCookie("LoginInfo");
            try
            {
                if (ModelState.IsValid == true)
                {
                    //remembering the login info and store it in cookie
                    if (loginModel.Remember == true)
                    {
                        cookie["LoginName"] = loginModel.LoginName;
                        cookie["LoginPassword"] = loginModel.LoginPassword;
                        cookie.Expires = DateTime.Now.AddDays(2);
                        HttpContext.Response.Cookies.Add(cookie);
                    }
                    else
                    {
                        cookie.Expires = DateTime.Now.AddDays(-1);
                        HttpContext.Response.Cookies.Add(cookie);
                    }
                    //to encrypt the password
                    var md5_password = md5Encrypt(loginModel.LoginPassword);

                    //to check the login info is correct or not
                    var loginDetails = db.LoginInfoes.Where(x => x.LoginName == loginModel.LoginName && x.LoginPassword == md5_password).FirstOrDefault();
                    if (loginDetails != null)
                    {
                        Session["LoginID"] = loginDetails.LoginID;
                        Session["LoginName"] = loginDetails.LoginName;
                        int important = loginDetails.UserID;
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        loginModel.LoginErrorMessage = "Wrong username or password.";
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            return View("Index", loginModel);
        }

        //MD5 Encryption
        public String md5Encrypt(String text) {
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