using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SV22T1020261.BusinessLayers;
using SV22T1020261.Models.Common;
using SV22T1020261.Models.HR;

namespace SV22T1020261.Admin.Controllers
{
    /// <summary>
    /// Các chức năng liên quan đến Nhân viên
    /// </summary>
    [Authorize(Roles = $"{WebUserRoles.DataManager},{WebUserRoles.Administrator}")]
    public class EmployeeController : BaseSearchController
    {
        /// <summary>
        /// Tên của biến lưu trữ điều kiện tìm kiếm của nhân viên trong Session
        /// </summary>
        private const string SEARCH_INPUT = "EmployeeSearchInput";

        /// <summary>
        /// Nhập đầu vào tìm kiếm -> Hiển thị kết quả tìm kiếm
        /// Phần tìm kiếm này sẽ được giao cho hàm khác thực hiện
        /// </summary>
        /// <returns></returns>
        public IActionResult Index()
        {
            var input = GetSearchInput(SEARCH_INPUT);
            return View(input);
        }

        /// <summary>
        /// Tìm kiếm và trả về kết quả
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Search(PaginationSearchInput input)
        {
            var result = await HRDataService.ListEmployeesAsync(input);
            ApplicationContext.SetSessionData(SEARCH_INPUT, input);
            return View(result);
        }

        /// <summary>
        /// Bổ sung nhân viên
        /// </summary>
        /// <returns></returns>
        public IActionResult Create()
        {
            ViewBag.Title = "Bổ sung nhân viên";
            var model = new Employee()
            {
                EmployeeID = 0,
                IsWorking = true
            };
            return View("Edit", model);
        }

        /// <summary>
        /// Cập nhật thông tin nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần cập nhật</param>
        /// <returns></returns>
        public async Task<IActionResult> Edit(int id)
        {
            ViewBag.Title = "Cập nhật thông tin nhân viên";
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> SaveData(Employee data, IFormFile? uploadPhoto)
        {
            try
            {
                ViewBag.Title = data.EmployeeID == 0 ? "Bổ sung nhân viên" : "Cập nhật thông tin nhân viên";

                //Kiểm tra dữ liệu đầu vào: FullName và Email là bắt buộc, Email chưa được sử dụng bởi nhân viên khác
                if (string.IsNullOrWhiteSpace(data.FullName))
                    ModelState.AddModelError(nameof(data.FullName), "Vui lòng nhập họ tên nhân viên");

                if (string.IsNullOrWhiteSpace(data.Email))
                    ModelState.AddModelError(nameof(data.Email), "Vui lòng nhập email nhân viên");
                else if (!await HRDataService.ValidateEmployeeEmailAsync(data.Email, data.EmployeeID))
                    ModelState.AddModelError(nameof(data.Email), "Email đã được sử dụng bởi nhân viên khác");

                if (!ModelState.IsValid)
                    return View("Edit", data);

                //Xử lý upload ảnh
                if (uploadPhoto != null)
                {
                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(uploadPhoto.FileName)}";
                    var filePath = Path.Combine(ApplicationContext.WWWRootPath, "images/employees", fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await uploadPhoto.CopyToAsync(stream);
                    }
                    data.Photo = fileName;
                }

                //Tiền xử lý dữ liệu trước khi lưu vào database
                if (string.IsNullOrEmpty(data.Address)) data.Address = "";
                if (string.IsNullOrEmpty(data.Phone)) data.Phone = "";
                if (string.IsNullOrEmpty(data.Photo)) data.Photo = "nophoto.png";

                //Lưu dữ liệu vào database (bổ sung hoặc cập nhật)
                if (data.EmployeeID == 0)
                {
                    await HRDataService.AddEmployeeAsync(data);
                }
                else
                {
                    await HRDataService.UpdateEmployeeAsync(data);
                }
                return RedirectToAction("Index");
            }
            catch //(Exception ex)
            {
                //TODO: Ghi log lỗi căn cứ vào ex.Message và ex.StackTrace
                ModelState.AddModelError(string.Empty, "Hệ thống đang bận hoặc dữ liệu không hợp lệ. Vui lòng kiểm tra dữ liệu hoặc thử lại sau");
                return View("Edit", data);
            }
        }


        /// <summary>
        /// Xoá nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần xoá</param>
        /// <returns></returns>
        public async Task<IActionResult> Delete(int id)
        {
            //Nếu method là POST thì xoá
            if (Request.Method == "POST")
            {
                try
                {
                    await HRDataService.DeleteEmployeeAsync(id);
                    return RedirectToAction("Index");
                }
                catch
                {
                    //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                    ModelState.AddModelError("Error", "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
                    return View();
                }
            }

            //GET: Hiển thị thông tin khách hàng cần xoá 
            var model = await HRDataService.GetEmployeeAsync(id);
            if (model == null)
                return RedirectToAction("Index");

            ViewBag.CanDelete = !await HRDataService.IsUsedEmployeeAsync(id);

            return View(model);
            //return View();
        }

        /// <summary>
        /// Đổi mật khẩu của nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần đổi mật khẩu</param>
        /// <returns></returns>
        [HttpGet]
        public async Task<IActionResult> ChangePassword(int id)
        {
            var model = await HRDataService.GetEmployeeAsync(id);
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ChangePassword(int Id, string Email, string NewPassword, string ConfirmPassword)
        {
            var model = await HRDataService.GetEmployeeAsync(Id);
            try
            {
                var kt = await HRDataService.ValidateEmployeeEmailAsync(Email, Id);
                if (!kt)
                {
                    ModelState.AddModelError(nameof(Email), "Email không tồn tại hoặc đã được sử dụng bởi nhân viên khác");
                    return View(model);
                }

                // ===== VALIDATION =====

                if (string.IsNullOrWhiteSpace(NewPassword))
                    ModelState.AddModelError("NewPassword", "Vui lòng nhập mật khẩu mới");

                if (string.IsNullOrWhiteSpace(ConfirmPassword))
                    ModelState.AddModelError("ConfirmPassword", "Vui lòng xác nhận mật khẩu");

                if (NewPassword != ConfirmPassword)
                    ModelState.AddModelError("ConfirmPassword", "Mật khẩu xác nhận không khớp");

                if (!ModelState.IsValid)
                    return View(model);

                string hashedPassword = CryptHelper.HashMD5(NewPassword);
                var isValid = await SecurityDataService.ChangePasswordAsync(Email, hashedPassword);
                if (!isValid)
                {
                    ModelState.AddModelError("NewPassword", "Mật khẩu hiện tại không đúng");
                    return View(model);
                }
                ViewBag.Message = "Đổi mật khẩu thành công";
            }
            catch
            {
                //Ghi log lỗi dựa vào thông tin trong Exception (ex.Message, ex.StackTrace)
                ModelState.AddModelError(string.Empty, "Hệ thống hiện đang bận, vui lòng thử lại sau vài phút");
            }
            return View(model);
        }

        /// <summary>
        /// Đổi vai trò (phân quyền) của nhân viên
        /// </summary>
        /// <param name="id">Mã nhân viên cần đổi vai trò</param>
        /// <returns></returns>
        public async Task<IActionResult> ChangeRole(int id)
        {
            var employee = await HRDataService.GetEmployeeAsync(id);
            if (employee == null) return NotFound();

            // Lấy danh sách các quyền hiện tại của nhân viên này
            var currentRoles = await SecurityDataService.GetRoleNamesUserAsync(id);

            // Truyền vào ViewBag để View sử dụng
            ViewBag.CurrentRoles = currentRoles;

            return View(employee);
        }
        [HttpPost]
        public async Task<IActionResult> ChangeRole(int employeeId, List<string> roles)
        {
            // Gộp mảng thành chuỗi: "admin,sales"
            string roleString = string.Join(",", roles);

            // Gọi service cập nhật vào Database
            await HRDataService.UpdateUserRolesAsync(employeeId, roleString);

            var employee = await HRDataService.GetEmployeeAsync(employeeId);
            
            // Lấy danh sách các quyền hiện tại của nhân viên này
            var currentRoles = await SecurityDataService.GetRoleNamesUserAsync(employeeId);
            
            // Truyền vào ViewBag để View sử dụng
            ViewBag.CurrentRoles = currentRoles;

            ViewBag.Message = "Phân quyền thành công";
            return View(employee);
        }

    }
}
