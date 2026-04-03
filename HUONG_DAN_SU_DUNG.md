# NHOM 09 — Hướng dẫn sử dụng ứng dụng Quản trị Oracle

Tài liệu mô tả cách cài đặt, kết nối và thao tác trên **ứng dụng Windows Forms** quản trị cơ sở dữ liệu Oracle (quản lý user/role, cấp quyền, xem và thu hồi quyền).

---

## 1. Giới thiệu

Ứng dụng cung cấp ba nhóm chức năng chính:

| Tab | Nội dung |
|-----|----------|
| **Quản lý User/Role** | Tạo / đổi mật khẩu / xóa user; tạo / xóa role; xem danh sách từ `dba_users`, `dba_roles`. |
| **Cấp quyền** | Cấp role, quyền hệ thống, hoặc quyền trên đối tượng (bảng, view, thủ tục, hàm); hỗ trợ cấp quyền theo cột cho `SELECT`/`UPDATE` trên TABLE/VIEW. |
| **Kiểm tra & thu hồi quyền** | Xem tổng hợp quyền (object, cột, system, role) và thu hồi theo dòng đang chọn. |

Dữ liệu hiển thị trên lưới là **kết quả truy vấn trực tiếp** từ data dictionary Oracle (không qua tiền xử lý đặc biệt).

---

## 2. Yêu cầu trước khi dùng

### 2.1. Phần mềm

- **Windows** với **.NET Framework 4.7.2** (hoặc tương thích).
- **Oracle Database** đang chạy và lắng nghe (ví dụ cổng `1521`).
- **Oracle.ManagedDataAccess** (đã tham chiếu trong project) — khi build, thư viện được copy cùng thư mục chạy.

### 2.2. Tài khoản kết nối

Ứng dụng thực thi lệnh DDL/DCL như `CREATE USER`, `GRANT`, `REVOKE`, truy vấn `dba_*`… Nên dùng tài khoản có **quyền DBA** hoặc tương đương (thường kết nối **`SYS AS SYSDBA`** hoặc user có quyền đọc `DBA_` views và thực hiện quản trị).

Nếu dùng user thường thiếu quyền, Oracle sẽ báo lỗi khi thao tác (ví dụ không xem được `dba_users`).

### 2.3. Thông tin kết nối cần biết

- **Máy chủ (host)**: ví dụ `localhost` hoặc IP máy chạy Oracle.
- **Cổng (port)**: mặc định thường `1521`.
- **Service name**: ví dụ `XEPDB1` (PDB trên Oracle XE), hoặc tên service do DBA cấp.
- **User / mật khẩu**: ví dụ `sys` + mật khẩu SYS (khi bật **SYSDBA**).

Chuỗi kết nối trong ứng dụng dạng: `host:port/serviceName`.

---

## 3. Chạy ứng dụng

1. Mở solution trong **Visual Studio**, chọn cấu hình **Debug** hoặc **Release**.
2. **Build** solution, sau đó **Start** (F5) hoặc chạy file `NHOM09.exe` trong thư mục `bin\Debug` hoặc `bin\Release`.

---

## 4. Màn hình đăng nhập (kết nối)

Khi khởi động, form **NHOM 09 - Đăng nhập** hiển thị trước (nhóm **Thông tin đăng nhập**).

| Trường | Ý nghĩa |
|--------|---------|
| **Máy chủ** | Host Oracle |
| **Port** | Cổng listener (thường 1521) |
| **Service/PDB** | Tên dịch vụ hoặc PDB (ví dụ `XEPDB1`) |
| **Tài khoản / Mật khẩu** | User và password |
| **Đăng nhập SYSDBA** | Bật khi cần kết nối kiểu SYS AS SYSDBA (quản trị toàn cục) |

Ô **Chuỗi kết nối** hiển thị chuỗi sẽ dùng (để kiểm tra, không nên chia sẻ công khai).

- Bấm **Kết nối**: ứng dụng thử mở kết nối; thành công thì vào màn hình chính.
- Bấm **Hủy**: thoát ứng dụng.

**Lưu ý:** Mật khẩu không được lưu cứng an toàn trong mã nguồn; file `App.config` trong repo chỉ là ví dụ — thay bằng thông tin thật trên máy bạn và **không commit mật khẩu** lên Git.

---

## 5. Màn hình chính — menu **Hệ thống**

| Mục menu | Chức năng |
|----------|-----------|
| **Kết nối...** | Mở lại form đăng nhập để đổi chuỗi kết nối (phiên làm việc hiện tại dùng chuỗi mới sau khi đóng form). |
| **Làm mới** | Tải lại dữ liệu cho **tab đang mở** (User/Role, danh sách cấp quyền, hoặc lưới kiểm tra quyền — tùy tab). |
| **Đăng xuất** | Đóng cửa sổ chính (kết thúc phiên). |

Trên tab **Quản lý User/Role**, nút **Làm mới danh sách** chỉ làm mới hai lưới user và role.

---

## 6. Tab «Quản lý User/Role»

Gồm hai vùng: **Người dùng** (trái) và **Vai trò** (phải).

### 6.1. Người dùng

- **Tài khoản / Mật khẩu**: nhập user và mật khẩu cần tạo hoặc đổi.
- **Tạo mới**: thực hiện `CREATE USER` với mật khẩu đã nhập.  
  - Không được để trống tên hoặc mật khẩu.  
  - Mật khẩu phải thỏa **chính sách mật khẩu** của Oracle (profile) nếu có — nếu không đạt, Oracle có thể báo lỗi khác (không chỉ thông báo phía ứng dụng).
- **Sửa (đổi mật khẩu)**: `ALTER USER ... IDENTIFIED BY ...` — cần nhập **mật khẩu mới** đầy đủ.
- **Xóa**: `DROP USER ... CASCADE` — xóa user và đối tượng trong schema (cẩn thận dữ liệu mất vĩnh viễn).

Lưới bên dưới liệt kê user từ `dba_users` (cột hiển thị phụ thuộc truy vấn trong code, ví dụ `USERNAME`, `ACCOUNT_STATUS`, `CREATED`).

### 6.2. Vai trò

- **Tên vai trò**: tên role cần tạo hoặc xóa.
- **Tạo mới**: `CREATE ROLE`.
- **Xóa**: `DROP ROLE`.

Lưới hiển thị role từ `dba_roles`.

Sau mỗi thao tác thành công, danh sách có thể được làm mới tự động; bạn vẫn có thể bấm **Làm mới danh sách** nếu cần.

---

## 7. Tab «Cấp quyền»

### 7.1. Chọn đối tượng nhận quyền (grantee)

- Combo **Đối tượng nhận** liệt kê user và role (từ `dba_users` ∪ `dba_roles`).
- Bấm **Tải danh sách người dùng / vai trò** (hoặc menu **Hệ thống → Làm mới** khi đang ở tab này) nếu vừa tạo user/role mới mà chưa thấy trong danh sách.

### 7.2. Ba kiểu cấp quyền (radio)

1. **Cấp role**  
   Chọn một trong các role có sẵn trong combo (ví dụ `CONNECT`, `RESOURCE`, `DBA`).  
   - Tùy chọn **WITH ADMIN OPTION** tương ứng `WITH ADMIN OPTION` khi `GRANT role`.

2. **Quyền hệ thống**  
   Chọn quyền như `CREATE SESSION`, `CREATE USER`, `GRANT ANY ROLE`, …  
   - **WITH ADMIN OPTION** khi cần.

3. **Quyền trên đối tượng**  
   - **Loại đối tượng**: Bảng (TABLE), Khung nhìn (VIEW), Thủ tục (PROCEDURE), Hàm (FUNCTION).  
   - **Quyền**: với TABLE/VIEW — `SELECT`, `INSERT`, `UPDATE`, `DELETE`; với PROCEDURE/FUNCTION — `EXECUTE`.  
   - **Đối tượng**: nhập hoặc chọn dạng **`SCHEMA.TÊN_ĐỐI_TƯỢNG`** (ví dụ `HR.EMPLOYEES`).  
   - **Tải danh sách đối tượng**: lọc theo ô **Lọc (tùy chọn)** nếu cần, nạp từ `dba_tables` / `dba_views` / `dba_objects` tùy loại.  
   - **Quyền theo cột**: chỉ áp dụng khi loại là **TABLE hoặc VIEW** và quyền là **SELECT** hoặc **UPDATE**.  
     - Bấm **Cấp quyền mức cột...** để mở hộp thoại chọn cột (lấy từ `dba_tab_columns`).  
     - Các cột hiển thị trong ô **Cột (nếu có)** dạng `COT1, COT2`.  
   - **Cho phép cấp lại (WITH GRANT OPTION)**: khi bật, thêm `WITH GRANT OPTION` cho quyền trên đối tượng; với **cấp role** / **quyền hệ thống** thì dùng **WITH ADMIN OPTION** (theo logic trong ứng dụng).

### 7.3. Thực hiện cấp quyền

Bấm **Thực hiện cấp quyền (GRANT)**. Nếu Oracle báo lỗi (thiếu quyền, đối tượng không tồn tại, cú pháp…), thông báo hiển thị trong hộp thoại **NHOM 09 - Lỗi**.

---

## 8. Tab «Kiểm tra & thu hồi quyền»

### 8.1. Xem quyền

- Truy vấn hợp nhất từ `dba_tab_privs`, `dba_col_privs`, `dba_sys_privs`, `dba_role_privs` (cột như `grantee`, `priv`, `obj`, `cols`, `opt` — đúng theo truy vấn trong ứng dụng).
- Ô **Lọc theo tài khoản / vai trò**: nhập tên user hoặc role (thường viết **IN HOA** như trong dictionary) để chỉ xem các dòng có `grantee` khớp; để trống thì xem tất cả.  
  *(Combo **Loại (User/Role)** trên form không tham gia vào điều kiện SQL trong phiên bản hiện tại.)*

Bấm **Làm mới** để tải lại lưới.

### 8.2. Thu hồi (revoke)

- Chọn **một dòng** trên lưới tương ứng quyền cần thu hồi.
- Bấm **Thu hồi dòng chọn**: ứng dụng tạo `REVOKE` phù hợp:
  - Có đối tượng: `REVOKE ... ON schema.object FROM ...`
  - Quyền cột `SELECT`/`UPDATE`: `REVOKE priv(cột) ON ...`
  - Chỉ system privilege hoặc role: `REVOKE ... FROM ...`

Sau khi thu hồi thành công, danh sách nên được làm mới lại.

---

## 9. Lỗi thường gặp và gợi ý

| Hiện tượng | Gợi ý |
|------------|--------|
| **Không kết nối được** | Kiểm tra Oracle đã chạy, host/port/service đúng, firewall, tài khoản/mật khẩu; PDB đúng tên (`XEPDB1` thay vì `ORCL` tùy cài đặt). |
| **ORA-00988 (mật khẩu)** | Đảm bảo mật khẩu không rỗng; đặt mật khẩu đủ mạnh theo profile Oracle. Ứng dụng đã format mật khẩu đúng cho `IDENTIFIED BY`. |
| **ORA-01031 hoặc không xem được DBA_*** | User kết nối thiếu quyền — dùng SYSDBA hoặc user được cấp quyền quản trị phù hợp. |
| **Không thấy user/role mới ở tab Cấp quyền** | Bấm **Tải danh sách người dùng / vai trò** hoặc menu **Làm mới**. |
| **Grant object sai tên** | Luôn dùng `OWNER.TÊN` đúng chữ hoa/thường theo Oracle (thường schema và tên object viết hoa trong dictionary). |

---

## 10. Bảo mật và môi trường thực hành

- Chỉ dùng tài khoản **SYS / SYSDBA** trong môi trường **học tập / lab** có kiểm soát.
- Không đưa mật khẩu thật vào Git hoặc ảnh chụp màn hình công khai.
- Trước khi `DROP USER CASCADE` hoặc revoke hàng loạt, nên xác nhận đúng đối tượng.

---

## 11. Tóm tắt luồng làm việc điển hình

1. Mở ứng dụng → nhập thông tin kết nối → **Kết nối**.  
2. Tab **Quản lý User/Role**: tạo user, (tuỳ chọn) tạo role.  
3. Tab **Cấp quyền**: chọn grantee → chọn loại quyền → cấp role hoặc quyền system/object.  
4. Tab **Kiểm tra & thu hồi quyền**: xác nhận quyền đã có; nếu cần, chọn dòng và **Thu hồi dòng chọn**.

---

*Tài liệu đi kèm project **NHOM 09** — Quản trị CSDL Oracle (WinForms). Cập nhật theo phiên bản mã nguồn hiện tại.*
