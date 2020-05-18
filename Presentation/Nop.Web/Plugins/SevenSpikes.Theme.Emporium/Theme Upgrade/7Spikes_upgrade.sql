/* Upgrade script for Store Locator plugin */

IF(EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[SS_SL_Shop]') AND type in (N'U')) AND
   EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[SS_SL_Shop]') and NAME='IsVisible') AND
   EXISTS (SELECT 1 FROM sys.columns WHERE object_id=object_id('[SS_SL_Shop]') and NAME='DisplayOrder'))

BEGIN

	UPDATE [SS_SL_Shop]
	SET [IsVisible] = 0 
	WHERE [IsVisible] IS NULL 
		
	UPDATE [SS_SL_Shop]
	SET [DisplayOrder] = 0
	WHERE [DisplayOrder] IS NULL


	ALTER TABLE [SS_SL_Shop] ALTER COLUMN [IsVisible] bit NOT NULL

	ALTER TABLE [SS_SL_Shop] ALTER COLUMN [DisplayOrder] INT NOT NULL
		
END	