# Typical Tech Tools

Typical Tech Tools is an learning material from TAFE 2026 Jan Enrollement. It's an ASP.NET Core MVC application that stores products and reviews in SQL Server through Entity Framework Core. This guide explains how to create a new database, migrate the supplied CSV data and run the application on another computer.

## Requirements

- .NET 10 SDK
- SQL Server (Windows installation or Docker container)
- A SQL Server login that can create and update the `TypicalTechTools` database
- Optional: SQL Server Management Studio, Azure Data Studio or Rider Database Tools
- Entity Framework Core command-line tool

Check the installed .NET version and install the EF Core tool if required:

```powershell
dotnet --version
dotnet tool install --global dotnet-ef
```

If `dotnet-ef` is already installed, update it instead:

```powershell
dotnet tool update --global dotnet-ef
```

## 1. Configure the database connection

Open `TypicalTechTools/appsettings.json` and replace the example connection details with details for the local SQL Server instance.

SQL Server authentication, including a Docker SQL Server:

```json
"ConnectionStrings": {
  "Default": "Server=localhost,1433;Database=TypicalTechTools;User Id=your_user_id;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
}
```

Windows authentication:

```json
"ConnectionStrings": {
  "Default": "Server=localhost;Database=TypicalTechTools;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

SQL Server Express:

```json
"ConnectionStrings": {
  "Default": "Server=localhost\\SQLEXPRESS;Database=TypicalTechTools;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

The double backslash is required in a JSON connection string. Use an assessment-only password and do not reuse a personal or production password.

## 2. Restore and build the project

Run these commands from the repository root:

```powershell
dotnet restore TypicalTechTools/TypicalTechTools.csproj
dotnet build TypicalTechTools/TypicalTechTools.csproj
```

The build must complete without errors before continuing.

## 3. Create the database schema

Apply all included EF Core migrations:

```powershell
dotnet ef database update --project TypicalTechTools/TypicalTechTools.csproj
```

This creates the `Products`, `Reviews` and EF migration-history tables. It does **not** import the CSV records. After all migrations are applied, the Products and Reviews tables are expected to be empty until the CSV import is completed.

Confirm the schema in SQL Server:

```sql
SELECT name
FROM sys.tables
WHERE name IN ('Products', 'Reviews', '__EFMigrationsHistory');
```

## 4. Make the CSV files accessible to SQL Server

The source files are:

- `TypicalTechTools/wwwroot/data/Products.csv` — expected data rows: **6**
- `TypicalTechTools/wwwroot/data/Reviews.csv` — expected data rows: **4**

The files do not contain header rows. `BULK INSERT` reads files from the computer or container running SQL Server, not from the web application process.

### SQL Server in Docker

Find the container name:

```powershell
docker ps --format "table {{.Names}}\t{{.Image}}\t{{.Ports}}"
```

Replace `YOUR_SQL_CONTAINER` below with that name:

```powershell
docker exec --user root YOUR_SQL_CONTAINER mkdir -p /var/opt/mssql/import
docker cp TypicalTechTools/wwwroot/data/Products.csv YOUR_SQL_CONTAINER:/var/opt/mssql/import/Products.csv
docker cp TypicalTechTools/wwwroot/data/Reviews.csv YOUR_SQL_CONTAINER:/var/opt/mssql/import/Reviews.csv
docker exec --user root YOUR_SQL_CONTAINER chown -R mssql:mssql /var/opt/mssql/import
```

Use these paths in the `BULK INSERT` statements:

```text
/var/opt/mssql/import/Products.csv
/var/opt/mssql/import/Reviews.csv
```

### SQL Server installed on Windows

Copy both files to a folder such as `C:\SqlImport`. The Windows account running the SQL Server service must have read access to that folder. Use these paths in the `BULK INSERT` statements:

```text
C:\SqlImport\Products.csv
C:\SqlImport\Reviews.csv
```

## 5. Create and load the staging tables

Connect to the `TypicalTechTools` database and run the following SQL. Change only the two file paths if SQL Server is not running in Docker.

```sql
IF OBJECT_ID('dbo.StagingProducts', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StagingProducts
    (
        StagingId int IDENTITY(1,1) PRIMARY KEY,
        ProductIdText nvarchar(255) NULL,
        ProductNameText nvarchar(255) NULL,
        ProductPriceText nvarchar(50) NULL,
        ProductDescriptionText nvarchar(max) NULL
    );
END;

IF OBJECT_ID('dbo.StagingReviews', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.StagingReviews
    (
        StagingId int IDENTITY(1,1) PRIMARY KEY,
        ReviewIdText nvarchar(255) NULL,
        ReviewText nvarchar(max) NULL,
        ProductIdText nvarchar(255) NULL
    );
END;

TRUNCATE TABLE dbo.StagingProducts;
TRUNCATE TABLE dbo.StagingReviews;

BULK INSERT dbo.StagingProducts
FROM '/var/opt/mssql/import/Products.csv'
WITH
(
    FORMAT = 'CSV',
    FIRSTROW = 1,
    FIELDQUOTE = '"',
    ROWTERMINATOR = '0x0a',
    CODEPAGE = '65001',
    TABLOCK
);

BULK INSERT dbo.StagingReviews
FROM '/var/opt/mssql/import/Reviews.csv'
WITH
(
    FORMAT = 'CSV',
    FIRSTROW = 1,
    FIELDQUOTE = '"',
    ROWTERMINATOR = '0x0a',
    CODEPAGE = '65001',
    TABLOCK
);

SELECT COUNT(*) AS StagedProducts FROM dbo.StagingProducts;
SELECT COUNT(*) AS StagedReviews FROM dbo.StagingReviews;
```

Expected results:

| Check | Expected count |
|---|---:|
| Staged products | 6 |
| Staged reviews | 4 |

## 6. Validate the staged records

Run the following checks before importing. Each query must return no rows. The expressions remove a UTF-8 byte-order mark from the first identifier and remove carriage-return characters from the final CSV field.

```sql
-- Invalid product values
SELECT *
FROM dbo.StagingProducts
WHERE TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, NCHAR(65279), ''))) IS NULL
   OR NULLIF(TRIM(ProductNameText), '') IS NULL
   OR TRY_CONVERT(decimal(18,2), TRIM(ProductPriceText)) IS NULL
   OR TRY_CONVERT(decimal(18,2), TRIM(ProductPriceText)) < 0
   OR NULLIF(TRIM(REPLACE(ProductDescriptionText, CHAR(13), '')), '') IS NULL;

-- Duplicate product identifiers
SELECT TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, NCHAR(65279), ''))) AS ProductId,
       COUNT(*) AS DuplicateCount
FROM dbo.StagingProducts
GROUP BY TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, NCHAR(65279), '')))
HAVING COUNT(*) > 1;

-- Invalid review values
SELECT *
FROM dbo.StagingReviews
WHERE TRY_CONVERT(int, TRIM(REPLACE(ReviewIdText, NCHAR(65279), ''))) IS NULL
   OR NULLIF(TRIM(ReviewText), '') IS NULL
   OR TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, CHAR(13), ''))) IS NULL;

-- Duplicate review identifiers
SELECT TRY_CONVERT(int, TRIM(REPLACE(ReviewIdText, NCHAR(65279), ''))) AS ReviewId,
       COUNT(*) AS DuplicateCount
FROM dbo.StagingReviews
GROUP BY TRY_CONVERT(int, TRIM(REPLACE(ReviewIdText, NCHAR(65279), '')))
HAVING COUNT(*) > 1;

-- Reviews that do not match a staged product
SELECT r.*
FROM dbo.StagingReviews AS r
LEFT JOIN dbo.StagingProducts AS p
    ON TRY_CONVERT(int, TRIM(REPLACE(r.ProductIdText, CHAR(13), '')))
     = TRY_CONVERT(int, TRIM(REPLACE(p.ProductIdText, NCHAR(65279), '')))
WHERE p.StagingId IS NULL;
```

Do not continue if any validation query returns a row. Correct the source or staging data, reload both staging tables and repeat all validation checks.

## 7. Import the validated data in one transaction

The target tables must be empty before this first full import. The transaction imports Products before Reviews because Reviews contains the foreign key. Any error rolls back the entire import.

```sql
SET XACT_ABORT ON;

DECLARE @ImportDate datetime2 = SYSUTCDATETIME();
DECLARE @ProductsIdentityOn bit = 0;
DECLARE @ReviewsIdentityOn bit = 0;
DECLARE @ImportedProducts int = 0;
DECLARE @ImportedReviews int = 0;

BEGIN TRY
    BEGIN TRANSACTION;

    IF EXISTS (SELECT 1 FROM dbo.Reviews)
       OR EXISTS (SELECT 1 FROM dbo.Products)
        THROW 50001, 'Import stopped: Products or Reviews already contains data.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.StagingProducts
        WHERE TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, NCHAR(65279), ''))) IS NULL
           OR NULLIF(TRIM(ProductNameText), '') IS NULL
           OR TRY_CONVERT(decimal(18,2), TRIM(ProductPriceText)) IS NULL
           OR TRY_CONVERT(decimal(18,2), TRIM(ProductPriceText)) < 0
           OR NULLIF(TRIM(REPLACE(ProductDescriptionText, CHAR(13), '')), '') IS NULL
    )
        THROW 50002, 'Import stopped: staged product validation failed.', 1;

    IF EXISTS
    (
        SELECT 1
        FROM dbo.StagingReviews AS r
        LEFT JOIN dbo.StagingProducts AS p
            ON TRY_CONVERT(int, TRIM(REPLACE(r.ProductIdText, CHAR(13), '')))
             = TRY_CONVERT(int, TRIM(REPLACE(p.ProductIdText, NCHAR(65279), '')))
        WHERE TRY_CONVERT(int, TRIM(REPLACE(r.ReviewIdText, NCHAR(65279), ''))) IS NULL
           OR NULLIF(TRIM(r.ReviewText), '') IS NULL
           OR TRY_CONVERT(int, TRIM(REPLACE(r.ProductIdText, CHAR(13), ''))) IS NULL
           OR p.StagingId IS NULL
    )
        THROW 50003, 'Import stopped: staged review validation failed.', 1;

    SET IDENTITY_INSERT dbo.Products ON;
    SET @ProductsIdentityOn = 1;

    INSERT INTO dbo.Products
    (
        ProductId,
        ProductName,
        ProductPrice,
        ProductDescription,
        UpdatedDate
    )
    SELECT
        TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, NCHAR(65279), ''))),
        TRIM(ProductNameText),
        TRY_CONVERT(decimal(18,2), TRIM(ProductPriceText)),
        TRIM(REPLACE(ProductDescriptionText, CHAR(13), '')),
        @ImportDate
    FROM dbo.StagingProducts;

    SET @ImportedProducts = @@ROWCOUNT;

    SET IDENTITY_INSERT dbo.Products OFF;
    SET @ProductsIdentityOn = 0;

    SET IDENTITY_INSERT dbo.Reviews ON;
    SET @ReviewsIdentityOn = 1;

    INSERT INTO dbo.Reviews
    (
        ReviewId,
        ReviewText,
        ProductId,
        CreatedDate
    )
    SELECT
        TRY_CONVERT(int, TRIM(REPLACE(ReviewIdText, NCHAR(65279), ''))),
        TRIM(ReviewText),
        TRY_CONVERT(int, TRIM(REPLACE(ProductIdText, CHAR(13), ''))),
        @ImportDate
    FROM dbo.StagingReviews;

    SET @ImportedReviews = @@ROWCOUNT;

    SET IDENTITY_INSERT dbo.Reviews OFF;
    SET @ReviewsIdentityOn = 0;

    COMMIT TRANSACTION;

    SELECT
        @ImportedProducts AS ImportedProducts,
        @ImportedReviews AS ImportedReviews,
        @ImportDate AS ImportDateUtc;
END TRY
BEGIN CATCH
    IF XACT_STATE() <> 0
        ROLLBACK TRANSACTION;

    IF @ProductsIdentityOn = 1
    BEGIN TRY
        SET IDENTITY_INSERT dbo.Products OFF;
    END TRY
    BEGIN CATCH
        PRINT 'Could not reset IDENTITY_INSERT for Products.';
    END CATCH;

    IF @ReviewsIdentityOn = 1
    BEGIN TRY
        SET IDENTITY_INSERT dbo.Reviews OFF;
    END TRY
    BEGIN CATCH
        PRINT 'Could not reset IDENTITY_INSERT for Reviews.';
    END CATCH;

    THROW;
END CATCH;
```

Expected result: `ImportedProducts = 6` and `ImportedReviews = 4`.

## 8. Verify the completed migration

```sql
SELECT COUNT(*) AS ProductCount FROM dbo.Products;
SELECT COUNT(*) AS ReviewCount FROM dbo.Reviews;

SELECT
    p.ProductId,
    p.ProductName,
    p.ProductPrice,
    p.ProductDescription,
    p.UpdatedDate
FROM dbo.Products AS p
ORDER BY p.ProductId;

SELECT
    r.ReviewId,
    r.ReviewText,
    r.ProductId,
    r.CreatedDate,
    p.ProductName
FROM dbo.Reviews AS r
INNER JOIN dbo.Products AS p ON p.ProductId = r.ProductId
ORDER BY r.ReviewId;

SELECT r.*
FROM dbo.Reviews AS r
LEFT JOIN dbo.Products AS p ON p.ProductId = r.ProductId
WHERE p.ProductId IS NULL;
```

Confirm that:

- Products contains 6 rows.
- Reviews contains 4 rows.
- The final orphan-review query returns no rows.
- Several names, prices, descriptions and reviews match the source CSV files.
- `UpdatedDate` and `CreatedDate` contain the migration time.

## 9. Run and test the application

```powershell
dotnet run --project TypicalTechTools/TypicalTechTools.csproj
```

Open the HTTPS address printed in the terminal. The default development profile uses:

```text
https://localhost:7050
```

Verify that:

- The product list displays the six imported products.
- Product details display related reviews.
- A product can be created and edited.
- A review can be created, edited and deleted.
- Invalid input displays validation messages and is not saved.
- Product prices and dates use the required display format.
- Warranty upload, download and deletion still work.

## Troubleshooting

### Login failed or server not found

- Confirm that SQL Server is running.
- Confirm the server name, port, username and password in `appsettings.json`.
- For Docker, confirm that port `1433` is published with `docker ps`.
- For Windows authentication, use `Trusted_Connection=True` and remove `User Id` and `Password`.

### `BULK INSERT` cannot open the file

- The path is interpreted by SQL Server, not by the ASP.NET application.
- For Docker, copy the files into the container and use the container paths.
- For Windows SQL Server, grant its service account read permission to the import folder.

### The import says the target tables contain data

The script deliberately prevents a second or partial import. Use a new assessment database, or delete test data only after making a backup. Because of the foreign key, Reviews must be deleted before Products:

```sql
BEGIN TRANSACTION;
DELETE FROM dbo.Reviews;
DELETE FROM dbo.Products;
COMMIT TRANSACTION;
```

Then reload and revalidate both staging tables before running the import again.

### HTTPS certificate warning

Trust the local development certificate:

```powershell
dotnet dev-certs https --trust
```

## Migration order summary

1. Configure the connection string.
2. Restore and build the project.
3. Run `dotnet ef database update` to create the schema.
4. Make the CSV files accessible to the SQL Server process.
5. Load the CSV rows into staging tables.
6. Validate all staged values and relationships.
7. Import Products and then Reviews inside one transaction.
8. Verify counts, values and foreign-key relationships.
9. Run the application and complete the user-interface tests.
