$(document).ready(function () {
    var employeeIdToDelete = 0;

    $("#btnAddNew").click(function () {
        resetEmployeeForm();
        $("#txtId").val("0");
        $("#modalTitle").text("Đăng ký nhân viên mới");

        $.ajax({
            url: '/Employee/GetNextEmployeeCode',
            type: 'GET',
            success: function (response) {
                if (response.success) {
                    $("#txtEmpCode").val(response.code);
                    $("#txtEmpCode").prop("readonly", true);
                } else {
                    console.log("Không thể lấy mã tự động: " + response.message);
                    $("#txtEmpCode").prop("readonly", false);
                }
                $("#employeeModal").modal('show');
            },
            error: function () {
                $("#txtEmpCode").prop("readonly", false);
                $("#employeeModal").modal('show');
            }
        });
    });

    $("#btnSaveEmployee").click(function () {
        var employeeData = {
            Id: parseInt($("#txtId").val()) || 0,
            EmployeeCode: $("#txtEmpCode").val().trim(),
            FullName: $("#txtFullName").val().trim(),
            Email: $("#txtEmail").val().trim(),
            Department: $("#txtDepartment").val().trim(),
            DateOfBirth: $("#txtBirthDate").val()
        };

        if (!validateEmployeeForm(employeeData)) return;

        var isEdit = employeeData.Id > 0;
        var urlAction = isEdit ? '/Employee/Edit' : '/Employee/Create';

        $.ajax({
            url: urlAction,
            type: 'POST',
            data: JSON.stringify(employeeData),
            contentType: 'application/json; charset=utf-8',
            dataType: 'json',
            success: function (response) {
                if (response.success) {
                    if (isEdit) {
                        var $row = $("tr[data-id='" + response.data.Id + "']");
                        $row.find(".emp-code").text(response.data.EmployeeCode);
                        $row.find(".emp-fullname").text(response.data.FullName);
                        $row.find(".emp-email").text(response.data.Email);
                        $row.find(".emp-department").text(response.data.Department);
                        $row.find(".emp-birthdate").text(response.data.DateOfBirth);

                        $("#employeeModal").modal('hide');
                        showServerMessage("Cập nhật thông tin nhân viên thành công.", "success");
                    } else {
                        var newRow = `<tr data-id="${response.data.Id}">
                                    <td class="emp-code">${escapeHtml(response.data.EmployeeCode)}</td>
                                    <td class="emp-fullname">${escapeHtml(response.data.FullName)}</td>
                                    <td class="emp-department">${escapeHtml(response.data.Department)}</td>
                                    <td class="emp-email">${escapeHtml(response.data.Email)}</td>
                                    <td class="emp-birthdate">${escapeHtml(response.data.DateOfBirth)}</td>
                                    <td class="text-center">
                                        <div class="d-flex justify-content-center align-items-center style-buttons-row">
                                            <button class="btn btn-sm btn-warning btn-edit mr-2" data-id="${response.data.Id}"><i class="fas fa-edit"></i> Sửa</button>
                                            <button class="btn btn-sm btn-danger btn-delete" data-id="${response.data.Id}"><i class="fas fa-trash"></i> Xóa</button>
                                        </div>
                                    </td>
                                </tr>`;

                        $("#employeeTable tbody").prepend(newRow);

                        $("#employeeModal").modal('hide');
                        showServerMessage("Thêm nhân viên thành công.", "success");
                    }
                } else {
                    showServerMessage("Thất bại: " + response.message, "danger");
                }
            },
            error: function (err) {
                showServerMessage("Có lỗi xảy ra trên hệ thống.", "danger");
                console.log(err);
            }
        });
    });

    $(document).on("click", ".btn-edit", function () {
        var id = $(this).data("id");
        clearValidationErrors();

        $.ajax({
            url: '/Employee/GetById',
            type: 'GET',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    $("#txtId").val(response.data.Id);
                    $("#txtEmpCode").val(response.data.EmployeeCode);
                    $("#txtFullName").val(response.data.FullName);
                    $("#txtEmail").val(response.data.Email);
                    $("#txtDepartment").val(response.data.Department);
                    $("#txtBirthDate").val(response.data.DateOfBirth || "");

                    $("#txtEmpCode").prop("readonly", true);
                    $("#modalTitle").text("Cập nhật thông tin nhân viên");
                    $("#employeeModal").modal('show');
                } else {
                    showServerMessage(response.message, "danger");
                }
            },
            error: function () {
                showServerMessage("Không thể kết nối đến máy chủ để lấy thông tin nhân viên.", "danger");
            }
        });
    });

    $(document).on("click", ".btn-delete", function () {
        employeeIdToDelete = $(this).data("id");
        $("#deleteConfirmModal").modal('show');
    });

    $("#btnConfirmDeleteExecute").click(function () {
        if (employeeIdToDelete === 0) return;

        $.ajax({
            url: '/Employee/Delete',
            type: 'POST',
            data: JSON.stringify({ id: employeeIdToDelete }),
            contentType: 'application/json; charset=utf-8',
            success: function (response) {
                $("#deleteConfirmModal").modal('hide');

                if (response.success) {
                    $("tr[data-id='" + employeeIdToDelete + "']").remove();
                    showServerMessage("Xóa nhân viên thành công.", "success");
                } else {
                    showServerMessage("Xóa thất bại: " + response.message, "danger");
                }
                employeeIdToDelete = 0;
            },
            error: function (err) {
                $("#deleteConfirmModal").modal('hide');
                showServerMessage("Có lỗi xảy ra trong quá trình xóa.", "danger");
                employeeIdToDelete = 0;
            }
        });
    });

    $("#employeeModal").on("hidden.bs.modal", function () {
        resetEmployeeForm();
    });

    $(document).on("click", ".toast-close", function () {
        $(this).closest(".app-toast").remove();
    });

    function resetEmployeeForm() {
        $("#employeeForm")[0].reset();
        $("#txtEmpCode").prop("readonly", false);
        clearValidationErrors();
    }

    function clearValidationErrors() {
        $(".text-danger").text("");
        $(".form-control").removeClass("is-invalid");
    }

    function setValidationError(inputSelector, errorSelector, message) {
        $(errorSelector).text(message);
        $(inputSelector).addClass("is-invalid");
    }

    function validateEmployeeForm(employeeData) {
        clearValidationErrors();
        var isValid = true;

        if (employeeData.EmployeeCode === "") {
            setValidationError("#txtEmpCode", "#errorEmployeeCode", "Mã nhân viên không được để trống.");
            isValid = false;
        } else if (employeeData.EmployeeCode.length < 3 || employeeData.EmployeeCode.length > 50) {
            setValidationError("#txtEmpCode", "#errorEmployeeCode", "Mã nhân viên phải từ 3 đến 50 ký tự.");
            isValid = false;
        } else if (!/^[A-Za-z0-9_-]+$/.test(employeeData.EmployeeCode)) {
            setValidationError("#txtEmpCode", "#errorEmployeeCode", "Mã nhân viên chỉ được gồm chữ, số, dấu gạch ngang hoặc gạch dưới.");
            isValid = false;
        }

        if (employeeData.FullName === "") {
            setValidationError("#txtFullName", "#errorFullName", "Họ tên không được để trống.");
            isValid = false;
        } else if (employeeData.FullName.length < 10 || employeeData.FullName.length > 100) {
            setValidationError("#txtFullName", "#errorFullName", "Họ tên phải từ 10 đến 100 ký tự.");
            isValid = false;
        }

        if (employeeData.Email === "") {
            setValidationError("#txtEmail", "#errorEmail", "Email không được để trống.");
            isValid = false;
        } else if (employeeData.Email.length > 100) {
            setValidationError("#txtEmail", "#errorEmail", "Email không được vượt quá 100 ký tự.");
            isValid = false;
        } else if (!/^[^@\s]+@[^@\s]+\.[^@\s]+$/.test(employeeData.Email)) {
            setValidationError("#txtEmail", "#errorEmail", "Email không đúng định dạng.");
            isValid = false;
        }

        if (employeeData.Department === "") {
            setValidationError("#txtDepartment", "#errorDepartment", "Phòng ban không được để trống.");
            isValid = false;
        } else if (employeeData.Department.length < 2 || employeeData.Department.length > 100) {
            setValidationError("#txtDepartment", "#errorDepartment", "Phòng ban phải từ 2 đến 100 ký tự.");
            isValid = false;
        }

        if (employeeData.DateOfBirth === "") {
            setValidationError("#txtBirthDate", "#errorDateOfBirth", "Ngày sinh không được để trống.");
            isValid = false;
        } else {
            var birthDate = new Date(employeeData.DateOfBirth);
            var today = new Date();
            today.setHours(0, 0, 0, 0);

            if (birthDate > today) {
                setValidationError("#txtBirthDate", "#errorDateOfBirth", "Ngày sinh không được vượt quá ngày hiện tại.");
                isValid = false;
            } else if (calculateAge(birthDate, today) < 18) {
                setValidationError("#txtBirthDate", "#errorDateOfBirth", "Nhân viên phải đủ 18 tuổi.");
                isValid = false;
            }
        }

        return isValid;
    }

    function calculateAge(birthDate, today) {
        var age = today.getFullYear() - birthDate.getFullYear();
        var monthDiff = today.getMonth() - birthDate.getMonth();

        if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
            age--;
        }

        return age;
    }

    function escapeHtml(value) {
        return $("<div>").text(value || "").html();
    }

    function showServerMessage(msg, type) {
        var toastClass = type === "success" ? "toast-success" : "toast-danger";
        var icon = type === "success" ? "fa-check-circle" : "fa-exclamation-triangle";
        var title = type === "success" ? "Thành công" : "Lỗi";

        var toastHtml = `
            <div class="app-toast ${toastClass}">
                <div class="toast-icon"><i class="fas ${icon}"></i></div>
                <div class="toast-content">
                    <div class="toast-title">${title}</div>
                    <div class="toast-message">${escapeHtml(msg)}</div>
                </div>
                <button type="button" class="toast-close" aria-label="Đóng">&times;</button>
            </div>`;

        var $toast = $(toastHtml);
        $("#serverMessage").append($toast);

        setTimeout(function () {
            $toast.fadeOut(200, function () {
                $(this).remove();
            });
        }, 3000);
    }
});
