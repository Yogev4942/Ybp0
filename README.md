Your body's Preformance (YBP) — Workout Manager & Lifter Platform

12th Grade Final Project · 5 Units Computer Science (Software Engineering Track)


Your body's Preformance(YBP0) is a mobile fitness platform built for two types of users: trainees and trainers.
A trainee joins the app, gets assigned to a trainer, and from that point on their trainer can build and customize entire workout programs for them — choosing exercises, setting sets, reps, and weight, and tracking progress over time. Every workout is logged, and the trainee sees a visual body model highlighting which muscles they worked that session and across their whole history.
Beyond the trainer/trainee relationship, Ybp0 also acts as a social platform for lifters — users can post their personal records and lifts, see what others are hitting, and follow the progress of the lifting community around them.
The app is designed to be the single place a lifter needs: a coach in their pocket and a community on the same screen.

Features

Trainer dashboard to manage assigned trainees and build their workout programs
Per-trainee workout logging with sets, reps, and weight per exercise
Visual human body model with worked muscles highlighted per session and over history
Home screen statistics: total workouts, workouts this week, most-worked muscle group
Community feed for posting and viewing personal records and lifts
Exercise library organized by muscle group
Full CRUD for users, trainees, trainers, exercises, and workouts via REST API


Solution Structure
Ybp0.sln
├── Models/           Domain model classes (User, Trainee, Trainer, Exercise, Workout, ...)
├── ViewModels/       Request and response objects used by the API and MAUI app
├── DataBase/         EF Core DbContext, repository interfaces, and repository implementations
├── WebServices/      ASP.NET Core Web API — controllers, Program.cs
└── Ybp0Maui/         .NET MAUI mobile app — pages, services, ViewModels for UI

Technical Stack
Language

C# 12 / .NET 8 — used across every project in the solution

Backend — WebServices
ComponentDetailFrameworkASP.NET Core Web API (.NET 8)ORMEntity Framework Core 8DatabaseSQLite (single .db file, zero-installation)API DocumentationSwagger / SwashbuckleArchitectureRepository Pattern with interface-based DI
Mobile — Ybp0Maui
ComponentDetailFramework.NET MAUI (Multi-platform App UI)UI LanguageXAML + C# code-behindArchitectureMVVM (Model–View–ViewModel)HTTP ClientSystem.Net.Http.HttpClient with System.Net.Http.JsonNavigationShell navigation
NuGet Packages
DataBase project:

Microsoft.EntityFrameworkCore — core ORM
Microsoft.EntityFrameworkCore.Sqlite — SQLite provider
Microsoft.EntityFrameworkCore.Design — migration tooling

WebServices project:

Microsoft.EntityFrameworkCore.Design
Swashbuckle.AspNetCore — Swagger UI

Ybp0Maui project:

Microsoft.Maui.Controls — built-in with MAUI workload
CommunityToolkit.Mvvm — source-generated MVVM (ObservableObject, RelayCommand)

Design Patterns
PatternWhere UsedRepository PatternDataBase/ — IWorkoutRepository, IExerciseRepository, etc.MVVMYbp0Maui/ — every page has a matching ViewModelDependency InjectionWebServices/Program.cs — all repositories registered via AddScopedDTO / ViewModel separationViewModels/ — API never exposes raw model classes directlyRESTful APIWebServices/Controllers/ — standard HTTP verbs and status codes
Database Schema Overview
User ──< Trainer ──< Trainee ──< Workout ──< WorkoutExercise >── Exercise >── MuscleGroup

A User is either a Trainer or a Trainee
A Trainer manages many Trainees
A Trainee logs many Workouts
Each Workout contains many WorkoutExercise entries (exercise + sets + reps + weight)
Each Exercise belongs to a MuscleGroup


Yogev — 12th Grade, Software Engineering Track
Final project submitted for 5-unit Computer Science Bagrut (בגרות)Content
