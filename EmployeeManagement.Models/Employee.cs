using System;
using System.ComponentModel.DataAnnotations;

namespace EmployeeManagement.Models
{
    public class Employee
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Mã nhân viên không được để trống.")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Mã nhân viên phải từ 3 đến 50 ký tự.")]
        public string EmployeeCode { get; set; }

        [Required(ErrorMessage = "Họ tên không được để trống.")]
        [StringLength(100, MinimumLength = 10, ErrorMessage = "Họ tên phải từ 10 đến 100 ký tự.")]
        public string FullName { get; set; }

        [Required(ErrorMessage = "Email không được để trống.")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
        [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự.")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Phòng ban không được để trống.")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Phòng ban phải từ 2 đến 100 ký tự.")]
        public string Department { get; set; }

        [Required(ErrorMessage = "Ngày sinh không được để trống.")]
        public DateTime DateOfBirth { get; set; }
    }
}
