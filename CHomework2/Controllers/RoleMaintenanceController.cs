using CHomework2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace CHomework2.Controllers
{
    public class RoleMaintenanceController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();
        // GET: RoleMaintenance
        public ActionResult Index()
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            List<Role> list = new List<Role>();
            var viewModel = new Role();
            viewModel.ListA = roles;
            viewModel.ListB = list;

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            return View(viewModel);
        }

        [HttpPost]
        public ActionResult function(FormCollection form, object sender, EventArgs e)
        {
            var button = form["button"];
            var selectRole = form["SelectRole"];
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            List<Role> emptyList = new List<Role>();
            var viewModel = new Role();
            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();
            viewModel.ListA = roles;

            if (button == "Query")
            {
                viewModel = Query(form);
                return View("Index", viewModel);
            }
            else if (button == "Add")
            {
                return View("Add", viewModel);
            }
            else if (button == "Modify")
            {
                List<Role> list = new List<Role>();
                var checkList = new List<String>();
                if (form["checkbox"] != null) {
                    checkList = form["checkbox"].Split(',').ToList();
                }
                int intRole;
                foreach (var role in checkList)
                {
                    intRole = int.Parse(role);
                    list.Add(db.Roles.Where(r => r.RoleID == intRole).FirstOrDefault());
                }
                if (list.Count() == 1)
                {
                    viewModel.ListB = list;
                    return View("Modify", viewModel);
                }
                else if (list.Count() == 0)
                {
                    viewModel.ListB = roles2;
                    TempData["SelectionMessage"] = "Plese select atleast one role to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    roles2 = db.Roles.ToList();
                    viewModel.ListB = roles2;
                    TempData["SelectionMessage"] = "Plese modify one role at a time.";
                    return View("Index", viewModel);
                }
            }
            else if (button == "Menu")
            {
                List<Role> list = new List<Role>();
                var checkList = form["checkbox"].Split(',').ToList();
                int intRole;
                foreach (var role in checkList)
                {
                    intRole = int.Parse(role);
                    list.Add(db.Roles.Where(r => r.RoleID == intRole).FirstOrDefault());

                }
                if (list.Count() == 1)
                {
                    viewModel.ListB = list;
                    viewModel.ListMenu = list.First().Menus.ToList();
                    viewModel.AllMenu = db.Menus.ToList();
                    return View("Menu", viewModel);
                }
                else if (list.Count() == 0)
                {
                    viewModel.ListB = emptyList;
                    TempData["SelectionMessage"] = "Plese select atleast one role to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    roles2 = db.Roles.ToList();
                    viewModel.ListB = roles2;
                    TempData["SelectionMessage"] = "Plese modify one role at a time.";
                    return View("Index", viewModel);
                }
            }
            else if (button == "Delete")
            {
                viewModel = Delete(form);
                return View("Index", viewModel);
            }
            else if (button == "Download")
            {
                Submit(sender, e);
                viewModel = Query(form);
                return View("Index", viewModel);
            }
            return View("Index", viewModel);
        }

        public Role Query(FormCollection form)
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            var viewModel = new Role();
            var selectRole = form["SelectRole"];

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            if (selectRole == "All")
            {
                viewModel.ListA = roles;
                viewModel.ListB = roles2;
            }
            else
            {
                roles2 = db.Roles.Where(r => r.RoleName == selectRole).ToList();
                viewModel.ListA = roles;
                viewModel.ListB = roles2;
            }
            return viewModel;
        }

        public Role Delete(FormCollection form)
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            var viewModel = new Role();
            Role dropRole = new Role();
            List<Menu> dropMenus = new List<Menu>();

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            var checkList = form["checkbox"].Split(',').ToList();
            int intRole;
            foreach (var role in checkList)
            {
                intRole = int.Parse(role);
                dropMenus = db.Roles.Where(r => r.RoleID == intRole).FirstOrDefault().Menus.ToList();
                foreach (var menu in dropMenus) {
                    db.Roles.Where(r => r.RoleID == intRole).FirstOrDefault().Menus.Remove(menu);
                }
                dropRole = db.Roles.Where(r => r.RoleID == intRole).FirstOrDefault();
                db.Roles.Remove(dropRole);
                db.SaveChanges();
            }
            roles = db.Roles.ToList();
            roles2 = db.Roles.ToList();
            viewModel.ListA = roles;
            viewModel.ListB = roles;
            return viewModel;
        }

        public ActionResult Add(FormCollection form, Role role)
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            var viewModel = new Role();
            viewModel.ListA = roles;
            viewModel.ListB = roles2;

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            var selectStatus = form["SelectStatus"];
            if (selectStatus == "Y")
            {
                role.Status = 1;
            }
            else { role.Status = 0; }
            role.CreateDate = DateTime.Now;
            role.CreateUser = userName;
            role.ModifyDate = DateTime.Now;
            role.ModifyUser = userName;

            db.Roles.Add(role);
            db.SaveChanges();

            roles = db.Roles.ToList();
            roles2 = db.Roles.ToList();
            viewModel.ListA = roles;
            viewModel.ListB = roles2;

            return View("Index", viewModel);
        }

        public ActionResult Modify(FormCollection form, Role role)
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            var viewModel = new Role();
            viewModel.ListA = roles;
            viewModel.ListB = roles2;
            var roleId = int.Parse(form["Origin_Role"]);
            var newRoleName = form["Changed_Role_Name"];

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            Role Role_to_update = db.Roles.SingleOrDefault(r => r.RoleID == roleId);

            Role_to_update.RoleName = newRoleName;
            Role_to_update.RoleDescription = form["Change_Role_Dis"];
            var selectStatus = form["SelectStatus"];
            if (selectStatus == "Y")
            {
                Role_to_update.Status = 1;
            }
            else { Role_to_update.Status = 0; }
            Role_to_update.ModifyDate = DateTime.Now;
            Role_to_update.ModifyUser = userName;
            db.SaveChanges();

            roles = db.Roles.ToList();
            roles2 = db.Roles.ToList();
            viewModel.ListA = roles;
            viewModel.ListB = roles2;

            return View("Index", viewModel);
        }

        public ActionResult Menu(FormCollection form, Role role)
        {
            var roles = db.Roles.ToList();
            var roles2 = db.Roles.ToList();
            var viewModel = new Role();
            viewModel.ListA = roles;
            viewModel.ListB = roles2;
            List<Menu> MenuList = new List<Menu>();
            MenuList = db.Menus.ToList();

            var roleName = int.Parse(form["roleId"]);
            Role select_role = db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault();

            var Role_Menu = select_role.Menus.ToList();

            Menu saveRoleMenu = new Menu();
            Role saveMenuRole = new Role();

            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            foreach (var menu in Role_Menu)
            {
                var checkMenu_delete = form[menu.MenuNo];
                if (checkMenu_delete == null)
                {
                    saveRoleMenu = db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault().Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault();
                    db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault().Menus.Remove(saveRoleMenu);
                    db.SaveChanges();
                }
            }
            foreach (var menu in MenuList)
            {
                var checkMenu_add = form[menu.MenuNo];
                if (checkMenu_add == "on")
                {
                    System.Diagnostics.Debug.WriteLine(menu.Roles.Count);
                    if (!db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.Contains(select_role))
                    {
                        db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.Add(select_role);
                        db.SaveChanges();
                        System.Diagnostics.Debug.WriteLine(db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().LinkName);
                        System.Diagnostics.Debug.WriteLine(select_role.RoleName);
                    }
                }
            }
            return View("Index", viewModel);
        }

        public void Submit(object sender, EventArgs e)
        {
            string RoleJSON = Request.Form["RoleJSON"];
            System.Diagnostics.Debug.WriteLine(RoleJSON);
            try
            {
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(RoleJSON);
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
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "RoleNPOI.xlsx"));
                    Response.BinaryWrite(exportData.ToArray());
                    Response.End();
                }
            }
            catch (Exception error) { }
        }
    }
}