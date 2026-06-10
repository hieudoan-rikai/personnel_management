using System;
using System.Web.Mvc;
using System.Linq;
using EmployeeManagement.Models;
using EmployeeManagement.Services;

namespace EmployeeManagement.Controllers
{
    public class EmployeeController : Controller
    {
        private readonly EmployeeService _employeeService = new EmployeeService();

        public ActionResult Index()
        {
            try
            {
                var employeeList = _employeeService.GetEmployeeList();
                return View(employeeList);
            }
            catch (Exception exception)
            {
                ViewBag.ErrorMessage = "Không thể tải danh sách nhân viên: " + exception.Message;
                return View(new System.Collections.Generic.List<Employee>());
            }
        }

        [HttpPost]
        public JsonResult Create(Employee employee)
        {
            try
            {
                if (employee == null)
                    return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ hoặc trống." });

                if (!ModelState.IsValid)
                    return Json(new { success = false, message = GetFirstModelError() });

                int newId = _employeeService.AddEmployee(employee);
                employee.Id = newId;

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        FullName = employee.FullName,
                        Email = employee.Email,
                        Department = employee.Department,
                        DateOfBirth = employee.DateOfBirth.ToString("dd/MM/yyyy")
                    }
                });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message });
            }
        }

        [HttpGet]
        public JsonResult GetById(int id)
        {
            try
            {
                var employee = _employeeService.GetEmployeeById(id);
                if (employee == null) return Json(new { success = false, message = "Không tìm thấy nhân viên!" }, JsonRequestBehavior.AllowGet);

                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        FullName = employee.FullName,
                        Email = employee.Email,
                        Department = employee.Department,
                        DateOfBirth = employee.DateOfBirth.ToString("yyyy-MM-dd")
                    }
                }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        [HttpPost]
        public JsonResult Edit(Employee employee)
        {
            try
            {
                if (employee == null) return Json(new { success = false, message = "Dữ liệu gửi lên không hợp lệ hoặc trống." });

                if (!ModelState.IsValid)
                    return Json(new { success = false, message = GetFirstModelError() });

                _employeeService.UpdateEmployee(employee);
                return Json(new
                {
                    success = true,
                    data = new
                    {
                        Id = employee.Id,
                        EmployeeCode = employee.EmployeeCode,
                        FullName = employee.FullName,
                        Email = employee.Email,
                        Department = employee.Department,
                        DateOfBirth = employee.DateOfBirth.ToString("dd/MM/yyyy")
                    }
                });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message });
            }
        }

        [HttpPost]
        public JsonResult Delete(int id)
        {
            try
            {
                _employeeService.DeleteEmployee(id);
                return Json(new { success = true });
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message });
            }
        }

        public ActionResult ExportExcel()
        {
            try
            {
                string templatePath = Server.MapPath("~/App_Data/EmployeeTemplate.xlsx");
                byte[] fileBytes = _employeeService.ExportToExcelTemplate(templatePath);
                string fileName = $"Danh_Sach_Nhan_Vien_{DateTime.Now:yyyyMMddHHmmss}.xlsx";

                return File(fileBytes, "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet", fileName);
            }
            catch (Exception exception)
            {
                return Content($"Có lỗi xảy ra trong quá trình xuất file Excel: {exception.Message}");
            }
        }

        [HttpGet]
        public JsonResult GetNextEmployeeCode()
        {
            try
            {
                var nextCode = _employeeService.GenerateNewEmployeeCode();
                return Json(new { success = true, code = nextCode }, JsonRequestBehavior.AllowGet);
            }
            catch (Exception exception)
            {
                return Json(new { success = false, message = exception.Message }, JsonRequestBehavior.AllowGet);
            }
        }

        private string GetFirstModelError()
        {
            return ModelState.Values
                .SelectMany(value => value.Errors)
                .Select(error => error.ErrorMessage)
                .FirstOrDefault(message => !string.IsNullOrWhiteSpace(message))
                ?? "Dữ liệu nhập chưa hợp lệ.";
        }
    }
}
