# Tạo Solution và các project

<aside>
💡

Tạo Blank Solution có tên SV22T1020261

Bổ sung cho solution các project sau:

- SV22T1020261.Shop: project dạng [ASP.NET](http://ASP.NET) Core MVC
    - Là ứng dụng web dành cho KH
- SV22T1020261.Admin: project dạng [ASP.NET](http://ASP.NET) Core MVC
    - Trang web dành cho Nhân viên, admin
- SV22T1020261.BusinessLayers: project dạng Class Library
    - Nghiệp vụ, tác nghiệp
- SV22T1020261.DataLayers: project dạng Class Library
    - Xử lí dữ liệu
- SV22T1020261.Models: project dạng Class Library
    - Cung cấp các cấu trúc dữ liệu
</aside>

# Thiết kế Layout cho App Admin

<aside>
💡

- Sử dụng Theme AdminLTE4, Boostrap5
- Mở file Layout của ứng dụng, copy code HTML của file Layout.html sang file Layout
</aside>

# Bố cục trang web

<aside>
💡

- Header
- Sidebar
- Footer
- Content
    - Header
    - Content
</aside>

# Lưu ý

`~/` nghĩa là bắt đầu từ file gốc của ứng dụng (VS) đi vào

# Trong file Layout.cshtml

<aside>
💡

Bắt buộc chỉ có 1 lệnh @RenderBody() được đặt tại vị trí khi trang web sử dụng  layout thì nội dung được viết tại đúng vị trí đó

</aside>

# Demo

<aside>
💡

Ứng dụng web quản lý bán hàng dùng AdminLTE4 và Boostrap 5 để làm theme giao diện.
Yêu cầu: Xây dựng View có chức năng tìm kiếm và hiển thị khách hàng theo mẫu:

- Sử dụng icon của boostrap5
- Thông tin hiển thị về khách hàng bao gồm: Tên khách hàng, tên giao dịch, điện thoại, email, địa chỉ, tỉnh thành, trạng thái (khoá hay không khoá)
- Tại mỗi dòng có nút (link) để Edit hoặc Delete Khách hàng
- Nút (link) bổ sung Khách hàng nằm ở cuối dòng header
</aside>

# Các Controller và Action dự kiến (chức năng dự kiến)

## Home

- Home/Index

## Account

- Account/Login
- Account/Logout
- Account/ChangePassword

## Supplier

- Supplier/Index
- Supplier/Create
- Supplier/Edit/{id}
- Supplier/Delete/{id}:
    - Hàm delete có tham số Id → VD: delete(int id)

## Customer

- Customer/Index
    - Hiển thị danh sách Khách hàng dưới dạng phân trang
    - Tìm kiếm Khách hàng theo tên
    - Điều hướng đến các chức năng khác liên quan đến Khách hàng
- Customer/Create
- Customer/Edit/{id}
- Customer/Delete/{id}
- Customer/ChangePassword/{id}

## Shipper

- Shipper/Index
- Shipper/Create
- Shipper/Edit/{id}
- Shipper/Delete/{id}

## Employee

- Employee/Index
- Employee/Create
- Employee/Edit/{id}
- Employee/Delete/{id}
- Employee/ChangePassword/{id}
- Employee/ChangeRole/{id}

## Category

- Category/Index
- Category/Create
- Category/Edit/{id}
- Category/Delete/{id}

## Product

- Product/Index
- Product/Detail
- Product/Create
- Product/Edit/{id}
- Product/Delete/{id}
- Product/ListAttributes/{id}
- Product/CreateAttributes/{id}
- Product/EditAttributes/{id}?attributeId={attributeId}
- Product/DeleteAttributes/{id}?attributeId={attributeId}
- Product/ListPhotos/{id}
- Product/CreatePhotos/{id}
- Product/EditPhotos/{id}?photoId={photoId}
- Product/DeletePhotos/{id}?photoId={photoId}

## Order

- Order/Index
- Order/Create
- Order/Edit/{id}
- Order/Delete/{id}