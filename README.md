[![Review Assignment Due Date](https://classroom.github.com/assets/deadline-readme-button-22041afd0340ce965d47ae6ef1cefeee28c7c493a6346c4f15d667ab976d596c.svg)](https://classroom.github.com/a/mr-hqvA6)

# RaceDay – Event Management System

## System Description

RaceDay is a full-stack, web-based event management platform built for the South African road running, walking, and cycling community. It replaces the paper-based registration, spreadsheets, and disconnected communication currently used by many local race organisers.

The platform allows **Event Organisers** to create and manage events, define categories, and capture participant results, while **Participants** can browse upcoming events, enter events, track their personal results history, and prepare for race day.

## User Roles

RaceDay supports two distinct user roles:

- **Organiser** — Can create, edit, and delete events; manage event categories; capture participant results; and view all enrolments for their events.
- **Participant** — Can register an account, browse events, enter an event by selecting a category, view their own enrolments, and track their personal race results.

## Repository Structure

```
/docs
  ├── ERD.png                      # Entity Relationship Diagram
  ├── API_Endpoint_Plan.md         # Full API endpoint specification table
  └── RaceDay_Database.sql         # SQL Server database creation + seed script
README.md
```

## Part 1 Deliverables

| Deliverable | Location | Description |
|---|---|---|
| ERD | `/docs/ERD.png` | Full data model with entities, attributes, primary/foreign keys, and cardinality |
| API Endpoint Plan | `/docs/API_Endpoint_Plan.md` | Table of all planned endpoints covering Authentication, User Profile, Events, Categories, Enrolments, and Results |
| SQL Script | `/docs/RaceDay_Database.sql` | CREATE TABLE statements matching the ERD, with constraints and seed data (2 Organisers, 2 Participants, 3 Events, categories, and enrolments) |

## Running the SQL Script

1. Open SQL Server Management Studio (SSMS).
2. Connect to a local or Azure SQL Server instance.
3. Open `docs/RaceDay_Database.sql`.
4. Execute the script against a clean/new database. It will create all tables, constraints, and seed data required for the RaceDay system.

## CI/CD
<img width="685" height="57" alt="Screenshot 2026-09-01 104021" src="https://github.com/user-attachments/assets/e88e6828-e462-4a1e-867a-6b3743857dbf" />
A GitHub Actions workflow (`.github/workflows/validate.yml`) runs on every push to verify that the `/docs` folder exists and contains the required planning files.
**Build status:** SUCCESSFULL

## Video Presentation
https://youtu.be/FhyP049WKPs

