function updatePro(id) {
    var updatedName = document.getElementById(`ProductName-${id}`).value;
    var updateUrl = '../../Admin/UpdateProduct';

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
                    updateProductList(result.categories);
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

function AddPro() {
    var Name = document.getElementById(`InputName`).value;
    var updateUrl = '../../Admin/AddProduct';
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


                    updateProductList(result.categories);
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
function deleteProduct(id) {
    if (confirm("Bạn có chắc muốn xóa mục này không?")) {
        var updateUrl = '../../Admin/DeleteProduct';
        $.ajax({
            url: updateUrl,
            type: 'POST',
            data: { id: id },
            success: function (result) {
                if (result.success) {
                    updateProductList(result.categories);
                    
                } else {
                    alert("Không tìm thấy mục cần xóa!");
                }
            },
            error: function (xhr, status, error) {
                alert("Có lỗi xảy ra: " + xhr.status + " - " + xhr.responseText);
                console.log("Chi tiết lỗi: ", error);
            }
        });
    }
}
function updateProductList(categories) {
    // Chọn phần tử <tbody> để cập nhật nội dung
    var categoryTableBody = $('tbody');
    categoryTableBody.empty(); // Xóa tất cả nội dung cũ trong <tbody>

    // Duyệt qua từng danh mục và tạo các hàng mới
    $.each(categories, function (index, category) {
        categoryTableBody.append(`
            <tr id="viewRow-${category.ProductId}">
                <th scope="row">${category.ProductId}</th>
                <td>${category.ProductName}</td>
                <td scope="col" style="text-align: center;">
                    <button type="button" class="btn btn-secondary" onclick="editItem(${category.ProductId})">
                        <i class="bi bi-pencil-square"></i> Sửa
                    </button
                    <button type="button" class="btn btn-secondary" onclick="deleteProduct(${category.ProductId})">
                        <i class="bi bi-pencil-square"></i> Xóa
                    </button>
                </td>
            </tr>
            <tr id="editRow-${category.ProductId}" style="display:none;">
                <th scope="row">${category.ProductId}</th>
                <td>
                    <input type="text" id="categoryName-${category.ProductId}" value="${category.ProductName}" class="form-control">
                </td>
                <td scope="col" style="text-align: center;">
                    <button type="button" class="btn btn-secondary" onclick="updateItem(${category.ProductId})">
                        <i class="bi bi-save"></i> Lưu
                    </button>
                    <button type="button" class="btn btn-danger" onclick="cancelEdit(${category.ProductId})">
                        <i class="bi bi-x-circle"></i> Hủy
                    </button>
                    <button type="button" class="btn btn-secondary" onclick="deleteProduct(${category.ProductId})">
                        <i class="bi bi-pencil-square"></i> Xóa
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