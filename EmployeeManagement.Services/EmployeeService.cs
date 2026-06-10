using EmployeeManagement.DataAccess;
using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using System.IO;
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
            if (string.IsNullOrWhiteSpace(employee.EmployeeCode))
                throw new Exception("Mã nhân viên không được để trống.");

            if (string.IsNullOrWhiteSpace(employee.FullName))
                throw new Exception("Họ tên không được để trống.");

            if (string.IsNullOrWhiteSpace(employee.Department))
                throw new Exception("Phòng ban không được để trống.");

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

            return _employeeRepository.Insert(employee);
        }

        public void UpdateEmployee(Employee employee)
        {
            ValidateEmployee(employee);

            if (_employeeRepository.IsEmployeeCodeExists(employee.EmployeeCode, employee.Id))
                throw new Exception("Mã nhân viên đã tồn tại ở một bản ghi khác.");

            bool isUpdated = _employeeRepository.Update(employee);
            if (!isUpdated)
                throw new Exception("Không tìm thấy nhân viên hoặc cập nhật thất bại.");
        }

        public void DeleteEmployee(int id)
        {
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
                CreateCellAndSetValue(row, 3, employee.DateOfBirth.ToString("yyyy/MM/dd"));

                startRowIndex++;
            }

            using (MemoryStream memoryStream = new MemoryStream())
            {
                workbook.Write(memoryStream);
                return memoryStream.ToArray();
            }
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