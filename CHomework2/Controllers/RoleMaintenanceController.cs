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
            var viewModel = new Role();
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = new List<Role>();
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
            var viewModel = new Role();
            int loginID = (int)Session["LoginID"];

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();
            viewModel.ListA = db.Roles.ToList();

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
                if (select_role(form).Count() == 1)
                {
                    viewModel.ListB = select_role(form);
                    return View("Modify", viewModel);
                }
                else if (select_role(form).Count() == 0)
                {
                    viewModel.ListB = db.Roles.ToList();
                    TempData["SelectionMessage"] = "Plese select atleast one role to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    viewModel.ListB = db.Roles.ToList();
                    TempData["SelectionMessage"] = "Plese modify one role at a time.";
                    return View("Index", viewModel);
                }
            }
            else if (button == "Menu")
            {
                if (select_role(form).Count() == 1)
                {
                    viewModel.ListB = select_role(form);
                    viewModel.ListMenu = select_role(form).First().Menus.ToList();
                    viewModel.AllMenu = db.Menus.ToList();
                    return View("Menu", viewModel);
                }
                else if (select_role(form).Count() == 0)
                {
                    viewModel.ListB = new List<Role>();
                    TempData["SelectionMessage"] = "Plese select atleast one role to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    viewModel.ListB = db.Roles.ToList();
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
            var viewModel = new Role();
            var selectRole = form["SelectRole"];
            try
            {
                int loginAccount = (int)Session["LoginID"];
                viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
                viewModel.ListA = db.Roles.ToList();

                if (selectRole == "All")
                {
                    viewModel.ListB = db.Roles.ToList();
                }
                else
                {
                    viewModel.ListB = db.Roles.Where(r => r.RoleName == selectRole).ToList();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                viewModel.ListB = new List<Role>();
            }
            return viewModel;
        }

        public ActionResult Add(FormCollection form, Role role)
        {
            var viewModel = new Role();
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            try
            {
                if (form["SelectStatus"] == "Y")
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
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();

            return View("Index", viewModel);
        }

        public List<Role> select_role(FormCollection form)
        {
            List<Role> listCheck = new List<Role>();
            try
            {
                var checkList = new List<String>();
                if (form["checkbox"] != null)
                {
                    checkList = form["checkbox"].Split(',').ToList();
                }

                foreach (var role in checkList)
                {
                    int int_role = Int32.Parse(role);
                    listCheck.Add(db.Roles.Where(r => r.RoleID == int_role).FirstOrDefault());
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

        public ActionResult Modify(FormCollection form, Role role)
        {
            var viewModel = new Role();
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();
            var roleId = int.Parse(form["Origin_Role"]);

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            try
            {
                Role Role_to_update = db.Roles.SingleOrDefault(r => r.RoleID == roleId);

                Role_to_update.RoleName = form["Changed_Role_Name"];
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
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }

            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();

            return View("Index", viewModel);
        }

        public ActionResult Menu(FormCollection form, Role role)
        {
            var viewModel = new Role();
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();
            List<Menu> MenuList = db.Menus.ToList();

            var roleName = int.Parse(form["roleId"]);
            Role select_role = db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault();
            var Role_Menu = select_role.Menus.ToList();

            int loginAccount = (int)Session["LoginID"];

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();
            try
            {
                foreach (var menu in Role_Menu)
                {
                    var checkMenu_delete = form[menu.MenuNo];
                    if (checkMenu_delete == null)
                    {
                        Menu saveRoleMenu = db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault().Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault();
                        db.Roles.Where(r => r.RoleID == roleName).FirstOrDefault().Menus.Remove(saveRoleMenu);
                        db.SaveChanges();
                    }
                }
                foreach (var menu in MenuList)
                {
                    var checkMenu_add = form[menu.MenuNo];
                    if (checkMenu_add == "on")
                    {
                        if (!db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.Contains(select_role))
                        {
                            db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.Add(select_role);
                            db.SaveChanges();
                        }
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            return View("Index", viewModel);
        }

        public Role Delete(FormCollection form)
        {
            var viewModel = new Role();

            int loginAccount = (int)Session["LoginID"];
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            try
            {
                foreach (var role in select_role(form))
                {
                    List<Menu> dropMenus = db.Roles.Where(r => r.RoleID == role.RoleID).FirstOrDefault().Menus.ToList();
                    foreach (var menu in dropMenus)
                    {
                        db.Roles.Where(r => r.RoleID == role.RoleID).FirstOrDefault().Menus.Remove(menu);
                    }
                    Role dropRole = db.Roles.Where(r => r.RoleID == role.RoleID).FirstOrDefault();
                    db.Roles.Remove(dropRole);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListA = db.Roles.ToList();
            viewModel.ListB = db.Roles.ToList();
            return viewModel;
        }

        public void Submit(object sender, EventArgs e)
        {
            string RoleJSON = Request.Form["RoleJSON"];
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
            catch (Exception error)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(error.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
        }
    }
}