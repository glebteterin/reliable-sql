CREATE TABLE SqlWrapperTestLog (
	[Id] UNIQUEIDENTIFIER NOT NULL,
	[Count] INT NOT NULL DEFAULT (0)
);
GO

CREATE PROCEDURE [SqlWrapperTest]
	@id UNIQUEIDENTIFIER,
	@errorRepeat INT,
	@errorNumber INT,
	@errorMessage VARCHAR(300)
AS
BEGIN

	SET NOCOUNT ON;

	DECLARE @counter INT = NULL;
	DECLARE @exists BIT = 1;

	-- clear the log table from previous test data
	DELETE FROM [SqlWrapperTestLog] WHERE [Id] <> @id;

	SELECT	@counter = [Count]
	FROM	[SqlWrapperTestLog]
	WHERE	[Id] = @id;

	IF (@counter IS NULL)
	BEGIN
		SET @counter = @errorRepeat;
		SET @exists = 0;
	END

	SET @counter = @counter - 1;

	IF (@exists = 1)
		UPDATE [SqlWrapperTestLog] SET [Count] = @counter WHERE [Id] = @id;
	ELSE
		INSERT INTO [SqlWrapperTestLog]([Id],[Count]) VALUES(@id, @errorRepeat);

	IF (@counter > 0)
		THROW @errorNumber, @errorMessage, 1;
	ELSE
		BEGIN
			SELECT 'SUCCESS'
		END

END
GO