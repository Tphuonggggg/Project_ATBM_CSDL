# Yêu cầu 4 - Backup và Recovery

Thư mục này chứa kịch bản backup, restore, tạo sự cố, đọc audit log và phục hồi dữ liệu cho schema `CQ09`.

## Mục tiêu demo

- Backup chủ động schema `CQ09` bằng Oracle Data Pump qua file batch.
- Backup tự động schema `CQ09` hằng ngày bằng `DBMS_SCHEDULER` gọi procedure dùng `DBMS_DATAPUMP`.
- Ghi kết quả backup tự động vào `CQ09.BACKUP_LOG`.
- Tạo một sự cố sửa sai dữ liệu đơn thuốc.
- Dùng audit/FGA để xác định user, SQL và thời điểm xảy ra sự cố.
- Dùng Flashback Query để khôi phục dữ liệu về trạng thái trước sự cố.
- Cung cấp lệnh RMAN tham khảo cho backup/recovery cấp database.

## File trong thư mục

| File | Mục đích |
|---|---|
| `00_setup.sql` | Cấp quyền backup/restore, cấp quyền `DATA_PUMP_DIR`, tạo `BACKUP_LOG`, procedure backup tự động và scheduler job |
| `01_backup_restore.bat` | Menu backup hoặc restore schema `CQ09` bằng Data Pump, nhập password khi chạy |
| `02_demo_recovery.sql` | Script hỗ trợ Flash Restore thủ công: đọc audit, preview flashback, hỏi xác nhận rồi mới phục hồi |
| `README.md` | Hướng dẫn này |

## Điều kiện trước khi chạy

Đã chạy các script chính ở thư mục `script SQL/`:

```sql
@"script SQL/StoredProcedures.sql"
@"script SQL/schema_data.sql"
@"script SQL/role.sql"
@"script SQL/VPD.sql"
@"script SQL/03_audit_setup.sql"
```

Audit nên được bật:

```sql
SHOW PARAMETER audit_trail;
```

Nếu kết quả là `NONE`, chạy bằng `SYS AS SYSDBA`:

```sql
ALTER SYSTEM SET audit_trail = DB, EXTENDED SCOPE = SPFILE;
```

Sau đó restart Oracle và chạy lại `03_audit_setup.sql`.

## Thứ tự demo đề xuất

### Bước 1 - Chuẩn bị quyền và backup tự động

Đăng nhập `SYS AS SYSDBA` và chạy:

```sql
@"script SQL/04_backup_recovery/00_setup.sql"
```

Script này kiểm tra `DATA_PUMP_DIR` trong PDB `XEPDB1`, cấp quyền Data Pump cho `CQ09`, tạo bảng `CQ09.BACKUP_LOG`, tạo procedure `CQ09.PRC_AUTO_EXPORT_SCHEMA`, và tạo job `CQ09.JOB_AUTO_EXPORT_SCHEMA` chạy hằng ngày.

### Bước 2 - Backup hoặc restore chủ động

Chạy file batch:

```bat
"script SQL\04_backup_recovery\01_backup_restore.bat"
```

Chọn:

- `1` để export schema `CQ09`.
- `2` để import schema `CQ09` từ dump file trong `DATA_PUMP_DIR`.

File batch không hardcode password. Người chạy nhập user, password, schema và connect string khi thực hiện.

### Bước 3 - Demo sự cố bằng app và phục hồi bằng giao diện

1. Mở app WinForms.
2. Đăng nhập bác sĩ `BS001`.
3. Sửa sai `LIEUDUNG` của đơn thuốc cần demo.
4. Đăng xuất hoặc mở phiên quản trị, đăng nhập `CQ09`, `SYSTEM` hoặc `SYS`.
5. Vào tab `8. Recovery`.
6. Bấm `Tai audit`.
7. Chọn dòng audit update `DONTHUOC` cần phục hồi.
8. Bấm `Restore audit da chon`.
9. App sẽ tự lấy `MAHSBA`, `NGAYDT`, `TENTHUOC` và `suggested_restore_ts` từ audit, tự preview giá trị hiện tại/giá trị cũ trong hộp xác nhận, rồi chỉ restore nếu người demo chọn `Yes`.

Tab Recovery chỉ phục hồi cột `LIEUDUNG` của `CQ09.DONTHUOC`. Danh sách audit được gom từ cả `UNIFIED_AUDIT_TRAIL` và `DBA_FGA_AUDIT_TRAIL`.

### Bước 4 - Demo Flash Restore thủ công bằng SQL

Nếu không dùng giao diện, chạy:

```sql
@"script SQL/04_backup_recovery/02_demo_recovery.sql"
```

Script này không tự gây sự cố nữa. Sự cố được tạo bằng app trước, sau đó script đọc audit, tính `RESTORE_TS`, preview dữ liệu flashback và chỉ restore nếu người chạy nhập `YES`.

Heuristic trừ 10 giây chỉ phù hợp cho demo. Nếu có nhiều cập nhật gần nhau, cần chọn mốc thời gian thủ công. Flashback Query cũng phụ thuộc `UNDO_RETENTION` đủ lớn và undo chưa bị ghi đè.

## Kiểm tra backup tự động

Kết quả chính của backup tự động nằm trong bảng:

```sql
SELECT TO_CHAR(run_time, 'YYYY-MM-DD HH24:MI:SS') AS run_time,
       dump_file,
       status,
       error_msg
FROM CQ09.BACKUP_LOG
ORDER BY run_time DESC;
```

Chạy thử job ngay, không cần đợi lịch hằng ngày:

```sql
BEGIN
    DBMS_SCHEDULER.RUN_JOB('CQ09.JOB_AUTO_EXPORT_SCHEMA', use_current_session => TRUE);
END;
/
```

Procedure `CQ09.PRC_AUTO_EXPORT_SCHEMA` gọi `DBMS_DATAPUMP.WAIT_FOR_JOB`, nên job scheduler chỉ kết thúc sau khi Data Pump export thật sự hoàn tất.

Có thể đối chiếu thêm trạng thái scheduler:

```sql
SELECT owner, job_name, enabled, state, repeat_interval
FROM dba_scheduler_jobs
WHERE owner = 'CQ09'
  AND job_name = 'JOB_AUTO_EXPORT_SCHEMA';

SELECT owner,
       job_name,
       status,
       TO_CHAR(log_date, 'YYYY-MM-DD HH24:MI:SS') AS log_time,
       error#,
       additional_info
FROM dba_scheduler_job_run_details
WHERE owner = 'CQ09'
  AND job_name = 'JOB_AUTO_EXPORT_SCHEMA'
ORDER BY log_date DESC;
```

## Lệnh kiểm tra nhanh sau phục hồi

```sql
SELECT MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG
FROM CQ09.DONTHUOC
WHERE MAHSBA = 'HSBA2024001'
ORDER BY NGAYDT, TENTHUOC;
```

Đọc log audit:

```sql
SELECT db_user,
       object_schema,
       object_name,
       policy_name,
       TO_CHAR(timestamp, 'YYYY-MM-DD HH24:MI:SS') AS audit_time,
       sql_text
FROM dba_fga_audit_trail
WHERE object_schema = 'CQ09'
ORDER BY timestamp DESC;
```

## RMAN tham khảo lý thuyết

Chạy RMAN trong CMD/PowerShell, không chạy trong SQL Developer:

```bat
rman target /
```

Backup toàn database:

```rman
BACKUP DATABASE;
```

Backup database kèm archive log:

```rman
BACKUP DATABASE PLUS ARCHIVELOG;
```

Restore/recover cơ bản:

```rman
SHUTDOWN IMMEDIATE;
STARTUP MOUNT;
RESTORE DATABASE;
RECOVER DATABASE;
ALTER DATABASE OPEN;
```

Ghi chú:

- RMAN phù hợp backup/restore toàn database.
- Để point-in-time recovery tốt cần cấu hình `ARCHIVELOG` và retention policy.
- Trong demo đồ án, Data Pump và Flashback Query thao tác nhanh hơn trên schema `CQ09`.

## So sánh phương pháp

| Phương pháp | Ưu điểm | Hạn chế |
|---|---|---|
| Data Pump `expdp/impdp` | Dễ demo, phù hợp backup schema/table, có dump rõ ràng | Không phải point-in-time recovery chính xác nếu không có dump đúng thời điểm |
| `DBMS_SCHEDULER` + `DBMS_DATAPUMP` | Đáp ứng backup tự động, có log trong `CQ09.BACKUP_LOG` | Vẫn phụ thuộc `DATA_PUMP_DIR`, quyền Oracle và dung lượng lưu dump |
| Flashback Query | Phục hồi nhanh một số dòng về thời điểm trước sự cố | Phụ thuộc undo retention, không thay thế backup thật |
| RMAN | Chuẩn Oracle cho backup/recovery toàn database | Cấu hình phức tạp hơn, cần quản lý archive log nếu muốn point-in-time recovery |

## Lỗi thường gặp

| Lỗi | Nguyên nhân | Cách xử lý |
|---|---|---|
| `ORA-39087 directory name is invalid` | Directory Data Pump chưa tồn tại hoặc sai tên | Kiểm tra `DATA_PUMP_DIR` trong PDB `XEPDB1`, rồi chạy lại `00_setup.sql` |
| `ORA-01031 insufficient privileges` | User thiếu quyền export/import, scheduler hoặc flashback | Chạy `00_setup.sql` bằng `SYS AS SYSDBA`; kiểm tra grant cho `CQ09` |
| `CQ09.BACKUP_LOG` có `ERROR` | Procedure Data Pump lỗi khi export | Xem `error_msg`, log file trong `DATA_PUMP_DIR`, và `DBA_SCHEDULER_JOB_RUN_DETAILS` |
| Không có audit log | Chưa bật `audit_trail` hoặc chưa chạy audit setup | Bật audit, restart Oracle, chạy lại `03_audit_setup.sql` |
| Flashback không thấy dữ liệu cũ | Undo retention không đủ | Dùng dump backup hoặc giảm khoảng thời gian giữa sự cố và restore |
| Import đè dữ liệu lỗi | Chế độ import chưa đúng | Kiểm tra tham số `TABLE_EXISTS_ACTION` trong `01_backup_restore.bat` |

## Thay đổi so với bản gốc

- Gộp 7 file thao tác thành 3 file chạy chính và README để giảm số bước demo.
- Thay hai batch riêng bằng một menu backup/restore và bỏ hardcode password.
- Thêm backup tự động bằng scheduler thay vì chỉ có backup chạy tay.
- Thêm `CQ09.BACKUP_LOG` để kiểm tra kết quả backup tự động rõ hơn scheduler log.
- Thêm tab `8. Recovery` trong app để demo Flash Restore bằng giao diện.
- Đổi `02_demo_recovery.sql` thành script hỗ trợ demo thủ công, có preview và xác nhận trước khi restore.
- Đưa lệnh RMAN vào README như phần tham khảo lý thuyết thay vì để file riêng.

## Kết luận demo

Kịch bản nộp nên trình bày theo chuỗi:

1. Có backup Data Pump chủ động và backup tự động.
2. Sự cố sửa sai dữ liệu được audit lại.
3. Audit log giúp xác định thời điểm và câu SQL gây lỗi.
4. Flashback Query khôi phục nhanh dòng bị sai.
5. Data Pump và RMAN là phương án phục hồi dự phòng theo mức schema hoặc database.
