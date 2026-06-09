# Yeu Cau 4 - Backup va Recovery

Thu muc nay hien thuc kich ban backup/restore cho schema `CQ09`.

## Thu tu demo de nop

1. Chay `00_prepare_backup_privileges.sql` bang SYS AS SYSDBA de CQ09 backup duoc du lieu dang bi VPD/OLS bao ve.
2. Chay `01_expdp_backup.bat` de backup schema `CQ09` bang Data Pump.
3. Dam bao da chay `../03_audit_setup.sql`.
4. Chay `03_demo_su_co.sql` de tao su co: `BS001` sua sai lieu dung don thuoc.
5. Chay `04_check_audit_log.sql` de lay thoi diem va SQL bi audit.
6. Sua bien `RESTORE_TS` trong `05_flashback_restore.sql` thanh thoi diem truoc su co.
7. Chay `05_flashback_restore.sql` bang `CQ09/ATBM123` hoac DBA de phuc hoi dung lieu dung.
8. Neu can phuc hoi ca schema tu file dump, chay `02_impdp_restore.bat`.

## Danh gia ngan gon

| Phuong phap | Uu diem | Nhuoc diem |
|---|---|---|
| Data Pump expdp/impdp | De demo, backup/restore theo schema/table, phu hop nop do an | Khong phuc hoi chinh xac den tung thoi diem neu khong co ban dump phu hop |
| RMAN | Chuan Oracle, manh, backup/recovery toan database | Kho cau hinh hon, can quan ly archive log neu muon point-in-time recovery |
| Flashback Query | Phuc hoi nhanh mot so dong ve thoi diem truoc su co, gan tot voi audit log | Phu thuoc undo retention, khong thay the backup that |

Ket luan: dung Data Pump lam backup chinh, dung audit log de xac dinh su co, va dung Flashback de phuc hoi nhanh cac dong bi sua sai.
