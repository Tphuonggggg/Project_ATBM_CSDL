# NHOM 09 - Huong dan su dung ung dung Quan tri Oracle (WinForms)
- Chuong trinh chay theo luong LoginForm -> MainForm.
- MainForm co 6 tab: User, Role, Grant, Revoke, Xem quyen, Object Browser.
- Cac thao tac tao/sua/xoa/grant/revoke dung cac stored procedure trong file script SQL.

---
## 1. Buoc bat buoc dau tien: chay script SQL trong Oracle
Ung dung hien tai goi truc tiep cac procedure sau:
- sp_create_user
- sp_alter_user_password
- sp_drop_user
- sp_set_user_lock
- sp_create_role
- sp_drop_role
- sp_grant_role
- sp_revoke_role
- sp_grant_system_privilege
- sp_revoke_system_privilege
- sp_grant_object_privilege
- sp_revoke_object_privilege

Tat ca cac procedure tren nam trong file:

- WindowsFormsApp1/StoredProcedures.sql

Neu ban chua chay script nay, app se bao loi khi bam cac nut thao tac DDL/DCL.

### 1.1. Cach chay script bang SQL Developer

1. Dang nhap vao Oracle bang tai khoan co du quyen (lab thuong dung SYS voi SYSDBA) va servicename la XEPDB1.
2. Mo file WindowsFormsApp1/StoredProcedures.sql.
3. Run Script toan bo file (khuyen nghi dung che do script, khong chi run tung cau).
4. Dam bao moi procedure compile thanh cong.

### 1.2. Kiem tra nhanh da tao procedure chua

Chay cau lenh sau trong schema vua tao procedure:
```sql
SELECT object_name, status
FROM user_objects
WHERE object_type = 'PROCEDURE'
  AND object_name LIKE 'SP_%'
ORDER BY object_name;
```
Ket qua mong doi:
- Co du 12 dong procedure bat dau bang SP_.
- STATUS la VALID.
---

## 2. Yeu cau moi truong
- Windows.
- .NET Framework 4.7.2.
- Oracle Database dang chay.
- Tai khoan Oracle du quyen doc cac view DBA_* va thuc hien DDL/DCL.
- Thu vien Oracle.ManagedDataAccess da duoc restore theo packages.config.
Khuyen nghi trong moi truong lab: dang nhap SYS + SYSDBA de demo day du tinh nang.

---

## 3. Build va chay ung dung

1. Mo solution WindowsFormsApp1.slnx trong Visual Studio.
2. Build project WindowsFormsApp1.
3. Chay file exe:
   - WindowsFormsApp1/bin/Debug/NHOM09.exe
   - hoac WindowsFormsApp1/bin/Release/NHOM09.exe

Luong chay:

1. Program mo LoginForm.
2. Dang nhap thanh cong thi vao MainForm.
3. MainForm hien 6 tab quan tri.

---

## 4. Dang nhap LoginForm

Nhap cac truong:

- Host (vi du: localhost)
- Port (thuong: 1521)
- Service/PDB (vi du: XEPDB1)
- User
- Password
- Tick SYSDBA neu can

Bam Ket noi:

- Thanh cong: vao MainForm.
- That bai: hien MessageBox loi ket noi Oracle.

### 4.1. Che do bypass de demo giao dien

Ban hien tai co tai khoan bypass:

- User: demo
- Password: demo

Khi dang nhap bang cap nay:

- Ung dung vao che do xem truoc (offline demo).
- Van mo duoc MainForm.
- Cac thao tac truy van/grant/revoke se bao loi vi khong ket noi Oracle that.

Chi dung bypass de quay man hinh giao dien, khong dung de demo chuc nang database.

---

## 5. Tong quan 6 tab tren MainForm

1. User: quan ly user Oracle.
2. Role: quan ly role Oracle.
3. Grant: cap system privilege, role, object privilege.
4. Revoke: thu hoi quyen da cap.
5. Xem quyen: xem quyen cua 1 grantee.
6. Object Browser: duyet object theo schema va xem cot.

Luu y quan trong:

- Tab Grant/Revoke can bam nut Tai danh sach users / roles truoc khi thao tac.
- Tab Grant/Revoke tu dong nap danh sach system privilege tu SYSTEM_PRIVILEGE_MAP va role tu DBA_ROLES khi vao tab.

---

## 6. Huong dan chi tiet tung tab

### 6.1. Tab 1 - User

Chuc nang:

- Tai lai danh sach user tu DBA_USERS.
- Them user.
- Sua (doi password va lock/unlock account).
- Xoa user (DROP USER ... CASCADE).

Luong thao tac nhanh:

1. Bam Tai lai.
2. Nhap Username va Password, bam Them user.
3. Chon dong user tren grid de tu dien vao form.
4. Muon khoa/mo khoa thi chon Account status OPEN/LOCKED, bam Sua.
5. Muon xoa thi bam Xoa va xac nhan canh bao.

Vi du demo:

- Tao U_TEST voi password P@ssw0rd_123.
- Khoa tai khoan U_TEST.
- Mo khoa lai U_TEST.

### 6.2. Tab 2 - Role

Chuc nang:

- Tai lai danh sach role tu DBA_ROLES.
- Tao role thuong hoac password role.
- Xoa role.

Luong thao tac nhanh:

1. Nhap Role name.
2. Neu can password role thi tick Role co password va nhap password.
3. Bam Them role.
4. Chon role tren grid va bam Xoa role neu can.

Luu y:

- Nut Sua trong tab nay khong duoc ho tro (disable).

### 6.3. Tab 3 - Grant

Buoc chuan bi:

1. Bam Tai danh sach users / roles.
2. Chon Grantee.

Nhom 1 - Cap system privilege:

- Chon privilege (vi du CREATE SESSION).
- Tick WITH ADMIN OPTION neu can.
- Bam Cap system privilege.

Nhom 2 - Cap role:

- Chon role (vi du CONNECT hoac R_TEST).
- Tick WITH ADMIN OPTION neu can.
- Bam Cap role.

Nhom 3 - Cap object privilege:

- Chon Object type: TABLE, VIEW, PROCEDURE, FUNCTION.
- Chon Privilege:
  - TABLE/VIEW: SELECT, INSERT, UPDATE, DELETE.
  - PROCEDURE/FUNCTION: EXECUTE.
- Nhap Object theo dung dinh dang OWNER.OBJECT (vi du HR.EMPLOYEES).
- Neu SELECT/UPDATE theo cot thi nhap Columns (CSV), vi du SALARY, COMMISSION_PCT.
- Tick WITH GRANT OPTION neu can.
- Bam Cap quyen object.

### 6.4. Tab 4 - Revoke

Buoc chuan bi:

1. Bam Tai danh sach users / roles.
2. Chon Grantee.

Thu hoi system privilege:

- Chon privilege.
- Bam Thu hoi system privilege.
- Xac nhan hop thoai canh bao.

Thu hoi role:

- Chon role.
- Bam Thu hoi role.
- Xac nhan.

Thu hoi object privilege:

- Chon object type va privilege.
- Nhap OWNER.OBJECT.
- Neu thu hoi theo cot (SELECT/UPDATE) thi nhap cot vao Columns (CSV).
- Bam Thu hoi quyen object.
- Xac nhan.

### 6.5. Tab 5 - Xem quyen

Chuc nang:

- Nhap ten user/role (app tu chuyen IN HOA).
- Bam Xem quyen de hien thi 3 grid:
  - System privileges (DBA_SYS_PRIVS).
  - Roles da cap (DBA_ROLE_PRIVS).
  - Object va column privileges (DBA_TAB_PRIVS + DBA_COL_PRIVS).

Luu y:

- Radio User/Role chi de de nhin giao dien, query thuc te deu dua tren grantee name.

### 6.6. Tab 6 - Object Browser

Chuc nang:

- Tai owners tu DBA_USERS.
- Duyet object theo owner va loai object.
- Xem cot cua TABLE/VIEW tu DBA_TAB_COLUMNS.

Luong thao tac nhanh:
1. Bam Tai owners.
2. Chon owner.
3. Chon loai object.
4. Bam Duyet.
5. Chon dong object trong grid tren.
6. Neu la TABLE/VIEW, grid duoi se hien thi danh sach cot.

---
## 7. Kich ban demo end-to-end de nop do an

1. Dang nhap Oracle bang SYSDBA trong SQL Developer.
2. Chay file WindowsFormsApp1/StoredProcedures.sql.
3. Mo NHOM09.exe va dang nhap vao app.
4. Tab User: tao U_TEST.
5. Tab Role: tao R_TEST.
6. Tab Grant:
   - Cap CREATE SESSION cho U_TEST.
   - Cap R_TEST hoac CONNECT cho U_TEST.
   - Tuy chon cap SELECT tren HR.EMPLOYEES.
7. Tab Xem quyen: nhap U_TEST va kiem tra du lieu tren 3 grid.
8. Tab Revoke: thu hoi 1 quyen vua cap va kiem tra lai o tab Xem quyen.
9. Tab Object Browser: duyet owner HR va mo cot bang EMPLOYEES.

---
## 8. Loi thuong gap va cach xu ly
| Hien tuong | Nguyen nhan pho bien | Cach xu ly |
|---|---|---|
| Bam thao tac ma bao khong tim thay procedure | Chua chay StoredProcedures.sql hoac procedure INVALID | Chay lai script, kiem tra USER_OBJECTS |
| ORA-01031 insufficient privileges | Tai khoan dang nhap khong du quyen | Dung SYSDBA trong lab hoac cap them quyen |
| ORA-00988 missing or invalid password(s) | Password khong dat policy DB | Doi password manh hon theo policy |
| Khong load duoc users/roles, owners | Khong du quyen doc DBA_* hoac ket noi loi | Kiem tra quyen va ket noi |
| Grant/Revoke object loi | Sai dinh dang OWNER.OBJECT hoac object khong ton tai | Nhap dung OWNER.OBJECT, kiem tra object |

---
## 9. Luu y an toan
- Chi dung SYS AS SYSDBA trong moi truong duoc phep.
- Can than voi DROP USER ... CASCADE vi co the mat toan bo object cua user.
---

