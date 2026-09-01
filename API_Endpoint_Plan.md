# RaceDay API Endpoint Plan

## 1. Authentication

| HTTP Method | Route                | Description                               | Role Required          | Request Body                                      | Expected Response |
| ----------- | -------------------- | ----------------------------------------- | ---------------------- | ------------------------------------------------- | ----------------- |
| POST        | `/api/auth/register` | Registers a new user account              | None                   | FirstName, LastName, Email, Password, Role, Phone | 201 Created       |
| POST        | `/api/auth/login`    | Authenticates a user and starts a session | None                   | Email, Password                                   | 200 OK            |
| POST        | `/api/auth/logout`   | Logs the current user out                 | Any authenticated user | None                                              | 200 OK            |

## 2. User Profile

| HTTP Method | Route                | Description                                 | Role Required           | Request Body                      | Expected Response |
| ----------- | -------------------- | ------------------------------------------- | ----------------------- | --------------------------------- | ----------------- |
| GET         | `/api/users/profile` | Retrieves the profile of the logged-in user | Participant / Organiser | None                              | 200 OK            |
| PUT         | `/api/users/profile` | Updates the logged-in user's profile        | Participant / Organiser | FirstName, LastName, Email, Phone | 200 OK            |

## 3. Events

| HTTP Method | Route              | Description                           | Role Required | Request Body                                                | Expected Response              |
| ----------- | ------------------ | ------------------------------------- | ------------- | ----------------------------------------------------------- | ------------------------------ |
| GET         | `/api/events`      | Retrieves all available events        | None          | None                                                        | 200 OK                         |
| GET         | `/api/events/{id}` | Retrieves details of a specific event | None          | None                                                        | 200 OK / 404 Not Found         |
| POST        | `/api/events`      | Creates a new race event              | Organiser     | Name, Description, EventDate, Location, Distance, EventType | 201 Created                    |
| PUT         | `/api/events/{id}` | Updates an existing event             | Organiser     | Name, Description, EventDate, Location, Distance, EventType | 200 OK / 404 Not Found         |
| DELETE      | `/api/events/{id}` | Deletes an existing event             | Organiser     | None                                                        | 204 No Content / 404 Not Found |

## 4. Categories

| HTTP Method | Route                              | Description                                | Role Required | Request Body              | Expected Response              |
| ----------- | ---------------------------------- | ------------------------------------------ | ------------- | ------------------------- | ------------------------------ |
| GET         | `/api/events/{eventId}/categories` | Retrieves categories belonging to an event | None          | None                      | 200 OK                         |
| POST        | `/api/events/{eventId}/categories` | Creates a category for an event            | Organiser     | CategoryName, Description | 201 Created                    |
| PUT         | `/api/categories/{id}`             | Updates an event category                  | Organiser     | CategoryName, Description | 200 OK / 404 Not Found         |
| DELETE      | `/api/categories/{id}`             | Deletes an event category                  | Organiser     | None                      | 204 No Content / 404 Not Found |

## 5. Event Enrolments

| HTTP Method | Route                              | Description                                            | Role Required | Request Body        | Expected Response              |
| ----------- | ---------------------------------- | ------------------------------------------------------ | ------------- | ------------------- | ------------------------------ |
| POST        | `/api/enrolments`                  | Enrols a participant in an event and selected category | Participant   | EventID, CategoryID | 201 Created                    |
| GET         | `/api/enrolments/my`               | Retrieves the logged-in participant's enrolments       | Participant   | None                | 200 OK                         |
| GET         | `/api/events/{eventId}/enrolments` | Retrieves enrolments for a specific event              | Organiser     | None                | 200 OK                         |
| DELETE      | `/api/enrolments/{id}`             | Cancels a participant's enrolment                      | Participant   | None                | 204 No Content / 404 Not Found |

## 6. Results

| HTTP Method | Route                           | Description                                   | Role Required | Request Body                               | Expected Response      |
| ----------- | ------------------------------- | --------------------------------------------- | ------------- | ------------------------------------------ | ---------------------- |
| POST        | `/api/results`                  | Records a participant's race result           | Organiser     | EnrolmentID, FinishTime, FinishingPosition | 201 Created            |
| PUT         | `/api/results/{id}`             | Updates an existing race result               | Organiser     | FinishTime, FinishingPosition              | 200 OK / 404 Not Found |
| GET         | `/api/results/my`               | Retrieves the logged-in participant's results | Participant   | None                                       | 200 OK                 |
| GET         | `/api/events/{eventId}/results` | Retrieves results for an event                | Organiser     | None                                       | 200 OK                 |

## Summary

The RaceDay API is divided into six main areas: Authentication, User Profile, Events, Categories, Event Enrolments and Results. The endpoints use appropriate HTTP methods and role-based access so that Organisers can manage events and results while Participants can enrol in events and view their own information and results.
