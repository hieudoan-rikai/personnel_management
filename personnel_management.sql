CREATE TABLE Employees (
    Id SERIAL PRIMARY KEY,
    EmployeeCode VARCHAR(50) NOT NULL UNIQUE,
    FullName VARCHAR(100) NOT NULL,
    Email VARCHAR(100) NOT NULL,
    Department VARCHAR(100) NOT NULL,
    DateOfBirth DATE NOT NULL
);

CREATE UNIQUE INDEX UX_Employees_Email_Active
ON Employees (LOWER(Email))
WHERE Department != 'DELETED_DELETED';

-- Chạy phần này nếu database đã tồn tại từ phiên bản chưa có Email:
-- ALTER TABLE Employees ADD COLUMN IF NOT EXISTS Email VARCHAR(100);
-- UPDATE Employees SET Email = LOWER(EmployeeCode) || '@company.local' WHERE Email IS NULL OR Email = '';
-- ALTER TABLE Employees ALTER COLUMN Email SET NOT NULL;
-- CREATE UNIQUE INDEX IF NOT EXISTS UX_Employees_Email_Active ON Employees (LOWER(Email)) WHERE Department != 'DELETED_DELETED';

INSERT INTO Employees (EmployeeCode, FullName, Email, Department, DateOfBirth) VALUES
('NV001', 'Nguyễn Văn A', 'nv001@company.local', 'Phòng Công nghệ thông tin', '1995-05-20'),
('NV002', 'Trần Thị B', 'nv002@company.local', 'Phòng Nhân sự', '1998-11-12'),
('NV003', 'Lê Văn Cường', 'nv003@company.local', 'Phòng Kế toán', '1992-02-28');
