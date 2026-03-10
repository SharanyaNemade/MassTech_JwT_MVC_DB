create database JwtDB;


CREATE TABLE Users
(
    Id INT IDENTITY,
    Username NVARCHAR(50),
    Password NVARCHAR(50)
)



INSERT INTO Users (Username, Password)
VALUES ('admin', '1234');

select * from users;



CREATE PROCEDURE sp_UserLogin
(
    @Username NVARCHAR(50),
    @Password NVARCHAR(50)
)
AS
BEGIN
    SELECT Id, Username
    FROM Users
    WHERE Username = @Username
    AND Password = @Password
END

EXEC sp_UserLogin 'admin','admin'