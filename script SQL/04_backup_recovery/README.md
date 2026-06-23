# HƯỚNG DẪN CHI TIẾT: SAO LƯU & PHỤC HỒI (BACKUP & RECOVERY)
### PHÂN HỆ 2 - YÊU CẦU 4 (NHÓM 09)

Tài liệu này hướng dẫn chi tiết cách chạy kịch bản sao lưu Data Pump, giả lập sự cố sửa sai dữ liệu, tra cứu vết bằng nhật ký kiểm toán (Audit Log) và khôi phục nhanh dữ liệu bằng **Flashback Query**.

---

## 📋 DANH SÁCH CÁC FILE TRONG PHÂN HỆ

| Tên File | Loại | Tài khoản chạy | Chức năng chi tiết |
| :--- | :---: | :---: | :--- |
| **`00_prepare_backup_privileges.sql`** | SQL | `SYS AS SYSDBA` | Cấp các quyền cần thiết cho `CQ09` để chạy Data Pump và phân quyền Bypass chính sách VPD/OLS khi backup. |
| **`01_expdp_backup.bat`** | Script CMD | *(Chạy trong CMD)* | Tự động chạy lệnh `expdp` để sao lưu toàn bộ schema `CQ09` ra file dump `.dmp` (có tên kèm ngày giờ). |
| **`02_impdp_restore.bat`** | Script CMD | *(Chạy trong CMD)* | Chạy lệnh `impdp` để phục hồi lại dữ liệu từ file dump đã chọn, ghi đè lên các bảng hiện có. |
| **`03_demo_su_co.sql`** | SQL | `BS001` (Bác sĩ) | Giả lập sự cố: Bác sĩ đăng nhập và sửa sai liều dùng thuốc của bệnh án thành một chuỗi cảnh báo lỗi nguy hiểm. |
| **`04_check_audit_log.sql`** | SQL | **`CQ09`** | Tra cứu log kiểm toán Fine-Grained Audit (FGA) để tìm chính xác mốc thời gian và câu lệnh SQL gây ra sự cố. |
| **`05_flashback_restore.sql`** | SQL | **`CQ09`** | Thực hiện khôi phục dữ liệu liều dùng thuốc về trạng thái trước sự cố bằng công cụ Flashback Query. |
| **`06_rman_commands.txt`** | Text | *(Tham khảo)* | Tổng hợp các lệnh sao lưu và khôi phục cấp vật lý sử dụng Oracle Recovery Manager (RMAN). |

---

## ⚡ TRÌNH TỰ CÁC BƯỚC DEMO KỊCH BẢN (1-2-3-4-5-6)

### Điều kiện tiên quyết:
* Bạn đã hoàn tất thiết lập hệ thống y tế bằng file tổng hợp `00_run_all.sql` ở thư mục gốc.
* Tính năng kiểm toán (Audit) đã được bật trong Oracle (kiểm tra bằng lệnh `SHOW PARAMETER audit_trail;` và đảm bảo kết quả khác `NONE`).

---

### 🟢 BƯỚC 1: Chuẩn bị quyền sao lưu
Đăng nhập vào SQL Developer bằng tài khoản **`SYS AS SYSDBA`** và chạy file:
```sql
@".\script SQL\04_backup_recovery\00_prepare_backup_privileges.sql"
```
*Chức năng: Cấp vai trò export/import database và quyền đặc biệt `EXEMPT ACCESS POLICY` cho `CQ09` để quá trình export không bị VPD/OLS cản trở.*

---

### 🟢 BƯỚC 2: Tạo bản sao lưu dự phòng (Data Pump Export)
1. Mở cửa sổ **CMD** hoặc **PowerShell** và di chuyển đến thư mục gốc của dự án.
2. Thực thi file batch sau:
```cmd
"script SQL\04_backup_recovery\01_expdp_backup.bat"
```
*Kết quả: Oracle sẽ xuất toàn bộ dữ liệu schema `CQ09` ra một file `.dmp` và file `.log` lưu tại thư mục Data Pump mặc định của Oracle Server (thường là `DATA_PUMP_DIR`). Hãy nhớ hoặc ghi lại tên file dump vừa tạo.*

---

### 🟢 BƯỚC 3: Giả lập sự cố thay đổi dữ liệu trái phép
1. Đăng nhập vào SQL Developer hoặc SQL*Plus bằng tài khoản bác sĩ **`BS001` / `ATBM123`**.
2. Thực thi file script sau:
```sql
@".\script SQL\04_backup_recovery\03_demo_su_co.sql"
```
*Hiện tượng: Bác sĩ `BS001` cập nhật sai cột `LIEUDUNG` của thuốc `Metformin 1000mg` trong bệnh án `HSBA2024001` thành: `"SU CO DEMO - sai lieu nguy hiem, can phuc hoi"`.*

---

### 🟢 BƯỚC 4: Tra cứu Audit Log để tìm mốc thời gian sự cố
Đăng nhập bằng tài khoản quản trị dự án **`CQ09`** và chạy file:
```sql
@".\script SQL\04_backup_recovery\04_check_audit_log.sql"
```
*Kết quả: Màn hình sẽ hiển thị lịch sử kiểm toán FGA chi tiết. Hãy quan sát và ghi lại **thời điểm xảy ra sự cố (Audit Time)** và câu lệnh SQL gây lỗi của user `BS001`.*

---

### 🟢 BƯỚC 5: Khôi phục dữ liệu bằng Flashback Query
1. Mở file **`05_flashback_restore.sql`** bằng công cụ soạn thảo hoặc SQL Developer.
2. Sửa lại mốc thời gian tại dòng khai báo `RESTORE_TS` thành thời điểm **ngay trước khi xảy ra sự cố** (lấy mốc thời gian tìm được ở Bước 4 trừ đi khoảng 10 giây). Ví dụ:
   ```sql
   DEFINE RESTORE_TS = "2026-06-23 11:30:00"
   ```
3. Đăng nhập bằng tài khoản **`CQ09`** và thực thi file:
```sql
@".\script SQL\04_backup_recovery\05_flashback_restore.sql"
```
*Kết quả: Hệ thống sẽ tự động đối chiếu dữ liệu cũ thông qua Flashback Query và cập nhật đè lại liều dùng thuốc ban đầu. Bạn sẽ thấy liều dùng thuốc được khôi phục nguyên vẹn.*

---

### 🟢 BƯỚC 6: Khôi phục toàn bộ từ file dump Data Pump (Nếu cần thiết)
Trong trường hợp gặp sự cố hỏng hóc nặng hoặc muốn reset toàn bộ bảng về thời điểm backup ở Bước 2:
1. Mở cửa sổ **CMD** hoặc **PowerShell**.
2. Chạy file batch:
```cmd
"script SQL\04_backup_recovery\02_impdp_restore.bat"
```
3. Khi dấu nhắc lệnh yêu cầu, hãy nhập chính xác tên file dump đã tạo ở Bước 2 (ví dụ: `CQ09_backup_20260623_093000.dmp`) và nhấn Enter.
*Kết quả: Toàn bộ bảng dữ liệu của schema `CQ09` sẽ được khôi phục về trạng thái lúc backup.*

---

## 🔍 PHÂN TÍCH & SO SÁNH CÁC PHƯƠNG PHÁP SAO LƯU

| Phương pháp | Cơ chế hoạt động | Ưu điểm khi demo đồ án | Hạn chế |
| :--- | :--- | :--- | :--- |
| **Data Pump** (`expdp`/`impdp`) | Sao lưu logic (xuất cấu trúc & dữ liệu ra file dump). | Dễ thực hiện, tạo ra file backup vật lý rõ ràng để nộp. | Không khôi phục được chính xác thời điểm mong muốn (point-in-time) nếu chưa có bản backup tại thời điểm đó. |
| **Flashback Query** | Truy vấn trạng thái dữ liệu trong quá khứ thông qua phân vùng Undo. | Khôi phục cực nhanh các dòng dữ liệu bị sửa đổi sai mà không cần tắt Database hoặc phục hồi toàn bộ bảng. | Bị giới hạn bởi kích thước phân vùng Undo (`undo_retention`), dữ liệu quá cũ sẽ không flashback được. |
| **RMAN** (Recovery Manager) | Sao lưu vật lý toàn bộ file dữ liệu (datafiles, controlfile...). | Giải pháp tiêu chuẩn cho doanh nghiệp, hỗ trợ phục hồi hệ thống sau lỗi phần cứng nặng. | Phức tạp hơn, cần quyền hệ thống cao và cấu hình chế độ ghi log lưu trữ (`ARCHIVELOG`). |
