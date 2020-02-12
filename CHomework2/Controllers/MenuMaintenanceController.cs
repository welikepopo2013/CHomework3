using CHomework2.Models;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Web.Mvc;
using Newtonsoft.Json;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;

namespace CHomework2.Controllers
{
    public class MenuMaintenanceController : Controller
    {
        SystemDataBaseEntity db = new SystemDataBaseEntity();
        // GET: MenuMaintenance
        public ActionResult Index()
        {
            if (Session["LoginID"] == null)
            {
                return RedirectToAction("Index", "Login");
            }
            else
            {
                var viewModel = new Menu();
                viewModel.ListA = db.Menus.ToList();
                viewModel.ListC = new List<Menu>();
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
        }

        [HttpPost]
        public ActionResult function(FormCollection form, object sender, EventArgs e)
        {
            var button = form["button"];
            var viewModel = new Menu();
            int loginAccount = (int)Session["LoginID"];

            viewModel.ListA = db.Menus.ToList();
            viewModel.ListC = new List<Menu>();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            if (button == "Query")
            {
                viewModel = query(form);
                return View("Index", viewModel);
            }

            else if (button == "Add")
            {
                viewModel.ListA = db.Menus.Where(m => m.MenuLevel == 1).ToList();
                return View("Add", viewModel);
            }

            else if (button == "Modify")
            {
                if (select_menu(form).Count() == 1)
                {
                    viewModel.ListA = select_menu(form);
                    return View("Modify", viewModel);
                }
                else if (select_menu(form).Count() == 0)
                {
                    TempData["SelectionMessage"] = "Plese select atleast one menu to modify.";
                    return View("Index", viewModel);
                }
                else
                {
                    TempData["SelectionMessage"] = "Plese modify one menu at a time.";
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

        public Menu query(FormCollection form)
        {
            var viewModel = new Menu();
            var selectMenuNo = form["SelectMenuNo"];
            var selectMenuName = form["SelectMenuName"];
            var selectType = form["SelectType"];
            var selectStatus = form["SelectStatus"];

            try
            {
                int loginAccount = (int)Session["LoginID"];
                LoginInfo loginInfo = new LoginInfo();
                viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

                viewModel.ListA = db.Menus.ToList();
                viewModel.ListC = db.Menus.ToList();

                if (selectMenuNo != "All")
                {
                    viewModel.ListC = db.Menus.Where(m => m.MenuNo == selectMenuNo).ToList();
                }
                if (selectMenuName != "All")
                {
                    viewModel.ListC = db.Menus.Where(m => m.LinkName == selectMenuName).ToList();
                }
                if (selectType != "All")
                {
                    if (selectType == "Menu")
                    {
                        viewModel.ListC = db.Menus.Where(m => m.LinkType == 0).ToList();
                    }
                    else if (selectType == "Program")
                    {
                        viewModel.ListC = db.Menus.Where(m => m.LinkType == 1).ToList();
                    }
                }
                if (selectStatus != "All")
                {
                    if (selectStatus == "Y")
                    {
                        viewModel.ListC = db.Menus.Where(m => m.Status == 1).ToList();
                    }
                    else if (selectStatus == "N")
                    {
                        viewModel.ListC = db.Menus.Where(m => m.Status == 0).ToList();
                    }
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
                viewModel.ListC = new List<Menu>();
            }
            return viewModel;
        }

        public ActionResult Add(FormCollection form, Menu menu)
        {
            var menuNo = db.Menus.ToList();
            var viewModel = new Menu();
            var newMenu = new Menu();
            var calMenu = new Menu();

            var MenuName = form["menuName"];
            var MenuGroup = form["SelectMenu"];
            var MenuLink = form["LinkURL"];

            string newMenuNo = "";
            int newMenuNoInt = 0;
            byte newLinkType = 0;
            byte newMenuLevel = 0;

            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            var userName = userInfo.UserID;

            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            try
            {
                if (MenuGroup == "This is a new Menu Group")
                {
                    calMenu = db.Menus.Where(m => m.MenuLevel == 1).OrderByDescending(m => m.MenuNo).FirstOrDefault();
                    newMenuNo = calMenu.MenuNo;
                    newMenuNoInt = int.Parse(newMenuNo) + 1;
                    newMenuNo = "00" + newMenuNoInt.ToString();
                    newLinkType = 0;
                    newMenuLevel = 1;


                }
                else if (MenuGroup != "This is a new Menu Group")
                {
                    foreach (var each in menuNo)
                    {
                        if (MenuGroup == each.LinkName)
                        {
                            calMenu = db.Menus.Where(m => m.MenuNo.Contains(each.MenuNo)).OrderByDescending(m => m.MenuNo).FirstOrDefault();
                            System.Diagnostics.Debug.WriteLine(calMenu.MenuNo);
                            if (calMenu.MenuNo == each.MenuNo)
                            {
                                newMenuNo = calMenu.MenuNo;
                                newMenuNoInt = int.Parse(newMenuNo) * 10 + 1;
                                newMenuNo = "00" + newMenuNoInt.ToString();
                            }
                            else
                            {
                                newMenuNo = calMenu.MenuNo;
                                newMenuNoInt = int.Parse(newMenuNo) + 1;
                                newMenuNo = "00" + newMenuNoInt.ToString();
                            }
                            newLinkType = 1;
                            newMenuLevel = 2;
                        }
                    }
                }

                newMenu.MenuNo = newMenuNo;
                newMenu.LinkType = newLinkType;
                newMenu.LinkName = MenuName;
                if (newMenuLevel == 1)
                {
                    newMenu.LinkURL = null;
                }
                else
                {
                    newMenu.LinkURL = MenuLink;
                }

                if (form["SelectStatus"] == "Y")
                {
                    newMenu.Status = 1;
                }
                else { newMenu.Status = 0; }
                newMenu.MenuLevel = newMenuLevel;
                newMenu.CeateDate = DateTime.Now;
                newMenu.Createuser = userName;
                newMenu.ModifyDate = DateTime.Now;
                newMenu.ModifyUser = userName;
                db.Menus.Add(newMenu);
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }

            viewModel.ListA = db.Menus.ToList();
            viewModel.ListC = db.Menus.ToList();

            return View("Index", viewModel);
        }

        public List<Menu> select_menu(FormCollection form)
        {
            List<Menu> listCheck = new List<Menu>();
            try
            {
                var checkList = new List<String>();
                if (form["checkbox"] != null)
                {
                    checkList = form["checkbox"].Split(',').ToList();
                }

                foreach (var menu in checkList)
                {
                    listCheck.Add(db.Menus.Where(m => m.MenuNo == menu).FirstOrDefault());
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

        public ActionResult Modify(FormCollection form, Menu menu)
        {
            var viewModel = new Menu();
            String Menu_No = form["Origin_Menu_No"];
            int loginID = (int)Session["LoginID"];
            var userInfo = db.Users.Where(l => l.UserID == loginID).FirstOrDefault();
            Menu Update_Menu = db.Menus.SingleOrDefault(m => m.MenuNo == Menu_No);
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginID).FirstOrDefault();

            try
            {
                Update_Menu.LinkName = form["Menu_Name"];

                if (form["SelectType"] == "Program")
                {
                    Update_Menu.LinkType = 1;
                }
                else { Update_Menu.LinkType = 0; }

                if (form["SelectStatus"] == "Y")
                {
                    Update_Menu.Status = 1;
                }
                else { Update_Menu.Status = 0; }

                if (form["Menu_Level"] == "0")
                {
                    Update_Menu.MenuLevel = 0;
                }
                else if (form["Menu_Level"] == "1")
                {
                    Update_Menu.MenuLevel = 1;
                }
                else { Update_Menu.MenuLevel = 2; }
                Update_Menu.ModifyDate = DateTime.Now;
                Update_Menu.ModifyUser = userInfo.UserID;
                db.SaveChanges();
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            viewModel.ListA = db.Menus.ToList();
            viewModel.ListC = db.Menus.ToList();
            return View("Index", viewModel);
        }

        public Menu delete(FormCollection form)
        {
            var viewModel = new Menu();
            List<Role> dropRoles = new List<Role>();

            try
            {
                foreach (var menu in select_menu(form))
                {
                    dropRoles = db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.ToList();
                    foreach (var role in dropRoles)
                    {
                        db.Menus.Where(m => m.MenuNo == menu.MenuNo).FirstOrDefault().Roles.Remove(role);
                    }
                    db.Menus.Remove(menu);
                    db.SaveChanges();
                }
            }
            catch (Exception e)
            {
                System.Diagnostics.Debug.WriteLine("--System Eror - Exception--");
                System.Diagnostics.Debug.WriteLine(e.ToString());
                System.Diagnostics.Debug.WriteLine("--System Eror - End--");
            }
            int loginAccount = (int)Session["LoginID"];
            LoginInfo loginInfo = new LoginInfo();
            viewModel.sideNav = db.LoginInfoes.Where(l => l.LoginID == loginAccount).FirstOrDefault();

            viewModel.ListA = db.Menus.ToList();
            viewModel.ListC = db.Menus.ToList();
            return viewModel;
        }

        public void Submit(object sender, EventArgs e)
        {
            string MenuJSON = Request.Form["MenuJSON"];
            System.Diagnostics.Debug.WriteLine(MenuJSON);
            try
            {
                DataTable dt = JsonConvert.DeserializeObject<DataTable>(MenuJSON);
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
                    Response.AddHeader("Content-Disposition", string.Format("attachment;filename={0}", "MenuNPOI.xlsx"));
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