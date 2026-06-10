using EmployeeManagement.Models;
using System;
using System.Collections.Generic;
using Npgsql;
using System.Configuration;

namespace EmployeeManagement.DataAccess
{
    public class EmployeeRepository
    {
        private readonly string _connectionString = ConfigurationManager.ConnectionStrings["PostgresConnection"].ConnectionString;

        public List<Employee> GetAll()
        {
            var employees = new List<Employee>();
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = "SELECT id, employeecode, fullname, email, department, dateofbirth FROM Employees WHERE department != 'DELETED_DELETED' ORDER BY id DESC";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            employees.Add(new Employee
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                EmployeeCode = reader["employeecode"].ToString(),
                                FullName = reader["fullname"].ToString(),
                                Email = reader["email"].ToString(),
                                Department = reader["department"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["dateofbirth"])
                            });
                        }
                    }
                }
            }
            return employees;
        }

        public Employee GetById(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = "SELECT id, employeecode, fullname, email, department, dateofbirth FROM Employees WHERE id = @Id AND department != 'DELETED_DELETED'";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    using (var reader = command.ExecuteReader())
                    {
                        if (reader.Read())
                        {
                            return new Employee
                            {
                                Id = Convert.ToInt32(reader["id"]),
                                EmployeeCode = reader["employeecode"].ToString(),
                                FullName = reader["fullname"].ToString(),
                                Email = reader["email"].ToString(),
                                Department = reader["department"].ToString(),
                                DateOfBirth = Convert.ToDateTime(reader["dateofbirth"])
                            };
                        }
                    }
                }
            }
            return null;
        }

        public int Insert(Employee employee)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"INSERT INTO Employees (employeecode, fullname, email, department, dateofbirth) 
                               VALUES (@EmployeeCode, @FullName, @Email, @Department, @DateOfBirth) RETURNING id;";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
                    command.Parameters.AddWithValue("@FullName", employee.FullName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Department", employee.Department);
                    command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar());
                }
            }
        }

        public bool Update(Employee employee)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"UPDATE Employees 
                               SET employeecode = @EmployeeCode, fullname = @FullName, email = @Email, department = @Department, dateofbirth = @DateOfBirth 
                               WHERE id = @Id AND department != 'DELETED_DELETED'";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", employee.Id);
                    command.Parameters.AddWithValue("@EmployeeCode", employee.EmployeeCode);
                    command.Parameters.AddWithValue("@FullName", employee.FullName);
                    command.Parameters.AddWithValue("@Email", employee.Email);
                    command.Parameters.AddWithValue("@Department", employee.Department);
                    command.Parameters.AddWithValue("@DateOfBirth", employee.DateOfBirth);

                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool Delete(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = "UPDATE Employees SET department = 'DELETED_DELETED' WHERE id = @Id AND department != 'DELETED_DELETED'";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    return command.ExecuteNonQuery() > 0;
                }
            }
        }

        public bool IsEmployeeCodeExists(string employeeCode, int excludeId = 0)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Employees
                               WHERE LOWER(employeecode) = LOWER(@EmployeeCode)
                               AND id != @ExcludeId
                               AND department != 'DELETED_DELETED'";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@EmployeeCode", employeeCode);
                    command.Parameters.AddWithValue("@ExcludeId", excludeId);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public bool IsEmailExists(string email, int excludeId = 0)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"SELECT COUNT(*) FROM Employees
                               WHERE LOWER(email) = LOWER(@Email)
                               AND id != @ExcludeId
                               AND department != 'DELETED_DELETED'";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Email", email);
                    command.Parameters.AddWithValue("@ExcludeId", excludeId);

                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }

        public string GetMaxEmployeeCode()
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = @"SELECT employeecode FROM Employees
                               WHERE employeecode LIKE 'NV%'
                               AND department != 'DELETED_DELETED'
                               ORDER BY CAST(SUBSTRING(employeecode, 3) AS INTEGER) DESC LIMIT 1";
                using (var command = new NpgsqlCommand(sql, connection))
                {
                    connection.Open();
                    var result = command.ExecuteScalar();
                    return result?.ToString() ?? string.Empty;
                }
            }
        }

        public bool ExistsActive(int id)
        {
            using (var connection = new NpgsqlConnection(_connectionString))
            {
                string sql = "SELECT COUNT(*) FROM Employees WHERE id = @Id AND department != 'DELETED_DELETED'";

                using (var command = new NpgsqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@Id", id);
                    connection.Open();
                    return Convert.ToInt32(command.ExecuteScalar()) > 0;
                }
            }
        }
    }
}
