-- =============================================================
-- CSC12001 - AN TOAN BAO MAT DU LIEU TRONG HTTT
-- PHAN HE 2: UNG DUNG QUAN LY DU LIEU Y TE
-- FILE 01: HOAN THIEN SCHEMA + SAMPLE DATA
-- Luu y: File nay CHI tao schema va du lieu mau, KHONG tao user/role.
-- Chay truoc file User/role.sql
-- =============================================================

SET DEFINE OFF;
SET SERVEROUTPUT ON;

PROMPT ===== 0. XOA CAC DOI TUONG CU NEU TON TAI =====

BEGIN
    FOR v IN (
        SELECT view_name FROM user_views
        WHERE view_name IN ('V_NGUOIDUNG_HE_THONG','V_MY_ACCOUNT')
    ) LOOP
        EXECUTE IMMEDIATE 'DROP VIEW ' || v.view_name;
        DBMS_OUTPUT.PUT_LINE('Dropped view: ' || v.view_name);
    END LOOP;
END;
/

BEGIN
    FOR t IN (
        SELECT table_name FROM user_tables
        WHERE table_name IN ('DONTHUOC','HSBA_DV','HSBA','BENHNHAN','NHANVIEN','THONGBAO')
        ORDER BY CASE table_name
            WHEN 'DONTHUOC' THEN 1
            WHEN 'HSBA_DV' THEN 2
            WHEN 'HSBA' THEN 3
            WHEN 'BENHNHAN' THEN 4
            WHEN 'NHANVIEN' THEN 5
            WHEN 'THONGBAO' THEN 6
        END
    ) LOOP
        EXECUTE IMMEDIATE 'DROP TABLE ' || t.table_name || ' CASCADE CONSTRAINTS PURGE';
        DBMS_OUTPUT.PUT_LINE('Dropped table: ' || t.table_name);
    END LOOP;
END;
/

BEGIN
    FOR s IN (
        SELECT sequence_name FROM user_sequences
        WHERE sequence_name IN ('SEQ_NHANVIEN','SEQ_BENHNHAN','SEQ_HSBA','SEQ_THONGBAO')
    ) LOOP
        EXECUTE IMMEDIATE 'DROP SEQUENCE ' || s.sequence_name;
        DBMS_OUTPUT.PUT_LINE('Dropped sequence: ' || s.sequence_name);
    END LOOP;
END;
/

PROMPT ===== 1. TAO BANG, KHOA, SEQUENCE, INDEX =====

-- =============================================================
-- 1. BANG NHANVIEN
-- =============================================================
CREATE TABLE NHANVIEN (
    MANV        VARCHAR2(10)  NOT NULL,
    HOTEN       NVARCHAR2(100) NOT NULL,
    PHAI        NCHAR(3)       CHECK (PHAI IN (N'Nam', N'Nữ')),
    NGAYSINH    DATE,
    CMND        VARCHAR2(12),
    QUEQUAN     NVARCHAR2(200),
    SODT        VARCHAR2(15),
    VAITRO      NVARCHAR2(30) NOT NULL
                  CHECK (VAITRO IN (
                    N'Điều phối viên',
                    N'Bác sĩ/Y sĩ',
                    N'Kỹ thuật viên',
                    N'Bệnh nhân'
                  )),
    CHUYENKHOA  NVARCHAR2(100),
    CONSTRAINT PK_NHANVIEN PRIMARY KEY (MANV)
);

-- =============================================================
-- 2. BANG BENHNHAN
-- =============================================================
CREATE TABLE BENHNHAN (
    MABN            VARCHAR2(10)   NOT NULL,
    TENBN           NVARCHAR2(100) NOT NULL,
    PHAI            NCHAR(3)       CHECK (PHAI IN (N'Nam', N'Nữ')),
    NGAYSINH        DATE,
    CCCD            VARCHAR2(12),
    SONHA           NVARCHAR2(50),
    TENDUONG        NVARCHAR2(100),
    QUANHUYEN       NVARCHAR2(100),
    TINHTP          NVARCHAR2(100),
    TIENSUBENH    NCLOB,
    TIENSUBENHGD   NCLOB,
    DIUNGTHUOC       NVARCHAR2(500),
    CONSTRAINT PK_BENHNHAN PRIMARY KEY (MABN)
);

-- =============================================================
-- 3. BANG HSBA (Ho So Benh An)
-- =============================================================
CREATE TABLE HSBA (
    MAHSBA      VARCHAR2(15)   NOT NULL,
    MABN        VARCHAR2(10)   NOT NULL,
    NGAY        DATE           NOT NULL,
    CHANDOAN    NCLOB,
    DIEUTRI     NCLOB,
    MABS        VARCHAR2(10)   NOT NULL,
    MAKHOA      NVARCHAR2(50),
    KETLUAN     NCLOB,
    CONSTRAINT PK_HSBA    PRIMARY KEY (MAHSBA),
    CONSTRAINT FK_HSBA_BN FOREIGN KEY (MABN) REFERENCES BENHNHAN(MABN),
    CONSTRAINT FK_HSBA_BS FOREIGN KEY (MABS) REFERENCES NHANVIEN(MANV)
);

-- =============================================================
-- 4. BANG HSBA_DV (Dich Vu Ho Tro Chan Doan)
-- =============================================================
CREATE TABLE HSBA_DV (
    MAHSBA      VARCHAR2(15)   NOT NULL,
    LOAIDV      NVARCHAR2(100) NOT NULL,
    NGAYDV      DATE           NOT NULL,
    MAKTV       VARCHAR2(10),
    KETQUA      NCLOB,
    CONSTRAINT PK_HSBA_DV    PRIMARY KEY (MAHSBA, LOAIDV, NGAYDV),
    CONSTRAINT FK_HSBADV_HS  FOREIGN KEY (MAHSBA) REFERENCES HSBA(MAHSBA),
    CONSTRAINT FK_HSBADV_KTV FOREIGN KEY (MAKTV)  REFERENCES NHANVIEN(MANV)
);

-- =============================================================
-- 5. BANG DONTHUOC
-- =============================================================
CREATE TABLE DONTHUOC (
    MAHSBA      VARCHAR2(15)   NOT NULL,
    NGAYDT      DATE           NOT NULL,
    TENTHUOC    NVARCHAR2(200) NOT NULL,
    LIEUDUNG    NVARCHAR2(300),
    CONSTRAINT PK_DONTHUOC   PRIMARY KEY (MAHSBA, NGAYDT, TENTHUOC),
    CONSTRAINT FK_DT_HSBA    FOREIGN KEY (MAHSBA) REFERENCES HSBA(MAHSBA)
);

-- =============================================================
-- 6. BANG THONGBAO (dung cho OLS - Yeu cau 2)
-- =============================================================
CREATE TABLE THONGBAO (
    MATHONGBAO  VARCHAR2(10)   NOT NULL,
    NOIDUNG     NCLOB,
    NGAYGIO     TIMESTAMP      DEFAULT SYSTIMESTAMP,
    DIADIEM     NVARCHAR2(200),
    CONSTRAINT PK_THONGBAO PRIMARY KEY (MATHONGBAO)
);

-- =============================================================
-- SEQUENCE de sinh ma tu dong
-- =============================================================
CREATE SEQUENCE SEQ_NHANVIEN START WITH 1  INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_BENHNHAN START WITH 1  INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_HSBA      START WITH 1  INCREMENT BY 1 NOCACHE;
CREATE SEQUENCE SEQ_THONGBAO  START WITH 1  INCREMENT BY 1 NOCACHE;

-- =============================================================
-- INDEX ho tro truy van thuong gap
-- =============================================================
CREATE INDEX IDX_HSBA_MABN  ON HSBA(MABN);
CREATE INDEX IDX_HSBA_MABS  ON HSBA(MABS);
CREATE INDEX IDX_HSBADV_KTV ON HSBA_DV(MAKTV);
CREATE INDEX IDX_DT_MAHSBA  ON DONTHUOC(MAHSBA);
CREATE INDEX IDX_NV_VAITRO  ON NHANVIEN(VAITRO);

-- =============================================================
-- COMMENT chu thich cac bang va cot chinh
-- =============================================================
COMMENT ON TABLE  NHANVIEN IS 'Danh sach nhan vien benh vien (DPV, BS, KTV)';
COMMENT ON TABLE  BENHNHAN IS 'Danh sach benh nhan';
COMMENT ON TABLE  HSBA     IS 'Ho so benh an';
COMMENT ON TABLE  HSBA_DV  IS 'Dich vu ho tro chan doan theo chi dinh bac si';
COMMENT ON TABLE  DONTHUOC IS 'Don thuoc chi dinh boi bac si / y si';
COMMENT ON TABLE  THONGBAO IS 'Thong bao cuoc hop khan (dung OLS)';

-- =============================================================
-- DU LIEU MAU
-- =============================================================

-- -----------------------------------------------
-- NHANVIEN: 20 DPV + 10 BS (sample) + 5 KTV + 5 BN-account
-- -----------------------------------------------
INSERT INTO NHANVIEN VALUES ('NV001','Nguyen Van An',N'Nam',DATE '1980-03-15','001080000001',N'Hà Nội','0912000001',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV002','Tran Thi Bich',N'Nữ',DATE '1985-07-22','001085000002',N'Hà Nội','0912000002',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV003','Le Van Cuong',N'Nam',DATE '1982-11-01','001082000003',N'TP.HCM','0912000003',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV004','Pham Thi Dung',N'Nữ',DATE '1990-05-30','001090000004',N'Hải Phòng','0912000004',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV005','Hoang Van Em',N'Nam',DATE '1978-09-12','001078000005',N'Đà Nẵng','0912000005',N'Điều phối viên',NULL);
-- (gia su co du 20 DPV; them 15 dong tuong tu o day)
INSERT INTO NHANVIEN VALUES ('NV006','Nguyen Thi Phuong',N'Nữ',DATE '1983-01-11','001083000006',N'Hà Nội','0912000006',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV007','Tran Van Quan',N'Nam',DATE '1979-04-28','001079000007',N'TP.HCM','0912000007',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV008','Le Thi Hoa',N'Nữ',DATE '1992-08-14','001092000008',N'Hà Nội','0912000008',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV009','Do Van Hung',N'Nam',DATE '1981-12-03','001081000009',N'TP.HCM','0912000009',N'Điều phối viên',NULL);
INSERT INTO NHANVIEN VALUES ('NV010','Vu Thi Lan',N'Nữ',DATE '1987-06-19','001087000010',N'Hải Phòng','0912000010',N'Điều phối viên',NULL);

-- Bac si / Y si (100 nhan vien, lay 10 mau)
INSERT INTO NHANVIEN VALUES ('BS001','GS.TS. Nguyen Van Minh',N'Nam',DATE '1965-02-20','001065000101',N'Hà Nội','0913000101',N'Bác sĩ/Y sĩ',N'Tiêu hóa');
INSERT INTO NHANVIEN VALUES ('BS002','ThS. Tran Thi Mai',N'Nữ',DATE '1975-08-15','001075000102',N'TP.HCM','0913000102',N'Bác sĩ/Y sĩ',N'Thần kinh');
INSERT INTO NHANVIEN VALUES ('BS003','TS. Le Van Nam',N'Nam',DATE '1970-04-10','001070000103',N'Hà Nội','0913000103',N'Bác sĩ/Y sĩ',N'Tim mạch');
INSERT INTO NHANVIEN VALUES ('BS004','BS. Pham Hoang Oanh',N'Nữ',DATE '1980-11-25','001080000104',N'TP.HCM','0913000104',N'Bác sĩ/Y sĩ',N'Tiêu hóa');
INSERT INTO NHANVIEN VALUES ('BS005','Y si Hoang Van Phuc',N'Nam',DATE '1985-03-07','001085000105',N'Hải Phòng','0913000105',N'Bác sĩ/Y sĩ',N'Tim mạch');
INSERT INTO NHANVIEN VALUES ('BS006','BS. Nguyen Thi Quynh',N'Nữ',DATE '1978-09-30','001078000106',N'Hà Nội','0913000106',N'Bác sĩ/Y sĩ',N'Thần kinh');
INSERT INTO NHANVIEN VALUES ('BS007','ThS. Tran Van Sang',N'Nam',DATE '1972-06-14','001072000107',N'TP.HCM','0913000107',N'Bác sĩ/Y sĩ',N'Tiêu hóa');
INSERT INTO NHANVIEN VALUES ('BS008','BS. Le Thi Thu',N'Nữ',DATE '1983-01-22','001083000108',N'Hà Nội','0913000108',N'Bác sĩ/Y sĩ',N'Tim mạch');
INSERT INTO NHANVIEN VALUES ('BS009','TS. Do Van Uy',N'Nam',DATE '1968-07-05','001068000109',N'Hải Phòng','0913000109',N'Bác sĩ/Y sĩ',N'Thần kinh');
INSERT INTO NHANVIEN VALUES ('BS010','BS. Vu Thi Van',N'Nữ',DATE '1990-05-18','001090000110',N'TP.HCM','0913000110',N'Bác sĩ/Y sĩ',N'Tiêu hóa');

-- Ky thuat vien (50 nhan vien, lay 5 mau)
INSERT INTO NHANVIEN VALUES ('KTV01','Nguyen Van Xa',N'Nam',DATE '1992-02-10','001092000201',N'Hà Nội','0914000201',N'Kỹ thuật viên',N'Xét nghiệm');
INSERT INTO NHANVIEN VALUES ('KTV02','Tran Thi Yen',N'Nữ',DATE '1995-07-23','001095000202',N'TP.HCM','0914000202',N'Kỹ thuật viên',N'Chẩn đoán hình ảnh');
INSERT INTO NHANVIEN VALUES ('KTV03','Le Van Zanh',N'Nam',DATE '1993-11-14','001093000203',N'Hà Nội','0914000203',N'Kỹ thuật viên',N'Xét nghiệm');
INSERT INTO NHANVIEN VALUES ('KTV04','Pham Thi Anh',N'Nữ',DATE '1991-04-05','001091000204',N'Hải Phòng','0914000204',N'Kỹ thuật viên',N'Chẩn đoán hình ảnh');
INSERT INTO NHANVIEN VALUES ('KTV05','Hoang Van Binh',N'Nam',DATE '1994-08-28','001094000205',N'TP.HCM','0914000205',N'Kỹ thuật viên',N'Xét nghiệm');

-- -----------------------------------------------
-- BENHNHAN: 15 mau (he thong co ~100,000)
-- -----------------------------------------------
INSERT INTO BENHNHAN VALUES ('BN000001',N'Nguyen Van Chinh',N'Nam',DATE '1975-03-12','079075000001','12',N'Lý Thường Kiệt',N'Quận 1',N'TP.HCM',N'Tiểu đường type 2',N'Cha mắc huyết áp',N'Penicillin');
INSERT INTO BENHNHAN VALUES ('BN000002',N'Tran Thi Dao',N'Nữ',DATE '1982-06-25','001082000002','55',N'Trần Hưng Đạo',N'Hoàn Kiếm',N'Hà Nội',NULL,NULL,NULL);
INSERT INTO BENHNHAN VALUES ('BN000003',N'Le Van Eo',N'Nam',DATE '1990-09-01','031090000003','78',N'Đinh Tiên Hoàng',N'Quận Bình Thạnh',N'TP.HCM',N'Viêm dạ dày mãn tính',N'Mẹ mắc ung thư đại tràng','Aspirin');
INSERT INTO BENHNHAN VALUES ('BN000004',N'Pham Thi Giang',N'Nữ',DATE '1968-12-15','046068000004','23',N'Hải Triều',N'Ngô Quyền',N'Hải Phòng',N'Tăng huyết áp',N'Ông nội tim mạch',NULL);
INSERT INTO BENHNHAN VALUES ('BN000005',N'Hoang Van Ha',N'Nam',DATE '2000-05-20','048000000005','5',N'Lê Lợi',N'Hải Châu',N'Đà Nẵng',NULL,NULL,NULL);
INSERT INTO BENHNHAN VALUES ('BN000006',N'Nguyen Thi Huong',N'Nữ',DATE '1959-07-30','079059000006','88',N'Võ Văn Tần',N'Quận 3',N'TP.HCM',N'Thần kinh ngoại biên',N'Bệnh tim bẩm sinh (con)',NULL);
INSERT INTO BENHNHAN VALUES ('BN000007',N'Tran Van Ich',N'Nam',DATE '1978-10-08','001078000007','14',N'Đội Cấn',N'Ba Đình',N'Hà Nội',NULL,NULL,N'Sulfa');
INSERT INTO BENHNHAN VALUES ('BN000008',N'Le Thi Kim',N'Nữ',DATE '1985-02-14','031085000008','67',N'Nguyễn Trãi',N'Quận 5',N'TP.HCM',N'Hội chứng ruột kích thích',NULL,NULL);
INSERT INTO BENHNHAN VALUES ('BN000009',N'Do Van Long',N'Nam',DATE '1995-04-03','046095000009','30',N'Tô Hiệu',N'Lê Chân',N'Hải Phòng',NULL,N'Đái tháo đường (mẹ)',NULL);
INSERT INTO BENHNHAN VALUES ('BN000010',N'Vu Thi Mai',N'Nữ',DATE '1970-11-20','001070000010','9',N'Phan Đình Phùng',N'Ba Đình',N'Hà Nội',N'Rối loạn lipid máu',NULL,NULL);
INSERT INTO BENHNHAN VALUES ('BN000011',N'Nguyen Van Nam',N'Nam',DATE '1953-08-17','079053000011','101',N'Đinh Bộ Lĩnh',N'Quận Bình Thạnh',N'TP.HCM',N'COPD',N'Ung thư phổi (cha)',NULL);
INSERT INTO BENHNHAN VALUES ('BN000012',N'Tran Thi Oanh',N'Nữ',DATE '1988-01-09','048088000012','22',N'Nguyễn Chí Thanh',N'Hải Châu',N'Đà Nẵng',NULL,NULL,N'Codeine');
INSERT INTO BENHNHAN VALUES ('BN000013',N'Le Van Phong',N'Nam',DATE '1992-07-15','031092000013','45',N'Cách Mạng Tháng 8',N'Quận 10',N'TP.HCM',N'Dị ứng thức ăn',NULL,N'Penicillin, Ibuprofen');
INSERT INTO BENHNHAN VALUES ('BN000014',N'Pham Thi Quynh',N'Nữ',DATE '1967-05-27','046067000014','18',N'Trần Phú',N'Hồng Bàng',N'Hải Phòng',N'Sỏi thận',N'Thận đa nang (cha)',NULL);
INSERT INTO BENHNHAN VALUES ('BN000015',N'Hoang Van Ru',N'Nam',DATE '2005-10-10','001005000015','3',N'Hoàng Diệu',N'Ba Đình',N'Hà Nội',NULL,NULL,NULL);

-- -----------------------------------------------
-- HSBA: 20 ho so benh an mau
-- -----------------------------------------------
INSERT INTO HSBA VALUES ('HSBA2024001','BN000001',DATE '2024-01-10',N'Tiểu đường type 2 kiểm soát kém',N'Điều chỉnh Metformin 1000mg 2 lần/ngày','BS001',N'Tiêu hóa',N'Cần tái khám sau 3 tháng');
INSERT INTO HSBA VALUES ('HSBA2024002','BN000002',DATE '2024-01-15',N'Đau đầu vận mạch, migraine',N'Nghỉ ngơi, dùng Sumatriptan 50mg khi cần','BS002',N'Thần kinh',N'Theo dõi tần suất cơn đau');
INSERT INTO HSBA VALUES ('HSBA2024003','BN000003',DATE '2024-01-20',N'Viêm dạ dày có HP (+)',N'Phác đồ bộ ba: Clarithromycin + Amoxicillin + PPI 14 ngày','BS004',N'Tiêu hóa',N'Nội soi kiểm tra sau 6 tuần');
INSERT INTO HSBA VALUES ('HSBA2024004','BN000004',DATE '2024-02-03',N'Tăng huyết áp grade 2',N'Amlodipine 10mg/ngày + hạn chế muối','BS003',N'Tim mạch',N'Đo huyết áp hàng ngày');
INSERT INTO HSBA VALUES ('HSBA2024005','BN000005',DATE '2024-02-10',N'Cảm cúm A (Influenza A)',N'Tamiflu 75mg x 2 lần/ngày x 5 ngày, nghỉ ngơi','BS006',N'Thần kinh',N'Khỏi sau 5 ngày điều trị');
INSERT INTO HSBA VALUES ('HSBA2024006','BN000006',DATE '2024-02-18',N'Bệnh thần kinh ngoại biên tiểu đường',N'Pregabalin 75mg x 2 lần/ngày + kiểm soát đường huyết','BS002',N'Thần kinh',N'Tái khám 1 tháng');
INSERT INTO HSBA VALUES ('HSBA2024007','BN000007',DATE '2024-03-01',N'Viêm đại tràng mãn tính, đợt bùng phát',N'Mesalazine 4g/ngày chia 2 lần + probiotic','BS007',N'Tiêu hóa',N'Nội soi đại tràng kế hoạch 3 tháng');
INSERT INTO HSBA VALUES ('HSBA2024008','BN000008',DATE '2024-03-05',N'Hội chứng ruột kích thích, thể tiêu chảy trội',N'Loperamide khi cần + chế độ ăn FODMAP thấp','BS001',N'Tiêu hóa',NULL);
INSERT INTO HSBA VALUES ('HSBA2024009','BN000009',DATE '2024-03-12',N'Suy tim độ II NYHA',N'Furosemide 40mg/ngày + Enalapril 5mg x 2/ngày','BS005',N'Tim mạch',N'Siêu âm tim tái đánh giá');
INSERT INTO HSBA VALUES ('HSBA2024010','BN000010',DATE '2024-03-20',N'Rối loạn lipid máu hỗn hợp',N'Rosuvastatin 20mg/tối + điều chỉnh chế độ ăn','BS003',N'Tim mạch',N'Xét nghiệm lipid máu sau 6 tuần');
INSERT INTO HSBA VALUES ('HSBA2024011','BN000011',DATE '2024-04-02',N'COPD đợt cấp mức độ trung bình',N'Salbutamol MDI + Ipratropium + kháng sinh Azithromycin 5 ngày','BS009',N'Thần kinh',N'Hướng dẫn kỹ thuật hít');
INSERT INTO HSBA VALUES ('HSBA2024012','BN000012',DATE '2024-04-10',N'Viêm khớp dạng thấp giai đoạn sớm',N'Methotrexate 10mg/tuần + Folic acid + hydroxychloroquine','BS008',N'Tim mạch',NULL);
INSERT INTO HSBA VALUES ('HSBA2024013','BN000013',DATE '2024-04-15',N'Dị ứng da tiếp xúc cấp tính',N'Loratadine 10mg/ngày + hydrocortisone cream 1%','BS004',N'Tiêu hóa',N'Tránh các tác nhân gây dị ứng');
INSERT INTO HSBA VALUES ('HSBA2024014','BN000014',DATE '2024-04-22',N'Sỏi thận 8mm niệu quản trái',N'Tamsulosin 0.4mg/tối, uống nhiều nước + ESWL nếu không tự ra','BS009',N'Thần kinh',NULL);
INSERT INTO HSBA VALUES ('HSBA2024015','BN000015',DATE '2024-05-03',N'Viêm phế quản cấp do vi khuẩn',N'Amoxicillin 500mg x 3 lần/ngày x 7 ngày + Bromhexine','BS006',N'Thần kinh',N'Khỏi sau 7 ngày');
INSERT INTO HSBA VALUES ('HSBA2024016','BN000001',DATE '2024-06-15',N'Hạ đường huyết triệu chứng',N'Bổ sung glucose, điều chỉnh liều Metformin','BS001',N'Tiêu hóa',N'Theo dõi chặt chẽ đường huyết');
INSERT INTO HSBA VALUES ('HSBA2024017','BN000003',DATE '2024-07-01',N'Ung thư dạ dày giai đoạn IB (sau sinh thiết)',N'Phẫu thuật cắt dạ dày bán phần + hóa trị bổ trợ','BS007',N'Tiêu hóa',N'Chuyển khoa phẫu thuật');
INSERT INTO HSBA VALUES ('HSBA2024018','BN000006',DATE '2024-08-10',N'Cơn đau thắt ngực không ổn định',N'Aspirin + Heparin + chụp mạch vành','BS005',N'Tim mạch',N'Can thiệp đặt stent động mạch vành');
INSERT INTO HSBA VALUES ('HSBA2024019','BN000009',DATE '2024-09-05',N'Rung nhĩ mới phát hiện',N'Warfarin 5mg/ngày + Digoxin + kiểm soát nhịp tim','BS003',N'Tim mạch',N'INR mục tiêu 2-3');
INSERT INTO HSBA VALUES ('HSBA2024020','BN000002',DATE '2024-10-12',N'Đột quỵ thiếu máu não (TIA)',N'Aspirin 100mg + Atorvastatin 40mg + phục hồi chức năng','BS002',N'Thần kinh',N'Theo dõi 24h, MRI não');

-- -----------------------------------------------
-- HSBA_DV: dich vu ho tro chan doan
-- -----------------------------------------------
INSERT INTO HSBA_DV VALUES ('HSBA2024001',N'Xét nghiệm HbA1c',DATE '2024-01-10','KTV01',N'HbA1c = 9.2% (cao)');
INSERT INTO HSBA_DV VALUES ('HSBA2024001',N'Xét nghiệm đường huyết đói',DATE '2024-01-10','KTV03',N'Glucose = 8.5 mmol/L');
INSERT INTO HSBA_DV VALUES ('HSBA2024002',N'Chụp CT đầu không cản quang',DATE '2024-01-15','KTV02',N'Không thấy tổn thương khu trú, phù hợp chẩn đoán migraine');
INSERT INTO HSBA_DV VALUES ('HSBA2024003',N'Nội soi dạ dày + sinh thiết',DATE '2024-01-20','KTV01',N'Viêm dạ dày HP(+), loét nhỏ thân vị');
INSERT INTO HSBA_DV VALUES ('HSBA2024003',N'Test urease nhanh HP',DATE '2024-01-20','KTV03',N'Dương tính');
INSERT INTO HSBA_DV VALUES ('HSBA2024004',N'ECG',DATE '2024-02-03','KTV04',N'Nhịp xoang, dày thất trái nhẹ');
INSERT INTO HSBA_DV VALUES ('HSBA2024004',N'Siêu âm tim',DATE '2024-02-03','KTV04',N'EF = 62%, dày thất trái nhẹ, không hở van');
INSERT INTO HSBA_DV VALUES ('HSBA2024009',N'Siêu âm tim',DATE '2024-03-12','KTV04',N'EF = 38%, giãn thất trái, hở hai lá độ I');
INSERT INTO HSBA_DV VALUES ('HSBA2024009',N'Xét nghiệm BNP',DATE '2024-03-12','KTV03',N'BNP = 560 pg/mL (tăng cao)');
INSERT INTO HSBA_DV VALUES ('HSBA2024010',N'Xét nghiệm bộ lipid máu',DATE '2024-03-20','KTV01',N'LDL = 4.8 mmol/L, TG = 3.2 mmol/L');
INSERT INTO HSBA_DV VALUES ('HSBA2024014',N'Siêu âm bụng',DATE '2024-04-22','KTV02',N'Sỏi niệu quản trái 8mm, không giãn đài bể thận');
INSERT INTO HSBA_DV VALUES ('HSBA2024014',N'Chụp X-quang KUB',DATE '2024-04-22','KTV05',N'Bóng cản quang niệu quản trái L3-L4');
INSERT INTO HSBA_DV VALUES ('HSBA2024017',N'Nội soi dạ dày + sinh thiết',DATE '2024-07-01','KTV01',N'Ung thư biểu mô tuyến giai đoạn sớm thân vị');
INSERT INTO HSBA_DV VALUES ('HSBA2024017',N'Chụp CT bụng có cản quang',DATE '2024-07-01','KTV02',N'U dạ dày 3cm, không di căn xa, hạch vùng 2 hạch');
INSERT INTO HSBA_DV VALUES ('HSBA2024018',N'ECG 12 chuyển đạo',DATE '2024-08-10','KTV04',N'ST chênh xuống V4-V6, sóng T âm');
INSERT INTO HSBA_DV VALUES ('HSBA2024018',N'Troponin I siêu nhạy',DATE '2024-08-10','KTV03',N'Troponin I = 0.08 ng/mL (ranh giới)');
INSERT INTO HSBA_DV VALUES ('HSBA2024020',N'MRI não diffusion',DATE '2024-10-12','KTV02',N'Nhồi máu cấp vùng bao trong trái 8x5mm');
INSERT INTO HSBA_DV VALUES ('HSBA2024020',N'Siêu âm doppler mạch cổ',DATE '2024-10-12','KTV05',N'Xơ vữa động mạch cảnh chung trái, hẹp 30%');

-- -----------------------------------------------
-- DONTHUOC: don thuoc mau
-- -----------------------------------------------
INSERT INTO DONTHUOC VALUES ('HSBA2024001',DATE '2024-01-10',N'Metformin 1000mg',N'1 viên x 2 lần/ngày, sau ăn sáng và tối');
INSERT INTO DONTHUOC VALUES ('HSBA2024001',DATE '2024-01-10',N'Atorvastatin 20mg',N'1 viên/tối trước khi ngủ');
INSERT INTO DONTHUOC VALUES ('HSBA2024002',DATE '2024-01-15',N'Sumatriptan 50mg',N'1 viên khi có cơn đau, tối đa 2 viên/ngày');
INSERT INTO DONTHUOC VALUES ('HSBA2024002',DATE '2024-01-15',N'Paracetamol 500mg',N'1-2 viên x 3-4 lần/ngày khi đau, không quá 8 viên/ngày');
INSERT INTO DONTHUOC VALUES ('HSBA2024003',DATE '2024-01-20',N'Clarithromycin 500mg',N'1 viên x 2 lần/ngày x 14 ngày');
INSERT INTO DONTHUOC VALUES ('HSBA2024003',DATE '2024-01-20',N'Amoxicillin 1000mg',N'1 viên x 2 lần/ngày x 14 ngày');
INSERT INTO DONTHUOC VALUES ('HSBA2024003',DATE '2024-01-20',N'Omeprazole 20mg',N'1 viên x 2 lần/ngày x 14 ngày, trước ăn 30 phút');
INSERT INTO DONTHUOC VALUES ('HSBA2024004',DATE '2024-02-03',N'Amlodipine 10mg',N'1 viên/sáng');
INSERT INTO DONTHUOC VALUES ('HSBA2024004',DATE '2024-02-03',N'Losartan 50mg',N'1 viên/sáng');
INSERT INTO DONTHUOC VALUES ('HSBA2024006',DATE '2024-02-18',N'Pregabalin 75mg',N'1 viên x 2 lần/ngày, sáng và tối');
INSERT INTO DONTHUOC VALUES ('HSBA2024007',DATE '2024-03-01',N'Mesalazine 1g',N'1 viên x 4 lần/ngày sau ăn');
INSERT INTO DONTHUOC VALUES ('HSBA2024007',DATE '2024-03-01',N'Probiotic Lactobacillus',N'1 gói/ngày, sau ăn');
INSERT INTO DONTHUOC VALUES ('HSBA2024009',DATE '2024-03-12',N'Furosemide 40mg',N'1 viên/sáng');
INSERT INTO DONTHUOC VALUES ('HSBA2024009',DATE '2024-03-12',N'Enalapril 5mg',N'1 viên x 2 lần/ngày');
INSERT INTO DONTHUOC VALUES ('HSBA2024009',DATE '2024-03-12',N'Spironolactone 25mg',N'1 viên/sáng');
INSERT INTO DONTHUOC VALUES ('HSBA2024010',DATE '2024-03-20',N'Rosuvastatin 20mg',N'1 viên/tối trước ngủ');
INSERT INTO DONTHUOC VALUES ('HSBA2024010',DATE '2024-03-20',N'Fenofibrate 160mg',N'1 viên/ngày trong bữa ăn');
INSERT INTO DONTHUOC VALUES ('HSBA2024012',DATE '2024-04-10',N'Methotrexate 2.5mg',N'4 viên 1 lần/tuần (vào ngày thứ Hai)');
INSERT INTO DONTHUOC VALUES ('HSBA2024012',DATE '2024-04-10',N'Folic acid 5mg',N'1 viên/ngày trừ ngày uống Methotrexate');
INSERT INTO DONTHUOC VALUES ('HSBA2024019',DATE '2024-09-05',N'Warfarin 5mg',N'1 viên/tối, điều chỉnh theo INR');
INSERT INTO DONTHUOC VALUES ('HSBA2024019',DATE '2024-09-05',N'Digoxin 0.25mg',N'1 viên/sáng, theo dõi mạch trước khi dùng');
INSERT INTO DONTHUOC VALUES ('HSBA2024020',DATE '2024-10-12',N'Aspirin 100mg',N'1 viên/ngày sau ăn sáng');
INSERT INTO DONTHUOC VALUES ('HSBA2024020',DATE '2024-10-12',N'Atorvastatin 40mg',N'1 viên/tối');
INSERT INTO DONTHUOC VALUES ('HSBA2024020',DATE '2024-10-12',N'Clopidogrel 75mg',N'1 viên/ngày sau ăn sáng');

-- -----------------------------------------------
-- THONGBAO: du lieu mau cho OLS (Yeu cau 2)
-- -----------------------------------------------
INSERT INTO THONGBAO VALUES ('TB001',N'Họp toàn bộ nhân viên về quy trình phòng cháy chữa cháy mới',TIMESTAMP '2024-03-01 08:00:00',N'Hội trường A - Cơ sở TP.HCM');
INSERT INTO THONGBAO VALUES ('TB002',N'Họp Ban Giám đốc: Kế hoạch mở rộng bệnh viện năm 2025',TIMESTAMP '2024-03-05 14:00:00',N'Phòng họp Ban Giám đốc - Cơ sở Hà Nội');
INSERT INTO THONGBAO VALUES ('TB003',N'Họp Lãnh đạo các khoa: Kế hoạch mua sắm thiết bị y tế quý 2',TIMESTAMP '2024-03-10 09:00:00',N'Phòng họp lớn - Cơ sở Hải Phòng');
INSERT INTO THONGBAO VALUES ('TB004',N'Họp khẩn Khoa Tiêu hóa: Xử lý ca ngộ độc thực phẩm hàng loạt',TIMESTAMP '2024-03-15 16:00:00',N'Phòng họp Khoa Tiêu hóa');
INSERT INTO THONGBAO VALUES ('TB005',N'Hội thảo nội bộ: Kỹ thuật nội soi mới dành cho KTV Tiêu hóa - CS HCM',TIMESTAMP '2024-03-20 07:30:00',N'Phòng đào tạo - CS TP.HCM');
INSERT INTO THONGBAO VALUES ('TB006',N'Cập nhật quy trình xét nghiệm COVID: KTV Tiêu hóa Hà Nội tham dự',TIMESTAMP '2024-03-22 10:00:00',N'Phòng họp Khoa Tiêu hóa - CS Hà Nội');
INSERT INTO THONGBAO VALUES ('TB007',N'Họp khẩn Lãnh đạo Khoa Tiêu hóa và Thần kinh tại Hải Phòng',TIMESTAMP '2024-03-25 15:00:00',N'Phòng họp liên khoa - CS Hải Phòng');

COMMIT;

PROMPT ===== 2. BO SUNG DU LIEU MAU CHO DU CASE DEMO =====

-- Bo sung du 20 Dieu phoi vien: NV001 -> NV020
DECLARE
    v_id VARCHAR2(10);
    v_cmnd VARCHAR2(12);
BEGIN
    FOR i IN 11..20 LOOP
        v_id := 'NV' || LPAD(i, 3, '0');
        v_cmnd := '001080' || LPAD(i, 6, '0');

        INSERT INTO NHANVIEN(MANV, HOTEN, PHAI, NGAYSINH, CMND, QUEQUAN, SODT, VAITRO, CHUYENKHOA)
        SELECT v_id,
               N'Dieu phoi vien ' || i,
               CASE WHEN MOD(i, 2) = 0 THEN N'Nữ' ELSE N'Nam' END,
               DATE '1985-01-01' + i,
               v_cmnd,
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Hà Nội'
                    WHEN 1 THEN N'TP.HCM'
                    ELSE N'Hải Phòng'
               END,
               '0912' || LPAD(i, 6, '0'),
               N'Điều phối viên',
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Tiêu hóa'
                    WHEN 1 THEN N'Thần kinh'
                    ELSE N'Tim mạch'
               END
        FROM dual
        WHERE NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = v_id);
    END LOOP;
END;
/

-- Bo sung du 100 Bac si/Y si: BS001 -> BS100
DECLARE
    v_id VARCHAR2(10);
    v_cmnd VARCHAR2(12);
BEGIN
    FOR i IN 11..100 LOOP
        v_id := 'BS' || LPAD(i, 3, '0');
        v_cmnd := '001070' || LPAD(i, 6, '0');

        INSERT INTO NHANVIEN(MANV, HOTEN, PHAI, NGAYSINH, CMND, QUEQUAN, SODT, VAITRO, CHUYENKHOA)
        SELECT v_id,
               N'Bac si/Y si ' || i,
               CASE WHEN MOD(i, 2) = 0 THEN N'Nữ' ELSE N'Nam' END,
               DATE '1975-01-01' + i,
               v_cmnd,
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Hà Nội'
                    WHEN 1 THEN N'TP.HCM'
                    ELSE N'Hải Phòng'
               END,
               '0913' || LPAD(i, 6, '0'),
               N'Bác sĩ/Y sĩ',
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Tiêu hóa'
                    WHEN 1 THEN N'Thần kinh'
                    ELSE N'Tim mạch'
               END
        FROM dual
        WHERE NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = v_id);
    END LOOP;
END;
/

-- Bo sung du 50 Ky thuat vien: KTV01 -> KTV50
DECLARE
    v_id VARCHAR2(10);
    v_cmnd VARCHAR2(12);
BEGIN
    FOR i IN 6..50 LOOP
        v_id := 'KTV' || LPAD(i, 2, '0');
        v_cmnd := '001090' || LPAD(i, 6, '0');

        INSERT INTO NHANVIEN(MANV, HOTEN, PHAI, NGAYSINH, CMND, QUEQUAN, SODT, VAITRO, CHUYENKHOA)
        SELECT v_id,
               N'Ky thuat vien ' || i,
               CASE WHEN MOD(i, 2) = 0 THEN N'Nữ' ELSE N'Nam' END,
               DATE '1990-01-01' + i,
               v_cmnd,
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Hà Nội'
                    WHEN 1 THEN N'TP.HCM'
                    ELSE N'Hải Phòng'
               END,
               '0914' || LPAD(i, 6, '0'),
               N'Kỹ thuật viên',
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Tiêu hóa'
                    WHEN 1 THEN N'Thần kinh'
                    ELSE N'Tim mạch'
               END
        FROM dual
        WHERE NOT EXISTS (SELECT 1 FROM NHANVIEN WHERE MANV = v_id);
    END LOOP;
END;
/

-- Bo sung benh nhan mau den BN000050.
-- De bai neu khoang 100000 benh nhan; khi demo khong nen tao 100000 Oracle user vi rat nang.
DECLARE
    v_id VARCHAR2(10);
    v_cccd VARCHAR2(12);
BEGIN
    FOR i IN 16..50 LOOP
        v_id := 'BN' || LPAD(i, 6, '0');
        v_cccd := '0790' || LPAD(i, 8, '0');

        INSERT INTO BENHNHAN(MABN, TENBN, PHAI, NGAYSINH, CCCD, SONHA, TENDUONG, QUANHUYEN, TINHTP,
                             TIENSUBENH, TIENSUBENHGD, DIUNGTHUOC)
        SELECT v_id,
               N'Benh nhan mau ' || i,
               CASE WHEN MOD(i, 2) = 0 THEN N'Nữ' ELSE N'Nam' END,
               DATE '1980-01-01' + i,
               v_cccd,
               TO_CHAR(i),
               N'Duong mau ' || i,
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Ba Đình'
                    WHEN 1 THEN N'Quận 1'
                    ELSE N'Lê Chân'
               END,
               CASE MOD(i, 3)
                    WHEN 0 THEN N'Hà Nội'
                    WHEN 1 THEN N'TP.HCM'
                    ELSE N'Hải Phòng'
               END,
               N'Chưa ghi nhận',
               N'Chưa ghi nhận',
               N'Không'
        FROM dual
        WHERE NOT EXISTS (SELECT 1 FROM BENHNHAN WHERE MABN = v_id);
    END LOOP;
END;
/

-- Bo sung mot so HSBA/HSBA_DV/DONTHUOC de moi nhom user co du case test
INSERT INTO HSBA(MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN)
SELECT 'HSBA2024021','BN000016',DATE '2024-11-01',N'Viêm dạ dày cấp',N'Nội soi và điều trị theo phác đồ','BS011',N'Tiêu hóa',N'Theo dõi tái khám sau 2 tuần'
FROM dual WHERE NOT EXISTS (SELECT 1 FROM HSBA WHERE MAHSBA = 'HSBA2024021');

INSERT INTO HSBA(MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN)
SELECT 'HSBA2024022','BN000017',DATE '2024-11-03',N'Rối loạn tiền đình',N'Thuốc cải thiện tuần hoàn não','BS012',N'Thần kinh',N'Tái khám nếu chóng mặt kéo dài'
FROM dual WHERE NOT EXISTS (SELECT 1 FROM HSBA WHERE MAHSBA = 'HSBA2024022');

INSERT INTO HSBA(MAHSBA, MABN, NGAY, CHANDOAN, DIEUTRI, MABS, MAKHOA, KETLUAN)
SELECT 'HSBA2024023','BN000018',DATE '2024-11-05',N'Tăng huyết áp',N'Theo dõi huyết áp và dùng thuốc','BS013',N'Tim mạch',N'Cần kiểm tra định kỳ'
FROM dual WHERE NOT EXISTS (SELECT 1 FROM HSBA WHERE MAHSBA = 'HSBA2024023');

INSERT INTO HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA)
SELECT 'HSBA2024021',N'Nội soi dạ dày',DATE '2024-11-01','KTV06',N'Viêm sung huyết hang vị'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM HSBA_DV WHERE MAHSBA='HSBA2024021' AND LOAIDV=N'Nội soi dạ dày' AND NGAYDV=DATE '2024-11-01'
);

INSERT INTO HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA)
SELECT 'HSBA2024022',N'Chụp CT não',DATE '2024-11-03','KTV07',N'Chưa phát hiện tổn thương khu trú'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM HSBA_DV WHERE MAHSBA='HSBA2024022' AND LOAIDV=N'Chụp CT não' AND NGAYDV=DATE '2024-11-03'
);

INSERT INTO HSBA_DV(MAHSBA, LOAIDV, NGAYDV, MAKTV, KETQUA)
SELECT 'HSBA2024023',N'ECG',DATE '2024-11-05','KTV08',N'Nhịp xoang'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM HSBA_DV WHERE MAHSBA='HSBA2024023' AND LOAIDV=N'ECG' AND NGAYDV=DATE '2024-11-05'
);

INSERT INTO DONTHUOC(MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG)
SELECT 'HSBA2024021',DATE '2024-11-01',N'Omeprazole 20mg',N'1 viên x 2 lần/ngày trước ăn'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM DONTHUOC WHERE MAHSBA='HSBA2024021' AND NGAYDT=DATE '2024-11-01' AND TENTHUOC=N'Omeprazole 20mg'
);

INSERT INTO DONTHUOC(MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG)
SELECT 'HSBA2024022',DATE '2024-11-03',N'Betahistine 16mg',N'1 viên x 3 lần/ngày'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM DONTHUOC WHERE MAHSBA='HSBA2024022' AND NGAYDT=DATE '2024-11-03' AND TENTHUOC=N'Betahistine 16mg'
);

INSERT INTO DONTHUOC(MAHSBA, NGAYDT, TENTHUOC, LIEUDUNG)
SELECT 'HSBA2024023',DATE '2024-11-05',N'Amlodipine 5mg',N'1 viên buổi sáng'
FROM dual WHERE NOT EXISTS (
    SELECT 1 FROM DONTHUOC WHERE MAHSBA='HSBA2024023' AND NGAYDT=DATE '2024-11-05' AND TENTHUOC=N'Amlodipine 5mg'
);

COMMIT;

PROMPT ===== 3. TAO VIEW ANH XA ORACLE USER VOI DU LIEU NGHIEP VU =====

CREATE OR REPLACE VIEW V_NGUOIDUNG_HE_THONG AS
SELECT
    MANV AS USERNAME,
    MANV AS MA_NGUOIDUNG,
    HOTEN AS HOTEN,
    VAITRO AS LOAI_NGUOIDUNG,
    CHUYENKHOA AS CHUYENKHOA,
    'NHANVIEN' AS NGUON_DU_LIEU
FROM NHANVIEN
UNION ALL
SELECT
    MABN AS USERNAME,
    MABN AS MA_NGUOIDUNG,
    TENBN AS HOTEN,
    N'Bệnh nhân' AS LOAI_NGUOIDUNG,
    CAST(NULL AS NVARCHAR2(100)) AS CHUYENKHOA,
    'BENHNHAN' AS NGUON_DU_LIEU
FROM BENHNHAN;

CREATE OR REPLACE VIEW V_MY_ACCOUNT AS
SELECT *
FROM V_NGUOIDUNG_HE_THONG
WHERE USERNAME = SYS_CONTEXT('USERENV', 'SESSION_USER');

PROMPT ===== 4. KIEM TRA DU LIEU SAU KHI HOAN THIEN =====

SELECT 'NHANVIEN' AS DOI_TUONG, COUNT(*) AS SO_LUONG FROM NHANVIEN
UNION ALL SELECT 'BENHNHAN', COUNT(*) FROM BENHNHAN
UNION ALL SELECT 'HSBA', COUNT(*) FROM HSBA
UNION ALL SELECT 'HSBA_DV', COUNT(*) FROM HSBA_DV
UNION ALL SELECT 'DONTHUOC', COUNT(*) FROM DONTHUOC
UNION ALL SELECT 'THONGBAO', COUNT(*) FROM THONGBAO;

SELECT VAITRO, COUNT(*) AS SO_LUONG
FROM NHANVIEN
GROUP BY VAITRO
ORDER BY VAITRO;

PROMPT ===== HOAN TAT FILE SCHEMA + DATA =====
