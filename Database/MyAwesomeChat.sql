CREATE DATABASE MyAwesomeChat
GO

USE MyAwesomeChat
GO

CREATE TABLE Users(
id INT NOT NULL IDENTITY, 
nick_name NVARCHAR(75) NOT NULL,
email NVARCHAR(100) NOT NULL,
password NVARCHAR(50) NULL,
activate_code NVARCHAR(500) NULL,
salt nvarchar(24) NULL,
online bit not null Default 0,
key_2auth nvarchar(44) not null,
reset_key_2auth nvarchar(44) not null,

CONSTRAINT PK_Users_Id PRIMARY KEY(id),
CONSTRAINT UQ_Users_NickName Unique(nick_name),
CONSTRAINT UQ_Users_Email Unique(email)
)
GO

INSERT INTO Users(nick_name,email) ('DELETED','-')

CREATE TABLE Chat(
id INT NOT NULL IDENTITY, 
name NVARCHAR(100) NOT NULL,
creator INT NOT NULL,

CONSTRAINT PK_Chat_Id PRIMARY KEY(id),
CONSTRAINT FK_Chat_UserId FOREIGN KEY (creator) REFERENCES Users(id) ON DELETE NO ACTION,  
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

CREATE TABLE Message(
from_id int not null DEFAULT 1,
chat_id int not null,
sent_at DATETIME2(3) not null,
content nvarchar(200) not null

CONSTRAINT FK_Message_user_id FOREIGN KEY(from_id) REFERENCES Users(id) ON DELETE SET DEFAULT,
CONSTRAINT FK_Message_chat_id FOREIGN KEY(chat_id) REFERENCES Chat(id) ON DELETE CASCADE,

CONSTRAINT PK_Message PRIMARY KEY(sent_at,from_id,chat_id)
);

CREATE TABLE TechMessage(
chat_id int not null,
sent_at DATETIME2(3) not null,
content nvarchar(200) not null

CONSTRAINT FK_TechMessage_chat_id FOREIGN KEY(chat_id) REFERENCES Chat(id) ON DELETE CASCADE,

CONSTRAINT PK_TechMessage PRIMARY KEY(sent_at,chat_id)
);


CREATE TABLE UserRefreshes(
user_Id int not null, 
expiration_start datetime2(0) not null, 
expiration_end datetime2(0) not null, 
token nvarchar(300),

CONSTRAINT FK_UserRefreshes_user_id FOREIGN KEY(user_Id) REFERENCES Users(id) ON DELETE CASCADE,
CONSTRAINT PK_UserRefreshes PRIMARY KEY(token,user_Id)
);