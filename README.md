# ExpenseTracker
This is an Expense Tracker system that can record daily expenditure in
Step - 1 : Connect your ServerName &  DBName
Step - 2 : Update-Database
Step - 3 : Run the Store Procedure [ 
USE [ExpenseDB]
GO
/****** Object:  StoredProcedure [dbo].[SP_FilteringExpenseData]    Script Date: 2021-12-11 12:58:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--		SP_FilteringExpenseData '5','0','01122021','30122021'

CREATE PROC [dbo].[SP_FilteringExpenseData]
(
@PageSize int =15,
@PageNo int=0,
@FromDate VARCHAR(10),
@ToDate VARCHAR(10)
)
AS
BEGIN
		DECLARE @TOTALROW INT
		SET @TOTALROW = ( SELECT COUNT(*) FROM DailyExpenses DE INNER JOIN ExpenseCategories EC 
						  ON EC.Id = DE.ExpenseCategoryId WHERE DE.IsDeleted = 0 AND EC.IsDeleted = 0 
						  AND FORMAT(DE.ExpenseDate,'ddMMyyyy') BETWEEN @FromDate AND @ToDate )

		SELECT DE.Id,DE.ExpenseCategoryId,EC.CategoryName,DE.ExpenseAmount,
		FORMAT(DE.ExpenseDate,'yyyy-MM-dd')ExpenseDate,
		@TOTALROW TotalRow		
		FROM DailyExpenses  DE
		INNER JOIN ExpenseCategories EC ON EC.Id = DE.ExpenseCategoryId
		WHERE DE.IsDeleted = 0 AND EC.IsDeleted = 0
		AND FORMAT(DE.ExpenseDate,'ddMMyyyy') BETWEEN @FromDate AND @ToDate
		ORDER BY DE.ExpenseDate ASC
		OFFSET @PageSize*@PageNo ROWS
		FETCH NEXT @PageSize ROWS ONLY
END

GO
/****** Object:  StoredProcedure [dbo].[SP_Get_DailyExpense_Data_List]    Script Date: 2021-12-11 12:58:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--		SP_Get_DailyExpense_Data_List '20','0'

CREATE PROC [dbo].[SP_Get_DailyExpense_Data_List]
(
@PageSize int =15,
@PageNo int=0
)
AS
BEGIN
		DECLARE @TOTALROW INT
		SET @TOTALROW = ( SELECT COUNT(*) FROM DailyExpenses DE INNER JOIN ExpenseCategories EC 
						  ON EC.Id = DE.ExpenseCategoryId WHERE DE.IsDeleted = 0 AND EC.IsDeleted = 0 )

		SELECT DE.Id,DE.ExpenseCategoryId,EC.CategoryName,DE.ExpenseAmount,
		FORMAT(DE.ExpenseDate,'yyyy-MM-dd')ExpenseDate,
		@TOTALROW TotalRow		
		FROM DailyExpenses  DE
		INNER JOIN ExpenseCategories EC ON EC.Id = DE.ExpenseCategoryId
		WHERE DE.IsDeleted = 0 AND EC.IsDeleted = 0
		ORDER BY DE.ExpenseDate ASC
		OFFSET @PageSize*@PageNo ROWS
		FETCH NEXT @PageSize ROWS ONLY
END

GO
/****** Object:  StoredProcedure [dbo].[SP_Get_ExpenseCategory_Data_List]    Script Date: 2021-12-11 12:58:02 AM ******/
SET ANSI_NULLS ON
GO
SET QUOTED_IDENTIFIER ON
GO
--		SP_Get_ExpenseCategory_Data_List '20','0'

CREATE PROC [dbo].[SP_Get_ExpenseCategory_Data_List]
(
@PageSize int =15,
@PageNo int=0
)
AS
BEGIN
		DECLARE @TOTALROW INT
		SET @TOTALROW = ( SELECT COUNT(*) FROM ExpenseCategories WHERE IsDeleted = 0 )

	SELECT Id,CategoryName,
		@TOTALROW TotalRow		
		FROM ExpenseCategories  WHERE IsDeleted = 0
		ORDER BY Id ASC
		OFFSET @PageSize*@PageNo ROWS
		FETCH NEXT @PageSize ROWS ONLY
END

GO
 ]
 Step - 4 : run the application
 Here you can see two Navbar name as (1. Expense Category 2. Daily Expenses) 
