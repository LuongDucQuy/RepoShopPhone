CREATE DATABASE dbClickShop;
GO

USE dbClickShop;
GO

-- Tbl_Bill
CREATE TABLE [dbo].[Tbl_Bill] (
    [IDBill] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [user_id] INT NOT NULL,
    [ngayDat] DATETIME NOT NULL,
    [ngayCan] DATETIME NULL,
    [ngayGiao] DATETIME NULL,
    [HoTen] NVARCHAR(100) NULL,
    [diaChi] NVARCHAR(255) NOT NULL,
    [dienThoai] NVARCHAR(15) NULL,
    [cachThanhToan] NVARCHAR(50) NULL,
    [cachVanChuyen] NVARCHAR(50) NULL,
    [maTrangThai] INT NULL,
    [ghiChu] NVARCHAR(MAX) NULL,
    [soTien] DECIMAL(18, 0) NOT NULL,
    [ProductId] INT NULL
);
GO

-- Tbl_Cart
CREATE TABLE [dbo].[Tbl_Cart] (
    [CartId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [ProductId] INT NULL,
    [MemberId] INT NULL,
    [CartStatusId] INT NULL
);
GO

-- Tbl_CartStatus
CREATE TABLE [dbo].[Tbl_CartStatus] (
    [CartStatusId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [CartStatus] VARCHAR(500) NULL
);
GO

-- Tbl_Category
CREATE TABLE [dbo].[Tbl_Category] (
    [CategoryId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [CategoryName] VARCHAR(500) NULL,
    [IsActive] BIT NULL,
    [IsDelete] BIT NULL,
    [CategoryImage] VARCHAR(MAX) NULL
);
GO

-- Tbl_MemberRole
CREATE TABLE [dbo].[Tbl_MemberRole] (
    [MemberRoleId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [memberId] INT NULL,
    [RoleId] INT NULL
);
GO

-- Tbl_Product
CREATE TABLE [dbo].[Tbl_Product] (
    [ProductId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [ProductName] VARCHAR(500) NULL,
    [CategoryId] INT NULL,
    [IsActive] BIT NULL,
    [IsDelete] BIT NULL,
    [CreateDate] DATETIME NULL,
    [ModifiedDate] DATETIME NULL,
    [ProductImage] VARCHAR(MAX) NULL,
    [IsFeatured] BIT NULL,
    [Quantity] INT NULL,
    [Price] DECIMAL(18, 0) NULL
);
GO

-- Tbl_Roles
CREATE TABLE [dbo].[Tbl_Roles] (
    [RoleId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [RoleName] VARCHAR(200) NULL
);
GO

-- Tbl_ShippingDetails
CREATE TABLE [dbo].[Tbl_ShippingDetails] (
    [ShippingDetailId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [MemberId] INT NULL,
    [Adress] VARCHAR(500) NULL,
    [City] VARCHAR(500) NULL,
    [State] VARCHAR(500) NULL,
    [Country] VARCHAR(50) NULL,
    [ZipCode] VARCHAR(50) NULL,
    [OrderId] INT NULL,
    [AmountPaid] DECIMAL(18, 0) NULL,
    [PaymentType] VARCHAR(50) NULL
);
GO

-- Tbl_SlideImage
CREATE TABLE [dbo].[Tbl_SlideImage] (
    [SlideId] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [SlideTitle] VARCHAR(500) NULL,
    [SlideImage] VARCHAR(MAX) NULL
);
GO

-- Tbl_User
CREATE TABLE [dbo].[Tbl_User] (
    [user_id] INT IDENTITY(1, 1) NOT NULL PRIMARY KEY,
    [Name] NVARCHAR(50) NULL,
    [UserName] VARCHAR(200) NULL,
    [EmailId] VARCHAR(200) NULL,
    [Password] VARCHAR(200) NULL,
    [IsActive] BIT NULL,
    [IsDelete] BIT NULL,
    [CreatedOn] DATETIME NULL,
    [ModifiedOn] DATETIME NULL,
    [role] NVARCHAR(50) NULL,
    [Address] NVARCHAR(50) NULL,
    [PhoneNumber] NVARCHAR(50) NULL,
    [confirmEmail] BIT NOT NULL
);
GO

SET IDENTITY_INSERT [dbo].[Tbl_Bill] ON 

INSERT [dbo].[Tbl_Bill] ([IDBill], [user_id], [ngayDat], [ngayCan], [ngayGiao], [HoTen], [diaChi], [dienThoai], [cachThanhToan], [cachVanChuyen], [maTrangThai], [ghiChu], [soTien], [ProductId]) VALUES (55, 73, CAST(N'2024-12-04T22:53:45.160' AS DateTime), NULL, NULL, N'Lương Đức Quý', N'39/16C, 102, Lã Xuân Oai, Thủ Đức', N'0866345649', N'COD', N'Standard Shipping', 1, N'', CAST(45980000 AS Decimal(18, 0)), 40)
INSERT [dbo].[Tbl_Bill] ([IDBill], [user_id], [ngayDat], [ngayCan], [ngayGiao], [HoTen], [diaChi], [dienThoai], [cachThanhToan], [cachVanChuyen], [maTrangThai], [ghiChu], [soTien], [ProductId]) VALUES (56, 73, CAST(N'2024-12-04T22:54:10.890' AS DateTime), NULL, NULL, N'Lương Đức Quý', N'39/16C, 102, Lã Xuân Oai, Thủ Đức', N'0866345649', N'COD', N'Standard Shipping', 5, N'', CAST(22990000 AS Decimal(18, 0)), 40)
INSERT [dbo].[Tbl_Bill] ([IDBill], [user_id], [ngayDat], [ngayCan], [ngayGiao], [HoTen], [diaChi], [dienThoai], [cachThanhToan], [cachVanChuyen], [maTrangThai], [ghiChu], [soTien], [ProductId]) VALUES (57, 73, CAST(N'2024-12-04T22:55:34.707' AS DateTime), NULL, NULL, N'Lương Đức Quý', N'39/16C, 102, Lã Xuân Oai, Thủ Đức', N'0866345649', N'COD', N'Standard Shipping', 5, N'', CAST(22990000 AS Decimal(18, 0)), 40)
INSERT [dbo].[Tbl_Bill] ([IDBill], [user_id], [ngayDat], [ngayCan], [ngayGiao], [HoTen], [diaChi], [dienThoai], [cachThanhToan], [cachVanChuyen], [maTrangThai], [ghiChu], [soTien], [ProductId]) VALUES (63, 73, CAST(N'2024-12-04T22:59:46.390' AS DateTime), NULL, NULL, N'Lương Đức Quý', N'39/16C, 102, Lã Xuân Oai, Thủ Đức', N'0866345649', N'COD', N'Standard Shipping', 5, N'', CAST(25590000 AS Decimal(18, 0)), 45)

SET IDENTITY_INSERT [dbo].[Tbl_Bill] OFF
GO
SET IDENTITY_INSERT [dbo].[Tbl_Category] ON 

INSERT [dbo].[Tbl_Category] ([CategoryId], [CategoryName], [IsActive], [IsDelete], [CategoryImage]) VALUES (1, N'Apple', 1, 0, N'product-1.png')
INSERT [dbo].[Tbl_Category] ([CategoryId], [CategoryName], [IsActive], [IsDelete], [CategoryImage]) VALUES (2, N'Samsung', 1, 0, N'1051_s24_ultra__3.png')
INSERT [dbo].[Tbl_Category] ([CategoryId], [CategoryName], [IsActive], [IsDelete], [CategoryImage]) VALUES (3, N'Oppo', 1, 0, N'oppo.jpg')
INSERT [dbo].[Tbl_Category] ([CategoryId], [CategoryName], [IsActive], [IsDelete], [CategoryImage]) VALUES (4, N'Xiaomi', 1, 0, N'xiaomi_13_ultra_black.jpeg')
SET IDENTITY_INSERT [dbo].[Tbl_Category] OFF
GO
SET IDENTITY_INSERT [dbo].[Tbl_Product] ON 

INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (3, N'Samsung Galaxy S24 Ultra 12GB 256GB', 2, 1, NULL, NULL, CAST(N'2024-12-03T13:41:43.957' AS DateTime), N'proDetail-2.1.jpg', NULL, 11, CAST(29990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (4, N'OPPO Find N3 16GB 512GB', 3, 1, NULL, NULL, CAST(N'2024-11-27T22:49:24.353' AS DateTime), N'oppo-find-n3-flip-pink-7-750x500.jpg', NULL, 5, CAST(41990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (5, N'Xiaomi 14 Ultra 5G (16GB 512GB)', 4, 1, NULL, NULL, CAST(N'2024-11-27T22:51:06.970' AS DateTime), N'xiaomi-14-ultra-5-750x500.jpg', NULL, 5, CAST(29990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (6, N'iPhone 16 Pro Max 256GB | Chính hãng VN/A', 1, 1, NULL, NULL, CAST(N'2024-11-27T22:54:25.160' AS DateTime), N'proDetail-1.2.jpg', NULL, 109, CAST(31990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (7, N'OPPO Reno10 Pro+ 5G 12GB 256GB', 3, 1, NULL, NULL, CAST(N'2024-11-27T22:56:42.947' AS DateTime), N'oppo-find-n3-flip-pink-7-750x500.jpg', NULL, 5, CAST(14990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (37, N'iPhone 13 128GB | Chính hãng VN/A', 1, 1, NULL, NULL, CAST(N'2024-12-01T00:40:58.240' AS DateTime), N'iphone-13-4-1-750x500.jpg', NULL, 49, CAST(13590000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (39, N'iPhone 15 Pro Max 256GB | Chính hãng VN/A', 1, 1, NULL, NULL, CAST(N'2024-11-19T20:53:50.107' AS DateTime), N'mockup-free-SssxgUV6JRE-unsplash.jpg', NULL, 100, CAST(29490000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (40, N'iPhone 14 Pro 128GB | Chính hãng VN/A', 1, 1, NULL, NULL, CAST(N'2024-11-27T22:55:11.403' AS DateTime), N'pexels-tdcat-3571093.jpg', NULL, 78, CAST(22990000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (41, N'Samsung Galaxy S23', 2, 1, NULL, NULL, CAST(N'2024-11-19T20:46:58.127' AS DateTime), N'proDetail-2.2.jpg', NULL, 150, CAST(799990 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (42, N'Samsung Galaxy S23 Ultra', 2, 1, NULL, NULL, CAST(N'2024-12-03T13:38:08.647' AS DateTime), N'proDetail-2.3.jpg', NULL, 60, CAST(1199900 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (43, N'Oppo Find N2', 3, 1, NULL, NULL, CAST(N'2024-12-01T00:30:29.423' AS DateTime), N'oppo-reno12.jpg', NULL, 120, CAST(8999900 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (44, N'Oppo Reno 8', 3, 1, NULL, NULL, CAST(N'2024-11-30T16:28:37.440' AS DateTime), N'xiaomi-14-ultra-5-750x500.jpg', NULL, 90, CAST(599990 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (45, N'iPhone 14 Pro Max 128GB | Chính hãng VN/A', 1, 1, NULL, NULL, CAST(N'2024-11-30T16:31:04.260' AS DateTime), N'proDetail-1.4.jpg', NULL, 70, CAST(25590000 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (46, N'Samsung Galaxy Z Flip 5', 2, 1, NULL, NULL, CAST(N'2024-11-30T16:29:15.860' AS DateTime), N'proDetail-2.jpg', NULL, 50, CAST(999990 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (47, N'OPPO Reno12 5G 12GB/512GB', 3, 1, NULL, NULL, CAST(N'2024-12-01T00:29:13.867' AS DateTime), N'oppo-reno12.jpg', NULL, 150, CAST(299990 AS Decimal(18, 0)))
INSERT [dbo].[Tbl_Product] ([ProductId], [ProductName], [CategoryId], [IsActive], [IsDelete], [CreateDate], [ModifiedDate], [ProductImage], [IsFeatured], [Quantity], [Price]) VALUES (48, N'iPhone 13 256GB', 1, 1, NULL, NULL, CAST(N'2024-12-01T00:30:54.510' AS DateTime), N'iphone-13-4.jpg', NULL, 29, CAST(6999999 AS Decimal(18, 0)))
SET IDENTITY_INSERT [dbo].[Tbl_Product] OFF
GO
SET IDENTITY_INSERT [dbo].[Tbl_User] ON 

INSERT [dbo].[Tbl_User] ([user_id], [Name], [UserName], [EmailId], [Password], [IsActive], [IsDelete], [CreatedOn], [ModifiedOn], [role], [Address], [PhoneNumber], [confirmEmail]) VALUES (49, N'Duc Quy 123', N'abccd', N'abcd@gmail.com', N'15e2b0d3c33891ebb0f1ef609ec419420c20e320ce94c65fbc8c3312448eb225', 1, NULL, CAST(N'2024-12-03T13:30:08.890' AS DateTime), CAST(N'2024-11-10T01:06:32.010' AS DateTime), N'User', N'Thủ Đức', N'0866345649', 1)
INSERT [dbo].[Tbl_User] ([user_id], [Name], [UserName], [EmailId], [Password], [IsActive], [IsDelete], [CreatedOn], [ModifiedOn], [role], [Address], [PhoneNumber], [confirmEmail]) VALUES (62, N'Luong Duc Quy', N'Quy123', N'luongducquy@gmail.com', N'002546d90882b43da0f2173c276ed490c8340753c6e08492b2e3750f0ab213b6', NULL, NULL, CAST(N'2024-12-04T23:16:08.690' AS DateTime), NULL, N'Admin', NULL, NULL, 1)
INSERT [dbo].[Tbl_User] ([user_id], [Name], [UserName], [EmailId], [Password], [IsActive], [IsDelete], [CreatedOn], [ModifiedOn], [role], [Address], [PhoneNumber], [confirmEmail]) VALUES (73, N'Lương Đức Quý', N'Quy12345', N'luongducquy20041117@gmail.com', N'7e72df75dcc161e0bffb93ab7e4d6602e751c50f95cd37284e3baa46bfd531f1', 1, NULL, CAST(N'2024-12-01T11:35:44.190' AS DateTime), CAST(N'2024-12-04T23:13:21.913' AS DateTime), N'User', N'39/16C, 102, Lã Xuân Oai, Thủ Đức', N'0866345649', 1)
SET IDENTITY_INSERT [dbo].[Tbl_User] OFF

GO
CREATE PROCEDURE [dbo].[GetBySearch]
	@search nvarchar(max) = null
AS 
BEGIN
	SELECT *from [dbo].[Tbl_Product] p 
	left join [dbo].[Tbl_Category] C on p.CategoryId = c.CategoryId
	where 
	P.ProductName LIKE CASE WHEN @search is not null then '%'+@search+'%' else P.ProductName end
	or
	C.CategoryName LIKE CASE WHEN @search is not null then '%'+@search+'%' else C.CategoryName end
	END 
GO

CREATE PROCEDURE [dbo].[sp_InsertBill]
    @user_id INT,
    @ngayDat DATETIME,
    @HoTen NVARCHAR(100),
    @diaChi NVARCHAR(255),
    @dienThoai NVARCHAR(15),
    @cachThanhToan NVARCHAR(50),
    @soTien DECIMAL(18, 0),
    @ProductId INT
AS
BEGIN
    INSERT INTO [dbo].[Tbl_Bill] 
    ([user_id], [ngayDat], [HoTen], [diaChi], [dienThoai], [cachThanhToan], [soTien], [ProductId])
    VALUES 
    (@user_id, @ngayDat, @HoTen, @diaChi, @dienThoai, @cachThanhToan, @soTien, @ProductId);
END;
GO




