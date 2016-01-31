Welcome to the TFC.tv Solution Github Page!
===========================

*View the [site](http://tfc.tv).*

Team Members
------------------
* Eugene Paden @eugenecp
* Albin Lim @istarbuxs
* Nads Berces @nadsberces

Release Notes
---------------------------
***June 08, 2015
1. Added Events table
CREATE TABLE [dbo].[OnlineEvents](
	[EventId] [int] IDENTITY(1,1) NOT NULL,
	[Code] [varchar](50) NOT NULL,
	[Name] [varchar](300) NULL,
	[Description] [varchar](500) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[StatusId] [int] NOT NULL,
	[Timezone] [nvarchar](255) NULL,
 CONSTRAINT [PK_Event] PRIMARY KEY CLUSTERED 
(
	[EventId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) 
GO

CREATE INDEX IX_OnlineEvents_1_StatusId
	ON dbo.OnlineEvents (StatusId);
GO

CREATE INDEX IX_OnlineEvents_1_Code
	ON dbo.OnlineEvents (Code);
GO
***May 5, 2015
1. Added CategoryRestrictions table
CREATE TABLE [dbo].[CategoryRestrictions](
	[CategoryRestrictionId] [int] IDENTITY(1,1) NOT NULL,
	[CategoryId] [int] NOT NULL,
	[Country] [nvarchar](2) NOT NULL,
	[Region] [nvarchar](50) NOT NULL,
	[City] [nvarchar](50) NOT NULL,
	[ZipCode] [nvarchar](50) NOT NULL,
	[Allowed] [bit] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedOn] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_CategoryRestrictions_1] PRIMARY KEY CLUSTERED 
(
	[CategoryRestrictionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) 
GO

2. Added CategoryRestrictions index
CREATE NONCLUSTERED INDEX [IX_CategoryRestrictions] ON [dbo].[CategoryRestrictions] 
(
	[CategoryId] ASC,
	[ZipCode] ASC,
	[City] ASC,
	[Region] ASC,
	[Country] ASC,
	[Allowed] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, SORT_IN_TEMPDB = OFF, IGNORE_DUP_KEY = OFF, DROP_EXISTING = OFF, ONLINE = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
GO

3. Added StoreProcedure CheckCategoryIfGeoAllowed

/****** Object:  StoredProcedure [dbo].[CheckCategoryIfGeoAllowed]    Script Date: 05/05/2015 14:47:39 ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
CREATE PROCEDURE [dbo].[CheckCategoryIfGeoAllowed]
@categoryId int, 
@country nvarchar(2),
@region nvarchar(50),
@city nvarchar(50),
@zipCode nvarchar(50)
AS
BEGIN
	

	declare @ok bit

	SET @ok = NULL

	 SELECT @zipCode = ISNULL(ZipCode, '--'), 
			@ok = Allowed
	   FROM [CategoryRestrictions] 
	  WHERE CategoryId = @categoryId
			AND ZipCode = @zipCode
			AND City = @city
			AND Region = @region
			AND Country = @country


	IF @ok IS NULL
	BEGIN
		SET @zipCode = '--'
		--check city
		 SELECT @city = ISNULL(City, '--'), 
				@ok = Allowed
		   FROM [CategoryRestrictions] 
		  WHERE CategoryId = @categoryId 
				AND ZipCode = @zipCode
				AND City = @city
				AND Region = @region
				AND Country = @country
		

		IF @ok IS NULL
		BEGIN
			SET @city = '--'
			--check region
				SELECT @region = ISNULL(Region, '--'), 
					@ok = Allowed
				FROM [CategoryRestrictions] 
				WHERE CategoryId = @categoryId 
					AND ZipCode = @zipCode
					AND City = @city
					AND Region = @region
					AND Country = @country

				
				IF @ok IS NULL
				BEGIN
					SET @region = '--'
					--check country
						SELECT @country = ISNULL(Country, '--'), 
							@ok = Allowed
						FROM [CategoryRestrictions] 
						WHERE CategoryId = @categoryId 
							AND ZipCode = @zipCode
							AND City = @city
							AND Region = @region
							AND Country = @country

						

						IF @ok IS NULL
						BEGIN
							SET @country = '--'
							--check all
								SELECT @ok = ISNULL(Allowed,1)
								FROM [CategoryRestrictions] 
								WHERE CategoryId = @categoryId 
									AND ZipCode = @zipCode
									AND City = @city
									AND Region = @region
									AND Country = @country

		
						END
				END
		END

	END

	SELECT ISNULL(@ok,1)
END
***April 14, 2015
1. Added PacMayLogs table

CREATE TABLE [dbo].[PacMayLogs](
	[Id] [int] IDENTITY(1,1) NOT NULL,
	[UserId] [uniqueidentifier] NULL,
	[PurchaseId] [int] NULL,
	[registDt] [datetime] NULL,
 CONSTRAINT [PK_PacMayLogs] PRIMARY KEY CLUSTERED 
(
	[Id] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
GO

CREATE INDEX IX_PacMayLogs_PurchaseId
	ON dbo.PacMayLogs (PurchaseId);
GO	

***February 09, 2015
1. Added ITEDetails table

CREATE TABLE [dbo].[ITEDetails](
	[UserId] [uniqueidentifier] NOT NULL,
	[ITEId] [nvarchar](50) NOT NULL,
 CONSTRAINT [PK_ITEDetails] PRIMARY KEY CLUSTERED 
(
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
GO

***March 02, 2015
1. Added new columns on ProgramSchedules

ALTER TABLE [dbo].[ProgramSchedules]
		ADD
		[CategoryId] [int] NULL,
		[Blurb] nvarchar(MAX) NULL


***November 30, 2014
1. Added new columns on CountryBitrates

ALTER TABLE [dbo].[CountryBitrates]
		ADD
		[ProgressiveHDBitrate] [int] NULL,
		[ProgressiveHighBitrate][int] NULL,
		[ProgressiveLowBitrate] [int] NULL

***November 25, 2014
1. Added new column on Products

ALTER TABLE [dbo].Products
ADD
[RegularProductId] [int] NULL

***November 18, 2014
1. Added new table (CountryBitrate)

CREATE TABLE [dbo].[CountryBitrates](
	[CountryBitrateId] [int] IDENTITY(1,1) NOT NULL,
	[CountryCode] [nchar](2) NULL,
	[LowerLimit] [int] NULL,
	[UpperLimit] [int] NULL,
 CONSTRAINT [PK_CountryBitrate] PRIMARY KEY CLUSTERED 
(
	[CountryBitrateId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)
GO

CREATE INDEX IX_CountryBitrates_CountryCode
	ON dbo.CountryBitrates (CountryCode);
GO	

ALTER TABLE [dbo].[CountryBitrates]
		ADD
		[Platform] [int] NULL DEFAULT((0))
GO

CREATE INDEX IX_CountryBitrates_CountryCode_Platform
	ON dbo.CountryBitrates (CountryCode,Platform);
GO			


***October 22, 2014
1. Added column on Products table

ALTER TABLE [dbo].Products
ADD
[IsRecurring] [int] NULL
	
GO		
ALTER TABLE [dbo].[Products]
ADD  DEFAULT ((0)) FOR [IsRecurring]
GO


***October 21, 2014
1. Added new table (Search Autocomplete)
CREATE TABLE [dbo].[SearchResults](
	[Sid] [int] IDENTITY(1,1) NOT NULL,
	[Input] [nvarchar](300) NULL,
	[Link] [nvarchar](max) NULL,
	[StatusId] [tinyint] NOT NULL,
	[CreatedOn] [datetime] NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedOn] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_SearchResults] PRIMARY KEY CLUSTERED 
(
	[Sid] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
) 

GO

ALTER TABLE [dbo].[SearchResults]
 ADD  DEFAULT ((0)) FOR [StatusId]
GO

CREATE INDEX IX_SearchResults_1_Input
	ON dbo.SearchResults (Input);
GO	

CREATE INDEX IX_SearchResults_1_StatusId
	ON dbo.SearchResults (StatusId);
GO	

***May 27, 2014
1. Added new columns (Social Network Images)
ALTER TABLE [dbo].Categories
		ADD
		[ImageSocialNetwork] [nvarchar](300) NULL
		
ALTER TABLE [dbo].Episodes
		ADD
		[ImageSocialNetwork] [nvarchar](300) NULL
		
---------------------------		
***July 11, 2014

alter table [Categories]
add ImageSocialNetwork nvarchar(300) null

alter table [Episodes]
add ImageSocialNetwork nvarchar(300) null

alter table [ProductPrices]
add
DiscountAmount money not null default 0,
DiscountPercentage decimal(10,5) not null default 0,
DiscountCopy nvarchar(500) null

alter table [Users]
add RegistrationCookie nvarchar(max) null

update a
set a.DiscountAmount=(b.amount*3 - a.amount),
a.discountPercentage = (b.amount*3 - a.amount)/(b.amount*3)
from productPrices a, productPrices b
where a.productId = 3
and b.productId=1
and a.currencyCode=b.CurrencyCode

-- 12 months
update a
set a.DiscountAmount=(b.amount*12 - a.amount),
a.discountPercentage = (b.amount*12 - a.amount)/(b.amount*12)
from productPrices a, productPrices b
where a.productId = 4
and b.productId=1
and a.currencyCode=b.CurrencyCode



***April 21 2014
1. created new table MopayTransactionRequests
CREATE TABLE [dbo].[MopayTransactionRequests](
	[GUID] [nvarchar](32) NOT NULL,
	[ReferenceId] [nvarchar](32) NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[Currency] [nvarchar](3) NOT NULL,
	[Amount] [money] NOT NULL,
	[DateCreated] [datetime] NOT NULL,
	[UpdatedOn] [datetime] NULL,
	[NumberOfAttempts] [int] NOT NULL,
	[ErrorCode] [nvarchar](20) NULL,
	[ErrorMessage] [text] NULL,
	[StatusId] [nchar](10) NULL,
 CONSTRAINT [PK_MopayTransactionRequests] PRIMARY KEY CLUSTERED 
(
	[GUID] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) ON [PRIMARY]
) ON [PRIMARY] 

GO

ALTER TABLE [dbo].[MopayTransactionRequests] ADD  DEFAULT (getdate()) FOR [DateCreated]
GO
---------------------------
***March 27, 2014
1. created new table MopayButtons
CREATE TABLE [dbo].[MopayButtons](
	[ButtonId] [nvarchar](32) NOT NULL,
	[CountryCode] [nvarchar](2) NOT NULL,
	[Amount] [money] NOT NULL,
	[Currency] [nvarchar](3) NOT NULL,
	[StatusId] [int] NOT NULL,
 CONSTRAINT [PK_MopayButtonId] PRIMARY KEY CLUSTERED 
(
	[ButtonId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
)

---------------------------
***December 20, 2013
1. added new column on Transactions table

	ALTER TABLE [dbo].[Transactions]
		ADD
		[NumberOfAttempts] [int] NULL


2. added new columns on RecurringBillings table
	
	ALTER TABLE [dbo].[RecurringBillings]
		ADD
		[PaymentTypeId] [int] NULL
		
	ALTER TABLE [dbo].[RecurringBillings]
		ADD
		[SubscriberId] [varchar](50) NULL	
		
***October 30, 2013
1. created new tables Promo & UserPromos

CREATE TABLE [dbo].[Promos](
	[PromoId] [int] IDENTITY(1,1) NOT NULL,
	[Name] [varchar](50) NULL,
	[Description] [varchar](500) NULL,
	[StartDate] [datetime] NULL,
	[EndDate] [datetime] NULL,
	[StatusId] [int] NOT NULL,
 CONSTRAINT [PK_Promo] PRIMARY KEY CLUSTERED 
(
	[PromoId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) 
GO

CREATE INDEX IX_Promos_1_StatusId
	ON dbo.Promos (StatusId);
GO	

CREATE TABLE [dbo].[UserPromos](
	[PromoId] [int] NOT NULL,
	[UserId] [uniqueidentifier] NOT NULL,
	[CreatedOn] [datetime] NOT NULL,
	[CreatedBy] [uniqueidentifier] NULL,
	[UpdatedOn] [datetime] NULL,
	[UpdatedBy] [uniqueidentifier] NULL,
 CONSTRAINT [PK_UserPromos_1] PRIMARY KEY CLUSTERED 
(
	[PromoId] ASC,
	[UserId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) 
GO

---------------------------
***September 27, 2013
1. added new column on ProductGroup table

	ALTER TABLE [dbo].[ProductGroup]
		ADD
		[Blurb] [nvarchar](max) NULL
	GO


---------------------------
***August 30, 2013
1. added new column on ProductGroup table

	ALTER TABLE [dbo].[ProductGroup]
		ADD
		[DefaultProductId] [int] NULL
	GO

---------------------------
***August 22, 2013
1. added new column on Users table

	ALTER TABLE [dbo].[Users]
		ADD
		[SessionId] [varchar](max) NULL
	GO
	ALTER TABLE [dbo].[Users]
		ADD
		[SessionLoggedDate] [datetime] NULL
	GO	
---------------------------
***August 15, 2013
1. Added new table PaypalIPNLog

CREATE TABLE [dbo].[PaypalIPNLog](
	[TransactionId] [int] IDENTITY(1,1) NOT NULL,
	[UniqueTransactionId] [varchar](50) NULL,
	[TransactionDate] [datetime] NULL,
	[UserId] [uniqueidentifier] NULL,
 CONSTRAINT [PK_PaypalIPNLog] PRIMARY KEY CLUSTERED 
(
	[TransactionId] ASC
)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON) 
) 
GO
CREATE INDEX IX_PaypalIPNLog_UTransId
		ON dbo.PaypalIPNLog (UniqueTransactionId);
GO
CREATE INDEX IX_PaypalIPNLog_UserId
ON dbo.PaypalIPNLog (UserId);
GO

---------------------------
***July 17, 2013
1. added new column on ProductGroup table

	ALTER TABLE [dbo].[ProductGroup]
		ADD
		[ProductSubscriptionTypeId] [int] NULL
1. added new column on Products table

	ALTER TABLE [dbo].[Products]
		ADD
		[BreakingDate] [datetime] NULL		
---------------------------
***April 25, 2013
1. added new column on RecurringBillings table

	ALTER TABLE [dbo].[RecurringBillings]
		ADD
		[NumberOfAttempts] [int] NULL

***April 24, 2013
1. added new column on RecurringBillings table

	ALTER TABLE [dbo].[RecurringBillings]
		ADD
		[GomsRemarks] [nvarchar](max) NULL
		
***April 19, 2013

1. added new column on RecurringBillings table

	ALTER TABLE [dbo].[RecurringBillings]
		ADD 
		[LastSentEmail] [datetime] NULL

***April 08, 2013

1. created scheduled task for recurring billing
2. added function for recurring payment
3. added function for updating of expiry date in recurring billing table
4. added new columns on CreditCards table

	ALTER TABLE [dbo].[CreditCards]
		ADD
		[CardType] [nvarchar](50) NULL
	
	ALTER TABLE [dbo].[CreditCards]
		ADD
		[PaymentMethodId] [int] NOT NULL
		
***April 05, 2013***

1. created CreditCards table

	CREATE TABLE [dbo].[CreditCards](
			[CCId] [int] IDENTITY(1,1) NOT NULL,
			[UserId] [uniqueidentifier] NOT NULL,
			[CreditCardHash] [nvarchar](50) NULL,
			[LastDigits] [nvarchar](4) NOT NULL,
			[StatusId] [int] NOT NULL,
			[CreatedOn] [datetime] NULL,
			[UpdatedOn] [datetime] NULL,
			[OfferingId] [int] NOT NULL,
		 CONSTRAINT [PK_CreditCards] PRIMARY KEY CLUSTERED 
		(
			[CCId] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
		)
	GO
	CREATE INDEX IX_CreditCards_OfferingId
		ON dbo.CreditCards (OfferingId);
	GO	
	CREATE INDEX IX_CreditCards_StatusId
		ON dbo.CreditCards (StatusId);
	GO
	CREATE INDEX IX_CreditCards_UserId
		ON dbo.CreditCards (UserId);
	GO

***April 04, 2013***

1. added RecurringBillings table (script found below)
2. installed SubstituteApp.Robots for robots.txt transformation
3. fixed sitemap.xml
4. cached sitemap for easier retrieval
5. fixed paypal transactions on CreateOrder function on GomsTFCtv
6. made use of the new function RegisterUser2 for post-processing of user registration

    CREATE TABLE [dbo].[RecurringBillings](
			[RecurringBillingId] [int] IDENTITY(1,1) NOT NULL,
			[UserId] [uniqueidentifier] NOT NULL,
			[ProductId] [int] NOT NULL,
			[EndDate] [datetime] NULL,
			[StatusId] [int] NOT NULL,
			[CreatedOn] [datetime] NULL,
			[UpdatedOn] [datetime] NULL,
			[NextRun] [datetime] NULL,
			[OfferingId] [int] NOT NULL,
			[PackageId] [int] NOT NULL,
			[CreditCardHash] [nvarchar](50) NULL,
		 CONSTRAINT [PK_RecurringBillings] PRIMARY KEY CLUSTERED 
		(
			[RecurringBillingId] ASC
		)WITH (PAD_INDEX  = OFF, STATISTICS_NORECOMPUTE  = OFF, IGNORE_DUP_KEY = OFF, ALLOW_ROW_LOCKS  = ON, ALLOW_PAGE_LOCKS  = ON)
		)
	GO
	CREATE INDEX IX_RecurringBillings_OfferingId
		ON dbo.RecurringBillings (OfferingId);
	GO
	CREATE INDEX IX_RecurringBillings_ProductId
		ON dbo.RecurringBillings (ProductId);
	GO
	CREATE INDEX IX_RecurringBillings_PackageId
		ON dbo.RecurringBillings (PackageId);
	GO
	CREATE INDEX IX_RecurringBillings_UserId
		ON dbo.RecurringBillings (UserId);
	GO

***March 25, 2013***

1. installed ELMAH package
2. adjusted ELMAH code to work with Azure Tables 2.0
3. secured elmah.axd
4. created function to log exceptions via ELMAH

***March 22, 2013***

1. created Halalan 2013 pages. check it [here](http://tfc.tv/Halalan2013)
2. added Facebook tracker on Registration complete page
3. fixed ordering of episodes on show & episode page

***March 12, 2013***

1. @nadsberces able to pull solution to local
2. set **Copy Local** to **true** for Microsoft.WindowsAzure.ServiceRuntime.dll on TFCtv GOMS Sync project

***March 08, 2013***

1. modified the README.md
2. included all the TFC.tv files from the solution
3. restored corrupted repository
4. synced to master
