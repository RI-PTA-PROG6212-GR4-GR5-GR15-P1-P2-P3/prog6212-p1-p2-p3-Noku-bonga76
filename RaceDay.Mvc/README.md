##Part 3 – MVC, Azure Blob Storage and Docker

### Architecture
Browser → RaceDay.Mvc (ASP.NET Core MVC) → RaceDay.API → SQL Server + Azure Blob Storage.
The MVC app has no database or Azure access. Every call goes through `Services/ApiClient.cs`, which forwards the API's session cookie. Roles are checked in the API (`RequireRole`) and in the MVC app (`RoleAttribute`).

### Roles
- **Organiser**: dashboard, create/edit/delete events with banner upload, categories, enrolments, results entry.
- **Participant**: browse and filter events, enrol in a category, My Events (with cancel), My Results, profile with picture.

### Run locally
1. Set the Azure connection string once, from inside `RaceDay.API`:
   `dotnet user-secrets set "AzureBlob:ConnectionString" "<your connection string>"`
2. Start the API: `dotnet run --project RaceDay.API --launch-profile https`
3. Start the MVC app: `dotnet run --project RaceDay.Mvc`
   (`Api:BaseUrl` in `RaceDay.Mvc/appsettings.json` must match the API address.)

### Run with Docker
Start the API so the container can reach it:
`dotnet run --project RaceDay.API --urls "http://0.0.0.0:5070"`

Build and run the MVC container: Open http://localhost:8080

### Azure Blob Storage
Event banners and profile pictures are uploaded by the MVC app to the API, which stores them in the `raceday-images` container. Banner URLs are saved in the `EventImages` table and profile picture URLs in `Users.ProfilePictureUrl`. The connection string is stored in user-secrets.

### Differences from the Part 1 plan
- Added `POST /api/events/{id}/banner` and `POST /api/users/profile/picture`.
- Added the `ProfilePictureUrl` column to `Users`.
- `PUT /api/results` takes the `ResultID` in the body.
- Profile, "my enrolments" and "my results" use the logged-in user from the session instead of an ID in the URL.

### CI/CD
<img width="570" height="50" alt="Screenshot 2026-11-05 131249" src="https://github.com/user-attachments/assets/970d07b1-81d4-473b-9747-55dbd92694bb" />

### Video
<unlisted YouTube link>

