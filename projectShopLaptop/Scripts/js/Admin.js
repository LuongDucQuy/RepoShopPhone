
        //@* UPDATE METHOD *@
    function updateItem(id) {
        var updatedName = document.getElementById(`categoryName-${id}`).value;
        var updateUrl = '../../Admin/UpdateCate';
    if (confirm("Bạn có chắc muốn cập nhật mục này không?")) {
        console.log(updateUrl);
    console.log("ID:", id, "Name:", updatedName);

    $.ajax({
        url: updateUrl,
    type: `POST`,
    data: {
        id: id,
    nameCate: updatedName
                },
    success: function (result) {
                    if (result.success) {
        alert("cap nhat thanh cong");
    console.log(result.categories);
    updateCategoryList(result.categories);
                    } else {
        alert(result.message || "Không tìm thấy mục cần cập nhật!");
                    }
                },
    error: function (xhr, status, error) {
        // Sử dụng xhr để lấy thông tin chi tiết về lỗi
        alert("Có lỗi xảy ra: " + xhr.status + " " + xhr.statusText);
    console.log("Chi tiết lỗi:", error); // Hoặc sử dụng error để kiểm tra thông tin
                }
            });
        }
    }
    function AddCate() {
        var Name = document.getElementById(`InputName`).value;

        var updateUrl = '../../Admin/AddCategory';
    if (confirm("Bạn có chắc muốn thêm mục này không?")) {


        $.ajax({
            url: updateUrl,
            type: `POST`,
            data: {
                nameCate: Name
            },
            success: function (result) {
                if (result.success) {
                    alert("them thanh cong");
                    console.log(result.categories);

                    $('#exampleModalCenter').modal('hide');
                    $('.modal-backdrop').remove();


                    updateCategoryList(result.categories);
                } else {
                    alert(result.message || "Không tìm thấy mục cần cập nhật!");
                }
            },
            error: function (xhr, status, error) {
                // Sử dụng xhr để lấy thông tin chi tiết về lỗi
                alert("Có lỗi xảy ra: " + xhr.status + " " + xhr.statusText);
                console.log("Chi tiết lỗi:", error); // Hoặc sử dụng error để kiểm tra thông tin
            }
        });
        }
    }


    //@* LOAD TABLE *@
    function updateCategoryList(categories) {
        // Chọn phần tử <tbody> để cập nhật nội dung
        var categoryTableBody = $('tbody');
        categoryTableBody.empty(); // Xóa tất cả nội dung cũ trong <tbody>

        // Duyệt qua từng danh mục và tạo các hàng mới
            $.each(categories, function (index, category) {
                categoryTableBody.append(`
            <tr id="viewRow-${category.CategoryId}">
                <th scope="row">${category.CategoryId}</th>
                <td>${category.CategoryName}</td>
                <td scope="col" style="text-align: center;">
                    <button type="button" class="btn btn-secondary" onclick="editItem(${category.CategoryId})">
                        <i class="bi bi-pencil-square"></i> Sửa
                    </button>
                </td>
            </tr>
            <tr id="editRow-${category.CategoryId}" style="display:none;">
                <th scope="row">${category.CategoryId}</th>
                <td>
                    <input type="text" id="categoryName-${category.CategoryId}" value="${category.CategoryName}" class="form-control">
                </td>
                <td scope="col" style="text-align: center;">
                    <button type="button" class="btn btn-secondary" onclick="updateItem(${category.CategoryId})">
                        <i class="bi bi-save"></i> Lưu
                    </button>
                    <button type="button" class="btn btn-danger" onclick="cancelEdit(${category.CategoryId})">
                        <i class="bi bi-x-circle"></i> Hủy
                    </button>
                </td>
            </tr>
        `);
        });
    }


            function editItem(cateId) {
                document.getElementById(`viewRow-${cateId}`).style.display = 'none';
            document.getElementById(`editRow-${cateId}`).style.display = '';
    }

            function cancelEdit(cateId) {
                document.getElementById(`viewRow-${cateId}`).style.display = '';
            document.getElementById(`editRow-${cateId}`).style.display = 'none';
    }