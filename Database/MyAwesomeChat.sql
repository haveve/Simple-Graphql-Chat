CREATE DATABASE MyAwesomeChat
GO

USE DATABASE MyAwesomeChat
GO

CREATE TABLE Users(
id INT NOT NULL IDENTITY, 
nick_name NVARCHAR(75) NOT NULL,
email NVARCHAR(500) NOT NULL,
password NVARCHAR(50) NULL,
activate_code NVARCHAR(500) NULL,
CONSTRAINT PK_Users_Id PRIMARY KEY(id)
)
GO

CREATE TABLE Chat(
id INT NOT NULL IDENTITY, 
name NVARCHAR(200) NOT NULL,
creator INT NOT NULL,

CONSTRAINT PK_Chat_Id PRIMARY KEY(id),
CONSTRAINT FK_Chat_UserId FOREIGN KEY (creator) REFERENCES Users(id) ON DELETE NO ACTION,  
CONSTRAINT UQ_Chat_creator UNIQUE(creator)
)
GO

CREATE TABLE Users_Chat_Keys(
	user_id INT NOT NULL,
	chat_id INT NOT NULL,

CONSTRAINT FK_Users_Chat_Keys_UserId FOREIGN KEY(user_id) REFERENCES Users(id) ON DELETE NO ACTION,
CONSTRAINT FK_Users_Chat_Keys_Chat FOREIGN KEY(chat_id) REFERENCES Chat(id) ON DELETE NO ACTION,
CONSTRAINT UQ_Users_Chat_Keys_UserChatIds Primary Key(user_id,chat_id)
)
GO

GO
CREATE TRIGGER dbo.uct_Chat_InsteadDelete 
ON dbo.Chat
INSTEAD OF DELETE
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @id INT 

	DECLARE crs_deleted_chat CURSOR
	FOR select id from deleted

	OPEN crs_deleted_chat

	FETCH NEXT FROM crs_deleted_chat 
	INTO @id

	WHILE(@@FETCH_STATUS = 0)
	BEGIN
		DELETE FROM Users_Chat_Keys WHERE chat_id = @id
		DELETE FROM Chat WHERE id = @id
		FETCH NEXT FROM crs_deleted_chat 
		INTO @id
	END

	CLOSE crs_deleted_chat
	DEALLOCATE crs_deleted_chat
END
GO

GO
CREATE TRIGGER dbo.uct_Users_InsteadDelete 
ON dbo.Users
INSTEAD OF DELETE
AS
BEGIN

	SET NOCOUNT ON

	DECLARE @id INT 

	DECLARE crs_deleted_user CURSOR
	FOR select id from deleted

	OPEN crs_deleted_user

	FETCH NEXT FROM crs_deleted_user 
	INTO @id

	WHILE(@@FETCH_STATUS = 0)
	BEGIN
		DELETE FROM Chat WHERE creator = @id
		DELETE FROM Users_Chat_Keys WHERE user_id = @id
		DELETE FROM Users WHERE id = @id
		FETCH NEXT FROM crs_deleted_user 
		INTO @id
	END
	
	CLOSE crs_deleted_user
	DEALLOCATE crs_deleted_user
END
