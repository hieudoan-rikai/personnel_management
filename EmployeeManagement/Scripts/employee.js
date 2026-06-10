$(document).ready(function () {

    $("#btnAddNew").click(function () {
        $("#employeeForm")[0].reset();
        $("#txtId").val("0");
        $(".text-danger").text("");
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
            Department: $("#txtDepartment").val().trim(),
            DateOfBirth: $("#txtBirthDate").val()
        };

        $(".text-danger").text("");
        var isValid = true;

        if (employeeData.EmployeeCode === "") {
            $("#errorEmployeeCode").text("Mã nhân viên không được để trống");
            isValid = false;
        }
        if (employeeData.FullName === "") {
            $("#errorFullName").text("Họ tên không được để trống");
            isValid = false;
        }
        if (employeeData.Department === "") {
            $("#errorDepartment").text("Phòng ban không được để trống");
            isValid = false;
        }
        if (employeeData.DateOfBirth === "") {
            $("#errorDateOfBirth").text("Ngày sinh không được để trống");
            isValid = false;
        } else {
            var birthDate = new Date(employeeData.DateOfBirth);
            var today = new Date();

            if (birthDate > today) {
                $("#errorDateOfBirth").text("Ngày sinh không được vượt quá hiện tại");
                isValid = false;
            }

            var age = today.getFullYear() - birthDate.getFullYear();
            var monthDiff = today.getMonth() - birthDate.getMonth();
            if (monthDiff < 0 || (monthDiff === 0 && today.getDate() < birthDate.getDate())) {
                age--;
            }

            if (age < 18) {
                $("#errorDateOfBirth").text("Nhân viên phải đủ 18 tuổi");
                isValid = false;
            }
        }

        if (!isValid) return;

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
                        $row.find(".emp-department").text(response.data.Department);
                        $row.find(".emp-birthdate").text(response.data.DateOfBirth);

                        $("#employeeModal").modal('hide');
                        showServerMessage("Cập nhật thông tin nhân viên thành công!", "success");
                    } else {
                        var newRow = `<tr data-id="${response.data.Id}">
                                    <td class="emp-code">${response.data.EmployeeCode}</td>
                                    <td class="emp-fullname">${response.data.FullName}</td>
                                    <td class="emp-department">${response.data.Department}</td>
                                    <td class="emp-birthdate">${response.data.DateOfBirth}</td>
                                    <td class="text-center">
                                        <div class="d-flex justify-content-center align-items-center style-buttons-row">
                                            <button class="btn btn-sm btn-warning btn-edit mr-2" data-id="${response.data.Id}"><i class="fas fa-edit"></i> Sửa</button>
                                            <button class="btn btn-sm btn-danger btn-delete" data-id="${response.data.Id}"><i class="fas fa-trash"></i> Xóa</button>
                                        </div>
                                    </td>
                                </tr>`;

                        $("#employeeTable tbody").prepend(newRow);

                        $("#employeeModal").modal('hide');
                        showServerMessage("Thêm nhân viên thành công!", "success");
                    }
                } else {
                    showServerMessage("Thất bại: " + response.message, "danger");
                }
            },
            error: function (err) {
                showServerMessage("Có lỗi xảy ra trên hệ thống!", "danger");
                console.log(err);
            }
        });
    });

    $(document).on("click", ".btn-edit", function () {
        var id = $(this).data("id");
        $(".text-danger").text("");

        $.ajax({
            url: '/Employee/GetById',
            type: 'GET',
            data: { id: id },
            success: function (response) {
                if (response.success) {
                    $("#txtId").val(response.data.Id);
                    $("#txtEmpCode").val(response.data.EmployeeCode);
                    $("#txtFullName").val(response.data.FullName);
                    $("#txtDepartment").val(response.data.Department);

                    if (response.data.DateOfBirth) {
                        var dateParts = response.data.DateOfBirth.split('/');
                        if (dateParts.length === 3) {
                            var formattedDate = dateParts[2] + '-' + dateParts[1] + '-' + dateParts[0];
                            $("#txtBirthDate").val(formattedDate);
                        } else {
                            $("#txtBirthDate").val(response.data.DateOfBirth);
                        }
                    }

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

    var employeeIdToDelete = 0;

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
                    showServerMessage("Xóa nhân viên thành công!", "success");
                } else {
                    showServerMessage("Xóa thất bại: " + response.message, "danger");
                }
                employeeIdToDelete = 0;
            },
            error: function (err) {
                $("#deleteConfirmModal").modal('hide');
                showServerMessage("Có lỗi xảy ra trong quá trình xóa!", "danger");
                employeeIdToDelete = 0;
            }
        });
    });

    function showServerMessage(msg, type) {
        var headerBg = type === "success" ? "bg-success" : "bg-danger";
        var icon = type === "success" ? "fa-check-circle" : "fa-exclamation-triangle";
        var title = type === "success" ? "Thông báo thành công" : "Thông báo lỗi";

        var modalHtml = `
            <div class="modal fade" id="dynamicNotifyModal" tabindex="-1" aria-hidden="true">
                <div class="modal-dialog modal-dialog-centered">
                    <div class="modal-content">
                        <div class="modal-header ${headerBg} text-white">
                            <h5 class="modal-title"><i class="fas ${icon}"></i> ${title}</h5>
                            <button type="button" class="close text-white" data-dismiss="modal" aria-label="Close">
                                <span aria-hidden="true">&times;</span>
                            </button>
                        </div>
                        <div class="modal-body fs-5" style="padding: 20px 24px; color: #1e293b; font-size: 1.05rem;">
                            ${msg}
                        </div>
                    </div>
                </div>
            </div>`;

        $("#serverMessage").html(modalHtml);

        $("#dynamicNotifyModal").modal('show');

        setTimeout(function () {
            $("#dynamicNotifyModal").modal('hide');
        }, 2000);
    }
});