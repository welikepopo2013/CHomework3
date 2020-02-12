using CHomework2.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using System.Security.Cryptography;
using System.Text;
using System.Web.UI.HtmlControls;
using System.Web.UI.WebControls;
using System.Data;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using NPOI.HSSF.UserModel;
using System.IO;

namespace CHomework2.Controllers
{
    public class UserMaintenanceController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();

        // GET: UserMaintenance
        public ActionResult Index()
        {
            var viewModel = new User();
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = new List<User>();
            try
            {
                int loginAccount = (int)Session["LoginID"];
                viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
                return View(viewModel);
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                return RedirectToAction("Index", "Login");
            }
        }

        [HttpPost]
        public ActionResult function(FormCollection form, object sender, EventArgs e)
        {
            var button = form["button"];
            var viewModel = new User();
            int loginAccount = (int)Session["LoginID"];

            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Users.ToList();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            if (button == "Query")
            {
                viewModel = query(form);
                return View("Index", viewModel);
            }
            else if (button == "Add")
            {
                return View("Add", viewModel);
            }
            else if (button == "Modify")
            {
                if (select_user(form).Count() == 1)
                {
                    viewModel.ListB = select_user(form);
                    return View("Modify", viewModel);
                }
                else if (select_user(form).Count() == 0)
                {
                    TempData["SelectionMessage"] = "Plese select atleast one user to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    TempData["SelectionMessage"] = "Plese modify one user at a time.";
                    return View("Index", viewModel);
                }

            }
            else if (button == "Delete")
            {
                viewModel = delete(form);
                db.SaveChanges();

                return View("Index", viewModel);
            }
            else if (button == "Download")
            {
                Submit(sender, e);
                viewModel = query(form);
                return View("Index", viewModel);
            }
            return View(viewModel);
        }

        public User query(FormCollection form)
        {
            var viewModel = new User();
            String searchNames = form["userName"];
            var roleSelect = form["SelectRole"];
            int loginAccount = (int)Session["LoginID"];

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            viewModel.ListA = db.Roles.ToList();

            try
            {
                if (searchNames != "" && roleSelect != "All")
                {
                    viewModel.ListB = db.Users.Where(u => u.UserName.Contains(searchNames) && u.Role.RoleName == roleSelect).ToList();
                }
                if (searchNames == "" && roleSelect == "All")
                {
                    viewModel.ListB = db.Users.ToList();
                }
                if (searchNames != "" && roleSelect == "All")
                {
                    viewModel.ListB = db.Users.Where(u => u.UserName.Contains(searchNames)).ToList();
                }
                if (searchNames == "" && roleSelect != "All")
                {
                    viewModel.ListB = db.Users.Where(u => u.Role.RoleName == roleSelect).ToList();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                viewModel.ListB = new List<User>();
            }
            return viewModel;
        }

        public ActionResult Add(FormCollection form, User user)
        {
            var viewModel = new User();
            List<LoginInfo> existLogin = db.LoginInfoes.ToList();

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();
            viewModel.ListA = db.Roles.ToList();

            try
            {
                var selectRole = form["SelectRole"];
                var md5_password = md5Encrypt(form["LoginPassword"]);

                foreach (var validation in existLogin)
                {
                    if (form["LoginAccount"] == validation.LoginName)
                    {
                        viewModel.ListB = db.Users.ToList();
                        viewModel.ModifyMessage = "Login Account already exist please choose another one.";
                        return View(viewModel);
                    }
                }

                user.UserName = form["userName"];
                user.RoleID = db.Roles.Where(r => r.RoleName == selectRole).FirstOrDefault().RoleID;
                if (form["SelectStatus"] == "Y")
                {
                    user.Status = 1;
                }
                else { user.Status = 0; }
                user.CreateDate = DateTime.Now;
                user.CreateUser = userName;
                user.ModifyDate = DateTime.Now;
                user.ModifyUser = userName;

                db.Users.Add(user);
                LoginInfo account = new LoginInfo();
                account.LoginName = form["LoginAccount"];
                account.LoginPassword = md5_password;
                account.UserID = user.UserID;
                db.LoginInfoes.Add(account);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListB = db.Users.ToList();
            return View("Index", viewModel);
        }

        public List<User> select_user(FormCollection form)
        {
            List<User> listCheck = new List<User>();
            try
            {
                var checkList = new List<String>();
                if (form["checkbox"] != null)
                {
                    checkList = form["checkbox"].Split(',').ToList();
                }

                foreach (var user in checkList)
                {
                    int int_user = Int32.Parse(user);
                    listCheck.Add(db.Users.Where(u => u.UserID == int_user).FirstOrDefault());
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            return listCheck;
        }

        public ActionResult Modify(FormCollection form, User user)
        {
            var viewModel = new User();
            int loginID = (int)Session["LoginID"];
            var userInfo = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault().User;
            var userName = userInfo.UserID;
            var userID = int.Parse(form["userID"]);
            var selectRole = form["SelectRole"];

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();
            viewModel.ListA = db.Roles.ToList();

            try {
                User Update_User = db.Users.SingleOrDefault(u => u.UserID == userID);
                Update_User.UserName = form["userName"];
                Update_User.RoleID = db.Roles.Where(r => r.RoleName == selectRole).FirstOrDefault().RoleID;
                if (form["SelectStatus"] == "Y")
                {
                    Update_User.Status = 1;
                }
                else { Update_User.Status = 0; }
                Update_User.ModifyDate = DateTime.Now;
                Update_User.ModifyUser = userName;
                db.SaveChanges();
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListB = db.Users.ToList();
            return View("Index", viewModel);
        }

        public User delete(FormCollection form)
        {
            var viewModel = new User();
            int loginAccount = (int)Session["LoginID"];

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            viewModel.ListA = db.Roles.ToList();

            try {
                foreach (var user in select_user(form))
                {
                    User dropUser = db.Users.Where(u => u.UserID == user.UserID).FirstOrDefault();
                    LoginInfo dropAccount = db.LoginInfoes.Where(l => l.User.UserID == dropUser.UserID).FirstOrDefault();
                    db.Users.Remove(dropUser);
                    db.LoginInfoes.Remove(dropAccount);
                    db.SaveChanges();
                }
            }
            catch (Exception e) {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListB = db.Users.ToList();
            return viewModel;
        }

        public void Submit(object sender, EventArgs e)
        {
            string UserJSON = Request.Form["UserJSON"];
            System.Diagnostics.Debug.WriteLine(UserJSON);
            try
            {
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(UserJSON);
                IWorkbook workbook;
                workbook = new XSSFWorkbook();
                ISheet sheet1 = workbook.CreateSheet("Sheet 1");
                //make a header row
                IRow row1 = sheet1.CreateRow(0);
                for (int j = 0; j < dt.Columns.Count; j++)
                {

                    ICell cell = row1.CreateCell(j);
                    String columnName = dt.Columns[j].ToString();
                    cell.SetCellValue(columnName);
                }
                //loops through data
                for (int i = 0; i < dt.Rows.Count; i++)
                {
                    IRow row = sheet1.CreateRow(i + 1);
                    for (int j = 0; j < dt.Columns.Count; j++)
                    {

                        ICell cell = row.CreateCell(j);
                        String columnName = dt.Columns[j].ToString();
                        cell.SetCellValue(dt.Rows[i][columnName].ToString());
                    }
                }
                using (var exportData = new MemoryStream())
                {
                    Response.Clear();
                    workbook.Write(exportData);
                    Response.ContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "UserNPOI.xlsx"));
                    Response.BinaryWrite(exportData.ToArray());
                    Response.End();
                }
            }
            catch (Exception error) { }
        }
        //MD5 Encryption
        public String md5Encrypt(String text)
        {
            try
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
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                return text;
            }
        }
    }
}