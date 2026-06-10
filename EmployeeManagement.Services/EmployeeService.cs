using EmployeeManagement.DataAccess;
using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace EmployeeManagement.Services
{
    public class EmployeeService
    {
        private readonly EmployeeRepository _employeeRepository = new EmployeeRepository();

        public List<Employee> GetEmployeeList()
        {
            return _employeeRepository.GetAll();
        }

        public Employee GetEmployeeById(int id)
        {
            return _employeeRepository.GetById(id);
        }

        private void ValidateEmployee(Employee employee)
        {
            if (employee == null)
                throw new Exception("Dữ liệu nhân viên không được để trống.");

            employee.EmployeeCode = employee.EmployeeCode?.Trim();
            employee.FullName = employee.FullName?.Trim();
            employee.Email = employee.Email?.Trim();
            employee.Department = employee.Department?.Trim();

            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new Exception("Mã nhân viên không được để trống.");

            if (employee.EmployeeCode.Length < 3 || employee.EmployeeCode.Length > 50)
                throw new Exception("Mã nhân viên phải từ 3 đến 50 ký tự.");

            if (!Regex.IsMatch(employee.EmployeeCode, @"^[A-Za-z0-9_-]+$"))
                throw new Exception("Mã nhân viên chỉ được gồm chữ, số, dấu gạch ngang hoặc gạch dưới.");

            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new Exception("Họ tên không được để trống.");

            if (employee.FullName.Length < 10 || employee.FullName.Length > 100)
                throw new Exception("Họ tên phải từ 10 đến 100 ký tự.");

            if (string.IsNullOrWhiteSpace(employee.Email))
                throw new Exception("Email không được để trống.");

            if (employee.Email.Length > 100)
                throw new Exception("Email không được vượt quá 100 ký tự.");

            if (!Regex.IsMatch(employee.Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                throw new Exception("Email không đúng định dạng.");

            if (string.IsNullOrWhiteSpace(employee.Department))
                throw new Exception("Phòng ban không được để trống.");

            if (employee.Department.Length < 2 || employee.Department.Length > 100)
                throw new Exception("Phòng ban phải từ 2 đến 100 ký tự.");

            if (employee.DateOfBirth == DateTime.MinValue)
                throw new Exception("Ngày sinh không được để trống.");

            if (employee.DateOfBirth > DateTime.Today)
                throw new Exception("Ngày sinh không được vượt quá ngày hiện tại.");

            int age = DateTime.Today.Year - employee.DateOfBirth.Year;
            if (employee.DateOfBirth > DateTime.Today.AddYears(-age))
                age--;

            if (age < 18)
                throw new Exception("Nhân viên phải đủ 18 tuổi.");
        }

        public int AddEmployee(Employee employee)
        {
            ValidateEmployee(employee);

            if (_employeeRepository.IsEmployeeCodeExists(employee.EmployeeCode))
                throw new Exception("Mã nhân viên đã tồn tại.");

            if (_employeeRepository.IsEmailExists(employee.Email))
                throw new Exception("Email đã tồn tại.");

            return _employeeRepository.Insert(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            ValidateEmployee(employee);

            if (employee.Id <= 0 || !_employeeRepository.ExistsActive(employee.Id))
                throw new Exception("Không tìm thấy nhân viên cần cập nhật.");

            if (_employeeRepository.IsEmployeeCodeExists(employee.EmployeeCode, employee.Id))
                throw new Exception("Mã nhân viên đã tồn tại ở một bản ghi khác.");

            if (_employeeRepository.IsEmailExists(employee.Email, employee.Id))
                throw new Exception("Email đã tồn tại ở một bản ghi khác.");

            bool isUpdated = _employeeRepository.Update(employee);
            if (!isUpdated)
                throw new Exception("Không tìm thấy nhân viên hoặc cập nhật thất bại.");
        }

        public void DeleteEmployee(int id)
        {
            if (id <= 0 || !_employeeRepository.ExistsActive(id))
                throw new Exception("Không tìm thấy nhân viên cần xóa.");

            bool isDeleted = _employeeRepository.Delete(id);
            if (!isDeleted)
                throw new Exception("Không tìm thấy nhân viên hoặc xóa thất bại.");
        }

        public byte[] ExportToExcelTemplate(string templatePath)
        {
            if (!File.Exists(templatePath))
                throw new FileNotFoundException("Không tìm thấy file template Excel trên hệ thống.");

            List<Employee> employeeList = _employeeRepository.GetAll();
            IWorkbook workbook;

            using (FileStream fileStream = new FileStream(templatePath, FileMode.Open, FileAccess.Read))
            {
                workbook = new XSSFWorkbook(fileStream);
            }

            ISheet sheet = workbook.GetSheetAt(0);
            EnsureEmployeeTemplateLayout(sheet);

            IRow rowDate = sheet.GetRow(2);
            if (rowDate == null) rowDate = sheet.CreateRow(2);
            CreateCellAndSetValue(rowDate, 1, DateTime.Now.ToString("yyyy/MM/dd"));

            int startRowIndex = 5;

            foreach (var employee in employeeList)
            {
                IRow row = sheet.GetRow(startRowIndex);
                if (row == null) row = sheet.CreateRow(startRowIndex);

                CreateCellAndSetValue(row, 0, employee.EmployeeCode);
                CreateCellAndSetValue(row, 1, employee.FullName);
                CreateCellAndSetValue(row, 2, employee.Department);
                CreateCellAndSetValue(row, 3, employee.Email);
                CreateCellAndSetValue(row, 4, employee.DateOfBirth.ToString("yyyy/MM/dd"));

                startRowIndex++;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
        }

        private void EnsureEmployeeTemplateLayout(ISheet sheet)
        {
            IRow headerRow = sheet.GetRow(4);
            if (headerRow == null) headerRow = sheet.CreateRow(4);

            CreateCellAndSetValue(headerRow, 0, "社員番号");
            CreateCellAndSetValue(headerRow, 1, "氏名");
            CreateCellAndSetValue(headerRow, 2, "部署");
            CreateCellAndSetValue(headerRow, 3, "メール");
            CreateCellAndSetValue(headerRow, 4, "生年月日");

            CopyCellStyle(headerRow, 3, 4);

            if (sheet.GetColumnWidth(4) == sheet.DefaultColumnWidth * 256)
                sheet.SetColumnWidth(4, sheet.GetColumnWidth(3));
        }

        private void CopyCellStyle(IRow row, int sourceCellIndex, int targetCellIndex)
        {
            ICell sourceCell = row.GetCell(sourceCellIndex);
            ICell targetCell = row.GetCell(targetCellIndex);

            if (sourceCell != null && targetCell != null)
                targetCell.CellStyle = sourceCell.CellStyle;
        }

        private void CreateCellAndSetValue(IRow row, int cellIndex, object value)
        {
            ICell cell = row.GetCell(cellIndex);
            if (cell == null) cell = row.CreateCell(cellIndex);

            if (value is int || value is double || value is long)
                cell.SetCellValue(Convert.ToDouble(value));
            else
                cell.SetCellValue(value?.ToString() ?? "");
        }

        public string GenerateNewEmployeeCode()
        {
            string maxCode = _employeeRepository.GetMaxEmployeeCode();

            if (string.IsNullOrEmpty(maxCode) || maxCode.Length < 3)
            {
                return "NV001"; 
            }

            try
            {
                string numberPart = maxCode.Substring(2);
                if (int.TryParse(numberPart, out int currentNumber))
                {
                    int nextNumber = currentNumber + 1;
                    return "NV" + nextNumber.ToString("D3");
                }
            }
            catch
            {
            }

            return "NV001";
        }
    }
}
