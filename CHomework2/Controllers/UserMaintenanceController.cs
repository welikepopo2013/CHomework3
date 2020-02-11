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
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            List<User> list = new List<User>();
            var viewModel = new User();
            viewModel.ListA = roleList;
            viewModel.ListB = list;

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            return View(viewModel);
        }
        User test = new User();

        [HttpPost]
        public ActionResult function(FormCollection form, object sender, EventArgs e)
        {
            var button = form["button"];
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            var viewModel = new User();
            String searchNames = form["userName"];
            var roleSelect = form["SelectRole"];

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            if (button == "Query")
            {
                viewModel = query(form);
                return View("Index", viewModel);
            }
            else if (button == "Add")
            {
                viewModel.ListA = roleList;
                viewModel.ListB = userList;
                return View("Add", viewModel);
            }
            else if (button == "Modify")
            {
                List<User> list = new List<User>();
                var checkList = new List<String>();
                if (form["checkbox"] != null)
                {
                    checkList = form["checkbox"].Split(',').ToList();
                }
                int intUser;
                foreach (var user in checkList)
                {
                    intUser = int.Parse(user);
                    list.Add(db.Users.Where(u => u.UserID == intUser).FirstOrDefault());
                }
                if (list.Count() == 1)
                {
                    viewModel.ListA = roleList;
                    viewModel.ListB = list;
                    return View("Modify", viewModel);
                }
                else if (list.Count() == 0)
                {
                    viewModel.ListA = roleList;
                    viewModel.ListB = userList;
                    TempData["SelectionMessage"] = "Plese select atleast one user to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    viewModel.ListA = roleList;
                    viewModel.ListB = userList;
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
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            var viewModel = new User();
            List<User> list = new List<User>();
            String searchNames = form["userName"];
            var roleSelect = form["SelectRole"];

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            if (searchNames != "" && roleSelect == "")
            {
                userList = db.Users.Where(u => u.UserName.Contains(searchNames)).ToList();
                viewModel.ListA = roleList;
                viewModel.ListB = userList;
            }
            else if (searchNames == "" && roleSelect != "")
            {
                if (roleSelect == "All")
                {
                    viewModel.ListA = roleList;
                    viewModel.ListB = userList;
                }
                else
                {
                    userList = db.Users.Where(u => u.Role.RoleName == roleSelect).ToList();
                    viewModel.ListA = roleList;
                    viewModel.ListB = userList;
                }
            }
            else if (searchNames != "" && roleSelect != "" && roleSelect == "All")
            {
                userList = db.Users.Where(u => u.UserName.Contains(searchNames)).ToList();
                viewModel.ListA = roleList;
                viewModel.ListB = userList;
            }
            else if (searchNames != "" && roleSelect != "" && roleSelect != "All")
            {
                userList = db.Users.Where(u => u.UserName.Contains(searchNames) && u.Role.RoleName == roleSelect).ToList();
                viewModel.ListA = roleList;
                viewModel.ListB = userList;
            }
            else
            {
                viewModel.ListA = roleList;
                viewModel.ListB = list;
                TempData["SelectionMessage"] = "Plese enter keyword in name field or select a role to filter the results.";
            }
            return viewModel;
        }
        public ActionResult Add(FormCollection form, User user)
        {
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            var viewModel = new User();
            List<LoginInfo> existLogin = new List<LoginInfo>();
            existLogin = db.LoginInfoes.ToList();

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            String newUserName = form["userName"];
            var selectRole = form["SelectRole"];
            var selectStatus = form["SelectStatus"];
            String loginAccount = form["LoginAccount"];
            String loginPassword = form["LoginPassword"];

            //MD5 encryption
            MD5 md5 = new MD5CryptoServiceProvider();
            md5.ComputeHash(ASCIIEncoding.ASCII.GetBytes(loginPassword));
            byte[] result = md5.Hash;
            StringBuilder strBuilder = new StringBuilder();
            for (int i = 0; i < result.Length; i++)
            {
                strBuilder.Append(result[i].ToString("x2"));
            }
            var md5_password = strBuilder.ToString();

            foreach (var validation in existLogin)
            {
                if (loginAccount == validation.LoginName)
                {
                    viewModel.ListA = roleList;
                    viewModel.ListB = userList;
                    viewModel.ModifyMessage = "Login Account already exist please choose another one.";
                    return View(viewModel);
                }
            }

            user.UserName = newUserName;
            user.RoleID = db.Roles.Where(r => r.RoleName == selectRole).FirstOrDefault().RoleID;
            if (selectStatus == "Y")
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
            account.LoginName = loginAccount;
            account.LoginPassword = md5_password;
            account.UserID = user.UserID;
            db.LoginInfoes.Add(account);

            db.SaveChanges();

            userList = db.Users.ToList();
            viewModel.ListA = roleList;
            viewModel.ListB = userList;
            return View("Index", viewModel);
        }
        public ActionResult Modify(FormCollection form, User user)
        {
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            var viewModel = new User();

            int loginID = (int)Session["LoginID"];
            var userInfo = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault().User;
            var userName = userInfo.UserID;

            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            var userID = int.Parse(form["userID"]);
            var user_Name = form["userName"];
            var selectRole = form["SelectRole"];
            var selectStatus = form["SelectStatus"];


            User Update_User = db.Users.SingleOrDefault(u => u.UserID == userID);
            Update_User.UserName = user_Name;
            Update_User.RoleID = db.Roles.Where(r => r.RoleName == selectRole).FirstOrDefault().RoleID;
            if (selectStatus == "Y")
            {
                Update_User.Status = 1;
            }
            else { Update_User.Status = 0; }
            Update_User.ModifyDate = DateTime.Now;
            Update_User.ModifyUser = userName;
            db.SaveChanges();
            userList = db.Users.ToList();
            viewModel.ListA = roleList;
            viewModel.ListB = userList;
            return View("Index", viewModel);
        }
        public User delete(FormCollection form)
        {
            var roleList = db.Roles.ToList();
            var userList = db.Users.ToList();
            var viewModel = new User();
            User dropUser = new User();
            LoginInfo dropAccount = new LoginInfo();

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();


            var checkList = new List<String>();
            if (form["checkbox"] != null)
            {
                checkList = form["checkbox"].Split(',').ToList();
            }
            int intUser;
            foreach (var user in checkList)
            {
                intUser = int.Parse(user);
                dropUser = db.Users.Where(u => u.UserID == intUser).FirstOrDefault();
                dropAccount = db.LoginInfoes.Where(l => l.User.UserID == dropUser.UserID).FirstOrDefault();
                db.Users.Remove(dropUser);
                db.LoginInfoes.Remove(dropAccount);
                db.SaveChanges();

            }
            userList = db.Users.ToList();
            viewModel.ListA = roleList;
            viewModel.ListB = userList;
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
    }
}