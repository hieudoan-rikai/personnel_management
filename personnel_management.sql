CREATE TABLE Employees (
    Id SERIAL PRIMARY KEY,
    EmployeeCode VARCHAR(50) NOT NULL UNIQUE,
    FullName VARCHAR(250) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL
);

-- Chèn dữ liệu mẫu
INSERT INTO Employees (EmployeeCode, FullName, Department, DateOfBirth) VALUES
('NV001', 'Nguyễn Văn A', 'Phòng Công nghệ thông tin', '1995-05-20'),
('NV002', 'Trần Thị B', 'Phòng Nhân sự', '1998-11-12'),
('NV003', 'Lê Văn C', 'Phòng Kế toán', '1992-02-28');