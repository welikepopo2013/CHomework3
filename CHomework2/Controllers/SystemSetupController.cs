using CHomework2.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;

namespace CHomework2.Controllers
{
    public class SystemSetupController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();
        // GET: SystemSetup
        public ActionResult Index()
        {
            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            loginInfo = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            return View(loginInfo);
        }
    }
}