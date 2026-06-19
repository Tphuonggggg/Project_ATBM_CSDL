# Hướng dẫn sử dụng phân hệ 2 - Nhóm 09

Tài liệu này hướng dẫn cài đặt, chạy script Oracle và demo ứng dụng WinForms cho phân hệ 2 của đồ án An toàn bảo mật HTTT.

## 1. Mục tiêu phân hệ

Phân hệ 2 xây dựng hệ thống quản lý dữ liệu y tế có kiểm soát truy cập và kiểm toán trên Oracle:

- Quản lý dữ liệu bệnh nhân, nhân viên, hồ sơ bệnh án, đơn thuốc, dịch vụ hỗ trợ chẩn đoán và thông báo.
- Phân quyền theo vai trò: điều phối viên, bác sĩ/y sĩ, kỹ thuật viên, bệnh nhân.
- Kiểm soát truy cập mức dòng/cột bằng RBAC, VPD và view bảo mật.
- Phát tán thông báo bằng Oracle Label Security.
- Audit/FGA các thao tác nhạy cảm.
- Backup, phục hồi và demo khôi phục dữ liệu sau sự cố.
- Ứng dụng WinForms để demo theo từng vai trò và màn hình quản trị DBA.

## 2. Yêu cầu môi trường

- Windows.
- Visual Studio có hỗ trợ .NET Framework WinForms.
- .NET Framework 4.8.
- Oracle Database có PDB `XEPDB1`.
- SQL Developer hoặc SQL*Plus.
- Oracle Managed Data Access đã restore theo `packages.config`.

Tài khoản demo trong script thường dùng mật khẩu:

```text
ATBM123
```

Khuyến nghị khi chạy script lần đầu: dùng `SYS AS SYSDBA`.

## 3. Cấu trúc thư mục quan trọng

```text
Project_ATBM_CSDL/
├─ HUONG_DAN_SU_DUNG.md
├─ README.md
├─ script SQL/
│  ├─ StoredProcedures.sql
│  ├─ schema_data.sql
│  ├─ role.sql
│  ├─ VPD.sql
│  ├─ RBAC.sql
│  ├─ OLS_setup.sql
│  ├─ 03_audit_setup.sql
│  ├─ 03_audit_test_actions.sql
│  ├─ 03_audit_read_logs.sql
│  └─ 04_backup_recovery/
└─ WindowsFormsApp1/
   ├─ WindowsFormsApp1.csproj
   ├─ LoginForm.cs
   ├─ MainForm.cs
   ├─ CoordinatorForm.cs
   ├─ DoctorForm.cs
   ├─ TechnicianForm.cs
   ├─ PatientForm.cs
   └─ OlsDemoForm.cs
```

## 4. Thứ tự chạy script Oracle

Chạy script theo đúng thứ tự sau để tránh thiếu user, role, view hoặc policy.

### Bước 1 - Tạo user quản trị và stored procedure quản trị

Đăng nhập bằng `SYS AS SYSDBA`, sau đó chạy:

```sql
@"script SQL/StoredProcedures.sql"
```

Script này tạo user/schema `CQ09`, cấp quyền cần thiết cho môi trường lab và tạo các procedure quản trị user/role/grant/revoke.

Kiểm tra nhanh:

```sql
SELECT object_name, status
FROM all_objects
WHERE owner = 'CQ09'
  AND object_type = 'PROCEDURE'
  AND object_name LIKE 'SP_%'
ORDER BY object_name;
```

Kết quả mong đợi: các procedure `SP_%` ở trạng thái `VALID`.

### Bước 2 - Tạo schema và dữ liệu mẫu

Chạy:

```sql
@"script SQL/schema_data.sql"
```

Script tạo các bảng chính:

- `NHANVIEN`
- `BENHNHAN`
- `HSBA`
- `HSBA_DV`
- `DONTHUOC`
- `THONGBAO`

Kiểm tra nhanh:

```sql
SELECT 'NHANVIEN' AS doi_tuong, COUNT(*) AS so_luong FROM CQ09.NHANVIEN
UNION ALL SELECT 'BENHNHAN', COUNT(*) FROM CQ09.BENHNHAN
UNION ALL SELECT 'HSBA', COUNT(*) FROM CQ09.HSBA
UNION ALL SELECT 'HSBA_DV', COUNT(*) FROM CQ09.HSBA_DV
UNION ALL SELECT 'DONTHUOC', COUNT(*) FROM CQ09.DONTHUOC
UNION ALL SELECT 'THONGBAO', COUNT(*) FROM CQ09.THONGBAO;
```

### Bước 3 - Tạo user và role nghiệp vụ

Chạy:

```sql
@"script SQL/role.sql"
```

Script tạo các role:

- `RL_DIEUPHOI`
- `RL_BACSI`
- `RL_KYTHUATVIEN`
- `RL_BENHNHAN`

Script cũng tạo user Oracle tương ứng với mã nhân viên/bệnh nhân trong dữ liệu mẫu.

Ví dụ tài khoản demo:

```text
NV001 / ATBM123
BS001 / ATBM123
KTV01 / ATBM123
BN000001 / ATBM123
```

### Bước 4 - Thiết lập VPD và view nghiệp vụ

Chạy:

```sql
@"script SQL/VPD.sql"
```

Script này là phần chính để demo phân quyền nghiệp vụ:

- Bác sĩ chỉ xem/sửa hồ sơ mình phụ trách.
- Kỹ thuật viên chỉ xem dịch vụ được phân công và chỉ cập nhật kết quả.
- Bệnh nhân chỉ xem hồ sơ cá nhân và cập nhật các cột được phép.
- Nhân viên chỉ xem hồ sơ cá nhân và cập nhật quê quán/số điện thoại.
- Điều phối viên có quyền điều phối bệnh nhân, hồ sơ, bác sĩ và kỹ thuật viên.

Các view ứng dụng sử dụng:

- `CQ09.VW_BENHNHAN`
- `CQ09.VW_NHANVIEN_CANHAN`
- `CQ09.VW_KTV_HSBA_DV`
- `CQ09.VW_BACSI_HSBA`
- `CQ09.VW_BACSI_BENHNHAN`
- `CQ09.VW_BACSI_DONTHUOC`
- `CQ09.VW_BACSI_HSBA_DV`
- `CQ09.VW_KTV_LIST`

### Bước 5 - Thiết lập OLS

Chạy bằng tài khoản có quyền SYS/OLS:

```sql
@"script SQL/OLS_setup.sql"
```

Script tạo policy OLS cho bảng `CQ09.THONGBAO`, tạo user demo `u1` đến `u8` và gán nhãn đọc tương ứng.

Tài khoản demo:

```text
u1 / ATBM123
u2 / ATBM123
...
u8 / ATBM123
```

### Bước 6 - Thiết lập audit

Chạy bằng `SYS AS SYSDBA`:

```sql
@"script SQL/03_audit_setup.sql"
```

Nếu `audit_trail = NONE`, cần bật audit và restart Oracle:

```sql
ALTER SYSTEM SET audit_trail = DB, EXTENDED SCOPE = SPFILE;
```

Sau đó chạy lại script audit.

Để tạo log demo:

```sql
@"script SQL/03_audit_test_actions.sql"
```

Để đọc log:

```sql
@"script SQL/03_audit_read_logs.sql"
```

### Bước 7 - Backup và recovery

Xem hướng dẫn chi tiết trong:

```text
script SQL/04_backup_recovery/README.md
```

Luồng chính:

1. Chuẩn bị quyền backup.
2. Export Data Pump.
3. Tạo sự cố demo.
4. Đọc audit log xác định thời điểm.
5. Flashback khôi phục dữ liệu.
6. Import dump khi cần phục hồi schema/table.

## 5. Build và chạy ứng dụng WinForms

Mở project bằng Visual Studio:

```text
WindowsFormsApp1/WindowsFormsApp1.csproj
```

Hoặc mở solution:

```text
WindowsFormsApp1.slnx
```

Sau đó:

1. Restore NuGet packages nếu Visual Studio chưa tự restore.
2. Build project `WindowsFormsApp1`.
3. Chạy ứng dụng.

Executable sau khi build thường nằm tại:

```text
WindowsFormsApp1/bin/Debug/NHOM09.exe
WindowsFormsApp1/bin/Release/NHOM09.exe
```

## 6. Đăng nhập ứng dụng

Màn hình login cần nhập:

- Host: ví dụ `localhost`
- Port: thường là `1521`
- Service/PDB: ví dụ `XEPDB1`
- User
- Password
- Tick `SYSDBA` nếu đăng nhập bằng SYS

Ứng dụng tự nhận diện vai trò qua `CQ09.V_MY_ACCOUNT` và mở form phù hợp.

| Tài khoản | Form mở ra | Mục đích demo |
|---|---|---|
| `SYS` hoặc `CQ09` | `MainForm` | Quản trị DBA: user, role, grant, revoke, xem quyền, object browser |
| `NV001` | `CoordinatorForm` | Điều phối bệnh nhân, HSBA, bác sĩ, KTV |
| `BS001` | `DoctorForm` | Bác sĩ xem/sửa HSBA, đơn thuốc, chỉ định dịch vụ |
| `KTV01` | `TechnicianForm` | KTV xem dịch vụ được giao, cập nhật kết quả |
| `BN000001` | `PatientForm` | Bệnh nhân xem/cập nhật thông tin cá nhân |
| `u1` đến `u8` | `OlsDemoForm` | Demo Oracle Label Security |

## 7. Chức năng theo từng vai trò

### 7.1. DBA/Admin

Màn hình `MainForm` có 6 tab:

1. User: tạo, đổi mật khẩu, khóa/mở khóa, xóa user.
2. Role: tạo role thường hoặc role có password, xóa role.
3. Grant: cấp system privilege, role, object privilege.
4. Revoke: thu hồi system privilege, role, object privilege.
5. Xem quyền: xem quyền hệ thống, role và object/column privilege của user/role.
6. Object Browser: duyệt owner, object và cột.

Trước khi dùng tab Grant/Revoke, bấm nút tải danh sách user/role.

### 7.2. Điều phối viên

Đăng nhập ví dụ:

```text
NV001 / ATBM123
```

Chức năng:

- Xem, thêm, sửa bệnh nhân.
- Tạo hồ sơ bệnh án.
- Phân công bác sĩ phụ trách.
- Tạo/chỉnh phân công dịch vụ hỗ trợ chẩn đoán cho kỹ thuật viên.

### 7.3. Bác sĩ/Y sĩ

Đăng nhập ví dụ:

```text
BS001 / ATBM123
```

Chức năng:

- Xem các HSBA do mình phụ trách.
- Cập nhật chẩn đoán, điều trị, kết luận.
- Thêm/sửa/xóa đơn thuốc trong phạm vi HSBA được phụ trách.
- Chỉ định dịch vụ chẩn đoán và phân công KTV.
- Xem/cập nhật tiền sử bệnh, tiền sử gia đình, dị ứng thuốc của bệnh nhân mình điều trị.
- Xem/cập nhật quê quán và số điện thoại cá nhân.

### 7.4. Kỹ thuật viên

Đăng nhập ví dụ:

```text
KTV01 / ATBM123
```

Chức năng:

- Chỉ xem các dịch vụ được phân công cho chính mình.
- Chỉ cập nhật cột `KETQUA`.
- Xem/cập nhật quê quán và số điện thoại cá nhân.

### 7.5. Bệnh nhân

Đăng nhập ví dụ:

```text
BN000001 / ATBM123
```

Chức năng:

- Xem thông tin cá nhân của chính mình.
- Cập nhật địa chỉ, tiền sử bệnh, tiền sử gia đình và dị ứng thuốc.
- Không được tự sửa mã bệnh nhân, họ tên, ngày sinh, CCCD.

### 7.6. OLS demo

Đăng nhập một trong các user:

```text
u1 / ATBM123
u2 / ATBM123
u3 / ATBM123
u4 / ATBM123
u5 / ATBM123
u6 / ATBM123
u7 / ATBM123
u8 / ATBM123
```

Ứng dụng mở màn hình `OlsDemoForm` và hiển thị các thông báo mà user được phép đọc từ `CQ09.THONGBAO`.

### 7.7. Hướng dẫn test yêu cầu 2 - OLS

Checklist trước khi test:

- Đã chạy đúng thứ tự các script: `StoredProcedures.sql`, `schema_data.sql`, `role.sql`, `VPD.sql`, `OLS_setup.sql`.
- Đang kết nối đúng PDB `XEPDB1`.
- `OLS_setup.sql` đã được chạy bằng tài khoản có quyền SYS/OLS để bật OLS, tạo policy, tạo user `u1` đến `u8` và gán nhãn.

Kết quả kỳ vọng khi đọc bảng `CQ09.THONGBAO`:

| User | Thông báo phải thấy | Số dòng |
|---|---|---:|
| `u1` | `t1`, `t2`, `t3`, `t4`, `t5`, `t6`, `t7` | 7 |
| `u2` | `t1`, `t3` | 2 |
| `u3` | `t1`, `t3` | 2 |
| `u4` | `t1` | 1 |
| `u5` | `t1` | 1 |
| `u6` | `t1`, `t3` | 2 |
| `u7` | `t1`, `t3`, `t4`, `t5`, `t6`, `t7` | 6 |
| `u8` | `t1`, `t6` | 2 |

Test thủ công bằng SQL*Plus hoặc SQL Developer:

```sql
CONNECT u1/ATBM123@XEPDB1
SELECT MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM
FROM CQ09.THONGBAO
ORDER BY MATHONGBAO;

CONNECT u4/ATBM123@XEPDB1
SELECT MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM
FROM CQ09.THONGBAO
ORDER BY MATHONGBAO;

CONNECT u8/ATBM123@XEPDB1
SELECT MATHONGBAO, NOIDUNG, NGAYGIO, DIADIEM
FROM CQ09.THONGBAO
ORDER BY MATHONGBAO;
```

Khi cần test đầy đủ, lặp lại cùng truy vấn cho `u1` đến `u8` và đối chiếu với bảng kết quả kỳ vọng phía trên.

Test bằng ứng dụng WinForms:

1. Mở app `NHOM09`.
2. Đăng nhập lần lượt `u1`, `u4`, `u7`, `u8` với mật khẩu `ATBM123`.
3. Kiểm tra màn hình `OlsDemoForm` hiển thị đúng số dòng và đúng danh sách mã thông báo theo bảng kỳ vọng.
4. Chụp màn hình phần số dòng hiển thị và danh sách mã thông báo để làm minh chứng demo.

Truy vấn kiểm tra cấu hình OLS sau khi chạy script:

```sql
SELECT POLICY_NAME, SCHEMA_NAME, TABLE_NAME
FROM DBA_SA_TABLE_POLICIES
WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY'
  AND SCHEMA_NAME = 'CQ09'
  AND TABLE_NAME = 'THONGBAO';

SELECT USER_NAME, MAX_READ_LABEL, DEF_LABEL
FROM DBA_SA_USER_LABELS
WHERE POLICY_NAME = 'OLS_THONGBAO_POLICY'
  AND USER_NAME IN ('U1','U2','U3','U4','U5','U6','U7','U8')
ORDER BY USER_NAME;
```

Lỗi thường gặp khi test OLS:

| Hiện tượng | Cách kiểm tra/xử lý |
|---|---|
| Không đăng nhập được `u1` đến `u8` | Chạy lại `OLS_setup.sql`, kiểm tra user đã được tạo và unlock. |
| User thấy sai danh sách thông báo | Kiểm tra policy đã apply lên `CQ09.THONGBAO` và nhãn user đã gán đúng bằng truy vấn cấu hình ở trên. |
| User không thấy dữ liệu | Kiểm tra `GRANT SELECT ON CQ09.THONGBAO TO uX` đã có, sau đó đăng xuất và đăng nhập lại. |
| Truy vấn báo lỗi OLS/LBACSYS | Chạy `OLS_setup.sql` bằng tài khoản đủ quyền SYS/OLS và kiểm tra Oracle edition có hỗ trợ OLS. |

## 8. Kịch bản demo đề xuất

### Kịch bản 1 - Quản trị user/role

1. Đăng nhập `SYS AS SYSDBA`.
2. Mở tab User, tạo user `U_TEST`.
3. Khóa/mở khóa user.
4. Mở tab Role, tạo role `R_TEST`.
5. Mở tab Grant, cấp `CREATE SESSION` hoặc role cho `U_TEST`.
6. Mở tab Xem quyền để kiểm tra.
7. Mở tab Revoke để thu hồi quyền.

### Kịch bản 2 - Điều phối

1. Đăng nhập `NV001/ATBM123`.
2. Tạo hoặc sửa một bệnh nhân.
3. Tạo HSBA mới.
4. Phân công bác sĩ.
5. Tạo dịch vụ và phân công KTV.

### Kịch bản 3 - Bác sĩ

1. Đăng nhập `BS001/ATBM123`.
2. Kiểm tra chỉ thấy HSBA của `BS001`.
3. Cập nhật chẩn đoán/điều trị/kết luận.
4. Thêm hoặc sửa đơn thuốc.
5. Chỉ định dịch vụ cho KTV.

### Kịch bản 4 - Kỹ thuật viên

1. Đăng nhập `KTV01/ATBM123`.
2. Kiểm tra chỉ thấy dịch vụ của `KTV01`.
3. Cập nhật kết quả dịch vụ.
4. Kiểm tra không sửa được dữ liệu ngoài cột `KETQUA`.

### Kịch bản 5 - Bệnh nhân

1. Đăng nhập `BN000001/ATBM123`.
2. Kiểm tra chỉ thấy thông tin của `BN000001`.
3. Cập nhật địa chỉ hoặc dị ứng thuốc.
4. Kiểm tra không có chức năng sửa mã, tên, ngày sinh, CCCD.

### Kịch bản 6 - OLS

1. Đăng nhập `u1`, kiểm tra thấy nhiều thông báo nhất.
2. Đăng nhập `u4`, kiểm tra chỉ thấy thông báo phù hợp nhãn của user.
3. Đăng nhập `u8`, kiểm tra thấy thông báo chung và thông báo đúng khoa/cơ sở.

### Kịch bản 7 - Audit và recovery

1. Chạy `03_audit_setup.sql`.
2. Chạy thao tác cập nhật nhạy cảm từ ứng dụng hoặc `03_audit_test_actions.sql`.
3. Đọc log bằng `03_audit_read_logs.sql`.
4. Tạo sự cố trong `04_backup_recovery/03_demo_su_co.sql`.
5. Xác định thời điểm bằng `04_check_audit_log.sql`.
6. Khôi phục bằng `05_flashback_restore.sql`.

## 9. Lỗi thường gặp

| Hiện tượng | Nguyên nhân thường gặp | Cách xử lý |
|---|---|---|
| Không đăng nhập được Oracle | Sai host/port/service/user/password | Kiểm tra `localhost:1521/XEPDB1`, listener và PDB |
| Không xác định được vai trò | Chưa chạy `schema_data.sql` hoặc `role.sql` | Chạy lại đúng thứ tự script |
| App báo không tìm thấy procedure | Chưa chạy `StoredProcedures.sql` | Chạy script và kiểm tra procedure `SP_%` |
| ORA-01031 insufficient privileges | User chạy script thiếu quyền | Dùng `SYS AS SYSDBA` cho bước hệ thống |
| Không thấy dữ liệu đúng vai trò | Chưa chạy `VPD.sql` hoặc role chưa gán | Chạy lại `role.sql`, `VPD.sql`, đăng nhập lại |
| OLS không chạy | Oracle chưa bật/cấu hình OLS | Chạy `OLS_setup.sql` bằng tài khoản đủ quyền |
| Audit không có log | `audit_trail` đang `NONE` hoặc chưa restart | Bật `audit_trail = DB, EXTENDED`, restart Oracle |
| Data Pump lỗi directory | Chưa tạo/grant directory backup | Chạy `00_prepare_backup_privileges.sql` |

## 10. Ghi chú khi nộp/demo

- Nên mở SQL Developer và ứng dụng WinForms song song.
- Trước khi demo, chạy lại script theo đúng thứ tự trên một PDB sạch.
- Chụp màn hình kiểm chứng cho VPD, OLS, Audit và Recovery.
- Không dùng tài khoản `SYS AS SYSDBA` cho nghiệp vụ thường ngày; chỉ dùng để setup/demo trong lab.
- `CQ09` được cấp quyền rộng để phục vụ đồ án và demo, không phải cấu hình production.
