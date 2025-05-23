USE [CarServerDb]
GO
/****** Object:  Table [dbo].[Accessory]    Script Date: 5/19/2025 2:58:06 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Accessory](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[ImageUrl] [varchar](200) NULL,
 CONSTRAINT [PK_Accessory] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[AppUser]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[AppUser](
	[Id] [uniqueidentifier] NOT NULL,
	[IdRole] [int] NOT NULL,
	[UserName] [nvarchar](100) NOT NULL,
	[Email] [varchar](100) NOT NULL,
	[PasswordHash] [nvarchar](200) NOT NULL,
	[PasswordResetTokenExpiry] [datetime] NULL,
	[PasswordResetToken] [nvarchar](200) NULL,
 CONSTRAINT [PK_AppUser] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Car]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Car](
	[Id] [uniqueidentifier] NOT NULL,
	[Name] [nvarchar](50) NOT NULL,
	[Description] [nvarchar](500) NULL,
	[ImageUrl] [varchar](200) NULL,
 CONSTRAINT [PK_Car] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[CarAccessory]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[CarAccessory](
	[IdCar] [uniqueidentifier] NOT NULL,
	[IdAccessory] [int] NOT NULL,
	[Quantity] [int] NOT NULL,
 CONSTRAINT [PK_CarAccessory] PRIMARY KEY CLUSTERED 
(
	[IdCar] ASC,
	[IdAccessory] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Esp32Camera]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Esp32Camera](
	[Id] [uniqueidentifier] NOT NULL,
	[LastSeen] [datetime] NOT NULL,
	[IsOnline] [bit] NOT NULL,
 CONSTRAINT [PK__Esp32Cam__3214EC07BE06F7FA] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Esp32Control]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Esp32Control](
	[Id] [uniqueidentifier] NOT NULL,
	[LastSeen] [datetime] NOT NULL,
	[IsOnline] [bit] NOT NULL,
 CONSTRAINT [PK__Esp32Con__3214EC073B8DD0A8] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
/****** Object:  Table [dbo].[Role]    Script Date: 5/19/2025 2:58:07 PM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE TABLE [dbo].[Role](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[RoleName] [varchar](50) NOT NULL,
 CONSTRAINT [PK_Role] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX = OFF, STATISTICS_NORECOMPUTE = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS = ON, ALLOW_PAGE_LOCKS = ON, OPTIMIZE_FOR_SEQUENTIAL_KEY = OFF) ON [PRIMARY]
) ON [PRIMARY]
GO
SET IDENTITY_INSERT [dbo].[Accessory] ON 

INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (1, N'DHT11', N'/imgs/Accessories/1.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (2, N'DHT22', N'/imgs/Accessories/2.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (3, N'ESP32 30 Pin', N'/imgs/Accessories/3.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (4, N'ESP32 38 Pin', N'/imgs/Accessories/4.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (5, N'ESP32 CAM', N'/imgs/Accessories/5.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (6, N'L293D', N'/imgs/Accessories/6.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (7, N'L298N', N'/imgs/Accessories/7.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (8, N'Motor DC', N'/imgs/Accessories/8.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (9, N'Pin 18650', N'/imgs/Accessories/9.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (10, N'Bánh xe', N'/imgs/Accessories/10.jpg')
INSERT [dbo].[Accessory] ([Id], [Name], [ImageUrl]) VALUES (11, N'Led trắng', N'/imgs/Accessories/11.jpg')
SET IDENTITY_INSERT [dbo].[Accessory] OFF
GO
INSERT [dbo].[AppUser] ([Id], [IdRole], [UserName], [Email], [PasswordHash], [PasswordResetTokenExpiry], [PasswordResetToken]) VALUES (N'709a09eb-7f49-4f7a-b161-2b74e3f1123e', 1, N'Admin', N'haquanbg203@gmail.com', N'AQAAAAIAAYagAAAAEALxAD+o8YGX+llC6neV/t6TGCvSuRwiLKPP7bcyRgykxS0iZtqt7CyDmZhGdEy8qQ==', CAST(N'2025-05-19T05:58:18.427' AS DateTime), N'9c6425a9-6aea-43bb-9ac2-168c502ea24b')
INSERT [dbo].[AppUser] ([Id], [IdRole], [UserName], [Email], [PasswordHash], [PasswordResetTokenExpiry], [PasswordResetToken]) VALUES (N'2792c146-7489-409b-996c-4a7a2da64171', 2, N'Quan123', N'quan123@gmail.com', N'AQAAAAIAAYagAAAAEMui08Z5ZYW1Cscpjgu2RPZyI+1+igZ8CwmQtQe8dYk8ADyO7yfeiry8z9lY2UZnJQ==', NULL, NULL)
INSERT [dbo].[AppUser] ([Id], [IdRole], [UserName], [Email], [PasswordHash], [PasswordResetTokenExpiry], [PasswordResetToken]) VALUES (N'ec4a3db6-456a-4e1c-ab87-501b6cf8391b', 3, N'VanA', N'nvana@gmail.com', N'AQAAAAIAAYagAAAAEJMgzEi0qSuHp67Q+wuCjlpZf/2WvwatnY1UBuOx25RjMEYFsv6WkyeIEdnFBmCEyw==', NULL, NULL)
INSERT [dbo].[AppUser] ([Id], [IdRole], [UserName], [Email], [PasswordHash], [PasswordResetTokenExpiry], [PasswordResetToken]) VALUES (N'302ca2c6-0f75-45e7-8217-ce18ea47df55', 4, N'VanB', N'vanb@mail.com', N'AQAAAAIAAYagAAAAECzDBA5kni00zHvzQupg/yHfudswpUnG7VgvFd2yg+N8yz+k/W8cbJYnSopgISqqCg==', NULL, NULL)
GO
INSERT [dbo].[Car] ([Id], [Name], [Description], [ImageUrl]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', N'Thiết bị 01', N'aa', N'/imgs/Cars/6bc780d5-199a-4bb9-bb30-511a25c307de.jpg')
INSERT [dbo].[Car] ([Id], [Name], [Description], [ImageUrl]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', N'Thiết bị 02', NULL, N'/imgs/Cars/9d6c11d7-26dd-4903-b6a5-a50c23cc3883.jpg')
GO
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 1, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 3, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 5, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 7, 2)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 8, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 9, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 10, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', 11, 2)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 1, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 4, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 5, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 6, 1)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 8, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 9, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 10, 4)
INSERT [dbo].[CarAccessory] ([IdCar], [IdAccessory], [Quantity]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', 11, 2)
GO
INSERT [dbo].[Esp32Camera] ([Id], [LastSeen], [IsOnline]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', CAST(N'2025-05-06T09:01:17.040' AS DateTime), 0)
INSERT [dbo].[Esp32Camera] ([Id], [LastSeen], [IsOnline]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', CAST(N'2025-05-06T06:53:17.323' AS DateTime), 0)
GO
INSERT [dbo].[Esp32Control] ([Id], [LastSeen], [IsOnline]) VALUES (N'6bc780d5-199a-4bb9-bb30-511a25c307de', CAST(N'2025-05-06T09:01:17.327' AS DateTime), 0)
INSERT [dbo].[Esp32Control] ([Id], [LastSeen], [IsOnline]) VALUES (N'9d6c11d7-26dd-4903-b6a5-a50c23cc3883', CAST(N'2025-05-06T07:31:01.303' AS DateTime), 0)
GO
SET IDENTITY_INSERT [dbo].[Role] ON 

INSERT [dbo].[Role] ([Id], [RoleName]) VALUES (1, N'Admin')
INSERT [dbo].[Role] ([Id], [RoleName]) VALUES (2, N'Guest')
INSERT [dbo].[Role] ([Id], [RoleName]) VALUES (3, N'Operation')
INSERT [dbo].[Role] ([Id], [RoleName]) VALUES (4, N'Viewer')
SET IDENTITY_INSERT [dbo].[Role] OFF
GO
ALTER TABLE [dbo].[Esp32Camera] ADD  CONSTRAINT [DF__Esp32Camera__Id__3D5E1FD2]  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[Esp32Control] ADD  CONSTRAINT [DF__Esp32Control__Id__3A81B327]  DEFAULT (newsequentialid()) FOR [Id]
GO
ALTER TABLE [dbo].[AppUser]  WITH CHECK ADD  CONSTRAINT [FK_AppUser_Role] FOREIGN KEY([IdRole])
REFERENCES [dbo].[Role] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[AppUser] CHECK CONSTRAINT [FK_AppUser_Role]
GO
ALTER TABLE [dbo].[Car]  WITH CHECK ADD  CONSTRAINT [FK_Car_Esp32Camera] FOREIGN KEY([Id])
REFERENCES [dbo].[Esp32Camera] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Car] CHECK CONSTRAINT [FK_Car_Esp32Camera]
GO
ALTER TABLE [dbo].[Car]  WITH CHECK ADD  CONSTRAINT [FK_Car_Esp32Control] FOREIGN KEY([Id])
REFERENCES [dbo].[Esp32Control] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[Car] CHECK CONSTRAINT [FK_Car_Esp32Control]
GO
ALTER TABLE [dbo].[CarAccessory]  WITH CHECK ADD  CONSTRAINT [FK_CarAccessory_Accessory] FOREIGN KEY([IdAccessory])
REFERENCES [dbo].[Accessory] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarAccessory] CHECK CONSTRAINT [FK_CarAccessory_Accessory]
GO
ALTER TABLE [dbo].[CarAccessory]  WITH CHECK ADD  CONSTRAINT [FK_CarAccessory_Car] FOREIGN KEY([IdCar])
REFERENCES [dbo].[Car] ([Id])
ON DELETE CASCADE
GO
ALTER TABLE [dbo].[CarAccessory] CHECK CONSTRAINT [FK_CarAccessory_Car]
GO
