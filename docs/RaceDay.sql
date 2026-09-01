IF DB_ID('RaceDay') IS NULL
    CREATE DATABASE RaceDay;
GO

USE RaceDay;
GO

USE RaceDay;
GO

SELECT * FROM Users;
SELECT * FROM Events;
SELECT * FROM Categories;
SELECT * FROM Enrolments;
SELECT * FROM Results;
SELECT * FROM EventImages;

IF OBJECT_ID('EventImages', 'U') IS NOT NULL DROP TABLE EventImages;
IF OBJECT_ID('Results', 'U') IS NOT NULL DROP TABLE Results;
IF OBJECT_ID('Enrolments', 'U') IS NOT NULL DROP TABLE Enrolments;
IF OBJECT_ID('Categories', 'U') IS NOT NULL DROP TABLE Categories;
IF OBJECT_ID('Events', 'U') IS NOT NULL DROP TABLE Events;
IF OBJECT_ID('Users', 'U') IS NOT NULL DROP TABLE Users;
GO

CREATE TABLE Users
(
UserID INT IDENTITY(1,1) PRIMARY KEY,
FirstName VARCHAR(50) NOT NULL,
LastName VARCHAR(50) NOT NULL,
Email VARCHAR(100) NOT NULL UNIQUE,
PasswordHash VARCHAR(255) NOT NULL,
Role VARCHAR(20) NOT NULL,
Phone VARCHAR(20)
);

CREATE TABLE Events
(
EventID INT IDENTITY(1,1) PRIMARY KEY,
OrganiserID INT NOT NULL,
Name VARCHAR(100) NOT NULL,
Description VARCHAR(500),
EventDate Date NOT NULL,
Location VARCHAR(200) NOT NULL,
Distance DECIMAL(6,2) NOT NULL,
EventType VARCHAR(50) NOT NULL,

CONSTRAINT FK_Events_Users
  FOREIGN KEY (OrganiserID)
  REFERENCES Users(UserID)
);

CREATE TABLE Categories
(
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    CategoryName VARCHAR(100) NOT NULL,
    Description VARCHAR(255),

    CONSTRAINT FK_Categories_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);

CREATE TABLE Enrolments
(
    EnrolmentID INT IDENTITY(1,1) PRIMARY KEY,
    ParticipantID INT NOT NULL,
    EventID INT NOT NULL,
    CategoryID INT NOT NULL,
    EnrolmentDate DATETIME2 NOT NULL DEFAULT GETDATE(),
    Status VARCHAR(20) NOT NULL DEFAULT 'Confirmed',

    CONSTRAINT FK_Enrolments_Participant
        FOREIGN KEY (ParticipantID)
        REFERENCES Users(UserID),

    CONSTRAINT FK_Enrolments_Event
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID),

    CONSTRAINT FK_Enrolments_Category
        FOREIGN KEY (CategoryID)
        REFERENCES Categories(CategoryID)
);

CREATE TABLE Results
(
    ResultID INT IDENTITY(1,1) PRIMARY KEY,
    EnrolmentID INT NOT NULL UNIQUE,
    FinishTime TIME NOT NULL,
    FinishingPosition INT NOT NULL,

    CONSTRAINT FK_Results_Enrolments
        FOREIGN KEY (EnrolmentID)
        REFERENCES Enrolments(EnrolmentID)
);

CREATE TABLE EventImages
(
    ImageID INT IDENTITY(1,1) PRIMARY KEY,
    EventID INT NOT NULL,
    ImageURL VARCHAR(500) NOT NULL,
    UploadedDate DATETIME2 NOT NULL DEFAULT GETDATE(),

    CONSTRAINT FK_EventImages_Events
        FOREIGN KEY (EventID)
        REFERENCES Events(EventID)
);

INSERT INTO Users
    (FirstName, LastName, Email, Role, PasswordHash, Phone)
VALUES
    ('Thabo', 'Mokoena', 'thabo.organiser@raceday.co.za', 'PasswordHash1', 'Organiser', '0711111111'),
    ('Lerato', 'Dlamini', 'lerato.organiser@raceday.co.za', 'PasswordHash2', 'Organiser', '0722222222'),
    ('Sipho', 'Nkosi', 'sipho.participant@raceday.co.za', 'PasswordHash3', 'Participant', '0733333333'),
    ('Amahle', 'Ndlovu', 'amahle.participant@raceday.co.za', 'PasswordHash4', 'Participant', '0744444444');

INSERT INTO Events
    (OrganiserID, Name, Description, EventDate, Location, Distance, EventType)
VALUES
    (1, 'Limpopo Road Run', 
     'A road running event for participants of different categories.',
     '2026-10-10', 'Polokwane', 10.00, 'Running'),

    (1, 'Tzaneen Mountain Challenge', 
     'A challenging outdoor running event through the Tzaneen area.',
     '2026-11-14', 'Tzaneen', 21.00, 'Running'),

    (2, 'Limpopo Family Fun Walk', 
     'A family-friendly walking event suitable for community participants.',
     '2026-12-05', 'Mokopane', 5.00, 'Walking');

INSERT INTO Categories
    (EventID, CategoryName, Description)
VALUES
    (1, '10km Open', '10 kilometre open category.'),
    (1, '10km Under 20', '10 kilometre category for participants under 20.'),

    (2, '21km Open', '21 kilometre open category.'),
    (2, '21km Senior', '21 kilometre senior category.'),

    (3, '5km Open', '5 kilometre open walking category.'),
    (3, '5km Family', '5 kilometre family walking category.');

INSERT INTO Enrolments
    (ParticipantID, EventID, CategoryID, Status)
VALUES
    (3, 1, 1, 'Confirmed'),
    (4, 1, 2, 'Confirmed'),
    (3, 2, 3, 'Confirmed'),
    (4, 3, 5, 'Confirmed');

INSERT INTO Results
    (EnrolmentID, FinishTime, FinishingPosition)
VALUES
    (1, '00:52:30', 1),
    (2, '01:05:45', 2);

INSERT INTO EventImages
    (EventID, ImageURL)
VALUES
    (1, 'https://example.com/images/limpopo-road-run.jpg'),
    (2, 'https://example.com/images/tzaneen-mountain-challenge.jpg'),
    (3, 'https://example.com/images/limpopo-family-fun-walk.jpg');
