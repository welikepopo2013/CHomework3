using CHomework2.Models;
using System;
using System.Linq;
using System.Web.Mvc;

namespace CHomework2.Controllers
{
    public class SystemSetupController : Controller
    {
        Trainee13Entities1 db = new Trainee13Entities1();
        // GET: SystemSetup
        public ActionResult Index()
        {
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
    }
}