You are a senior software architect, senior C# desktop engineer, senior security engineer, senior UI/UX designer, and senior product designer.

Your task is to build a FULL production-grade native Windows desktop application for personal work, freelance, income, expenses, savings, and project management.

The application must be built as a PREMIUM, PROFESSIONAL, MODERN, NATIVE DESKTOP APP — not a basic demo, not a toy app, and not a generic CRUD app.

==================================================
1. PRODUCT GOAL
==================================================

Build a premium desktop-native personal finance and work management system for a single user.

The user works with:
- multiple companies
- part-time jobs
- full-time jobs
- freelance clients
- freelance projects

The system should allow the user to:
- track companies and clients
- track projects worked on
- track income from salary / freelance / project payments / bonuses / other
- track personal expenses
- track work-related expenses
- track deductions / recurring obligations
- track savings
- calculate net balance
- link expenses to a company or project when needed
- generate premium dashboards and reports
- keep everything secure locally
- protect the app with authentication
- encrypt data
- create automatic local backups
- create encrypted cloud backup-ready files
- be extensible and maintainable

==================================================
2. TECH STACK REQUIREMENTS
==================================================

Use this exact stack unless there is a very strong technical reason to improve a detail:

- Language: C#
- Runtime: .NET 8
- Desktop Framework: WPF
- Pattern: MVVM
- UI Framework Styling: modern custom WPF styling with premium desktop feel
- Database: SQLite
- ORM / data access: Entity Framework Core
- Secure password hashing: Argon2
- Database encryption approach: design app in a way that can support encrypted database / secure key-based protection
- Local secrets storage: Windows Credential Manager where appropriate
- Logging: structured logging
- Dependency Injection: Microsoft.Extensions.DependencyInjection
- Configuration management: appsettings + secure local settings abstraction
- Backup format: versioned encrypted backup files
- Testing: xUnit for core services
- Architecture: Clean Architecture or a strong modular layered architecture

Do NOT build this as Electron, Tauri, MAUI, WinForms, or web app.
This must be a true native-feeling Windows desktop WPF application.

==================================================
3. APP POSITIONING
==================================================

This is NOT a simple expense tracker only.

This is a hybrid of:
- personal finance manager
- work income tracker
- freelance/project tracker
- savings tracker
- deductions tracker
- business activity organizer

The application is local-first, privacy-first, and premium.

==================================================
4. DESIGN GOALS
==================================================

The UI must look premium, modern, polished, elegant, and business-class.

UI design requirements:
- clean premium spacing
- modern desktop typography
- elegant cards
- smooth hierarchy
- soft shadows
- rounded corners used professionally
- refined icons
- premium dashboard feel
- balanced layout
- dark mode first, with optional light mode architecture
- subtle accent colors
- not childish
- not overly colorful
- not template-looking
- not default WPF look
- polished animations where appropriate but minimal
- high readability
- strong information density without clutter

The user should feel this is:
- professional
- premium
- secure
- serious
- high-end business software

Create a custom design system for the app:
- color palette
- typography scale
- spacing scale
- reusable buttons
- cards
- tables
- dialogs
- forms
- tabs
- sidebar
- top header
- KPIs
- charts area styling

Avoid ugly default controls.
Create a cohesive premium UI system.

==================================================
5. CORE MODULES
==================================================

Build the following modules:

A. Authentication / App Lock
- master password setup on first run
- login screen on app start
- secure password hashing using Argon2
- optional app auto-lock after inactivity
- lock screen re-entry
- session management for local app session only

B. Dashboard
- monthly total income
- monthly total expenses
- monthly total deductions
- monthly total savings
- net monthly balance
- active projects count
- companies/clients count
- recent transactions
- recent incomes
- recent projects
- charts for income vs expenses
- charts for category breakdown
- charts for monthly trend
- beautiful KPI cards

C. Companies / Clients Management
- add company/client
- edit company/client
- delete company/client with safe validation rules
- type: company / client
- engagement type: full-time / part-time / freelance / contract / other
- notes
- currency preferences
- status: active / inactive

D. Projects Management
- add project
- link to company/client
- project name
- project type
- status: planned / in progress / completed / on hold / cancelled
- agreed amount
- received amount
- expected amount
- start date
- end date
- notes
- tags
- completion tracking
- project profitability insight

E. Income Management
- add income entries
- edit / delete
- source type: salary / freelance / project payment / bonus / refund / other
- amount
- currency
- date
- linked company/client if any
- linked project if any
- payment status if needed
- notes
- recurring support if relevant

F. Expense Management
- add expenses
- edit / delete
- category
- amount
- currency
- date
- description
- linked to company/client optionally
- linked to project optionally
- personal or work-related flag
- recurring support
- attachments architecture ready even if not fully implemented now

G. Expense Categories
- CRUD for categories
- default categories seeded
- custom categories
- icon/color metadata support

H. Deductions / Obligations
- recurring monthly obligations
- one-time obligations
- title
- amount
- start date
- end date
- recurrence
- notes
- active/inactive

I. Savings
- savings entries
- savings goals
- progress tracking
- target amount
- saved amount
- notes
- dashboard integration

J. Reports & Insights
- monthly report
- yearly report
- income by source
- expense by category
- net by company/client
- project profitability
- monthly trends
- exportable report summaries

K. Settings
- app preferences
- auto-lock timeout
- default currency
- theme readiness
- backup preferences
- backup destination settings abstraction
- retention policy
- database maintenance actions
- export/import settings

L. Backup & Restore
- manual backup
- automatic backup
- local backup folder
- retention policy
- encrypted backup file creation
- restore backup flow
- backup metadata
- backup validation
- safe restore UX
- ability to prepare backup files suitable for cloud sync (for example Google Drive synced folder architecture)

==================================================
6. SECURITY REQUIREMENTS
==================================================

This is extremely important.

Implement strong security architecture for a local-first finance app.

Requirements:
- secure first-run master password setup
- Argon2 for password hashing
- never store plain text password
- secure session state
- app lock after inactivity
- minimize sensitive data exposure in logs
- secrets abstraction
- Windows Credential Manager integration where useful
- backup files must be encrypted
- design the data layer so it is ready for database-level encryption or strong file protection strategy
- avoid insecure coding patterns
- safe exception handling
- secure restore flow
- input validation
- transactional writes for critical operations
- data integrity checks where appropriate

Also:
- create a dedicated SecurityService
- create a BackupEncryptionService
- create session timeout handling
- create a secure password rules validator
- avoid storing secrets in plain config

==================================================
7. DATA MODEL REQUIREMENTS
==================================================

Design a clean normalized schema.

Include at minimum these entities:

- UserProfile / AppUser (single-user local profile)
- CompanyClient
- Project
- Income
- Expense
- ExpenseCategory
- Deduction
- SavingEntry
- SavingGoal
- Currency
- AppSetting
- BackupRecord
- AuditEntry (lightweight local audit trail for critical actions)
- Tag / ProjectTag if useful

Think carefully about relationships.

Expected relationship behavior:
- company/client can have many projects
- company/client can have many incomes
- company/client can have many expenses optionally
- project can have many incomes
- project can have many expenses
- categories can have many expenses
- saving goals can have many saving entries
- deductions can be recurring or one-time

Use sensible delete behavior:
- prefer soft-delete or restricted delete on important financial records
- avoid accidental cascade loss of money history

Add:
- CreatedAt
- UpdatedAt
- optional DeletedAt / IsDeleted where appropriate
- concurrency-safe thinking where needed

==================================================
8. ARCHITECTURE REQUIREMENTS
==================================================

Use a clean and scalable folder structure.

Suggested high-level structure:

- src/
  - App.Desktop
  - App.Application
  - App.Domain
  - App.Infrastructure
  - App.Tests

Expectations:
- Domain entities
- Value objects where useful
- DTOs
- services
- repositories or well-structured EF abstraction
- commands / queries if helpful
- ViewModels
- Views
- styles
- resources
- converters
- behaviors
- dialogs
- shared controls
- seed data
- migrations
- backup services
- security services
- reporting services

Use proper DI.
Keep code clean, maintainable, readable, and modular.
No spaghetti code.
No giant code-behind files.

==================================================
9. WPF UI/UX REQUIREMENTS
==================================================

Create a premium application shell with:
- left sidebar navigation
- top header
- dynamic content region
- polished login screen
- premium dashboard
- data tables with elegant styling
- strong form design
- clean dialogs/modals
- modern cards
- summary panels
- consistent icon system

Views to create:
- LoginView
- FirstRunSetupView
- DashboardView
- CompaniesView
- CompanyEditorDialog
- ProjectsView
- ProjectEditorDialog
- IncomeView
- IncomeEditorDialog
- ExpensesView
- ExpenseEditorDialog
- CategoriesView
- DeductionsView
- SavingsView
- ReportsView
- SettingsView
- BackupRestoreView
- LockScreenView

Premium details:
- hover states
- selected nav state
- disabled state
- empty states
- loading states
- error states
- success feedback
- clean confirmations
- not too many message boxes
- inline validation

Use charts in a way suitable for WPF and the overall design.
Ensure the app feels premium and modern.

==================================================
10. FUNCTIONAL QUALITY REQUIREMENTS
==================================================

The solution must be:
- production-minded
- robust
- strongly typed
- cleanly named
- easy to extend
- easy to maintain
- defensive against bad input
- not overengineered nonsense
- not underbuilt
- realistic and professional

Every feature should be built thoughtfully.

==================================================
11. DATABASE AND PERSISTENCE REQUIREMENTS
==================================================

Use EF Core migrations.
Seed sensible default data:
- expense categories
- maybe starter currencies
- basic settings

Use transactions for critical financial writes.
Create repository/service patterns only where they add clarity.
Do not create useless abstractions.

Include:
- DbContext
- migrations
- entity configurations
- indexes
- constraints
- validation logic

Optimize for a single-user local desktop app.

==================================================
12. BACKUP REQUIREMENTS
==================================================

Backup is critical.

Implement:
- local automatic backup
- manual backup
- encrypted backup output
- restore from backup
- backup history
- retention strategy
- timestamped backup naming
- integrity check before restore
- graceful restore flow
- backup configuration screen

Architect it so user can later point backups to:
- local folder
- Google Drive synced folder
- OneDrive synced folder
- Dropbox synced folder

Do not tightly couple backup logic to a single cloud provider API.
Design cloud-ready backup file handling.

==================================================
13. REPORTING REQUIREMENTS
==================================================

The app should generate useful reports and insights:
- net monthly balance
- net yearly balance
- income trend
- expense trend
- expense by category
- top expense categories
- income by company/client
- project profitability
- unpaid vs paid project amounts
- savings progress

Make the reporting UI professional and visually strong.

==================================================
14. CODE QUALITY RULES
==================================================

Strictly follow these rules:
- clean code
- meaningful names
- SOLID where appropriate
- avoid premature abstraction
- avoid duplication
- avoid magic strings
- centralize constants where useful
- no ugly hacks
- no lazy shortcuts
- no giant methods
- no giant ViewModels
- no giant XAML files without organization
- separate responsibilities properly
- comment only where useful
- do not over-comment obvious code

==================================================
15. DELIVERABLE STYLE
==================================================

I do NOT want only explanations.
I want you to actually generate the project.

You must:
1. Create the full solution structure
2. Create the core domain models
3. Create the EF Core infrastructure
4. Create the WPF shell
5. Create the design system resources
6. Create all major views and viewmodels
7. Implement the main CRUD flows
8. Implement authentication and lock flow
9. Implement backup and restore services
10. Implement premium UI styling
11. Add seed data
12. Add sample chart-ready reporting components
13. Add validation
14. Add tests for critical services
15. Make the project runnable

==================================================
16. OUTPUT EXECUTION STRATEGY
==================================================

Build this project in phases.
After each phase, clearly state:
- what was completed
- what files were created
- what remains next

Suggested phases:
Phase 1: solution architecture + project setup
Phase 2: domain + infrastructure + database
Phase 3: authentication + security + first run
Phase 4: application shell + design system + navigation
Phase 5: dashboard
Phase 6: companies and projects
Phase 7: income and expenses
Phase 8: deductions and savings
Phase 9: reports
Phase 10: backup and restore
Phase 11: polishing, validations, tests, seed, refinements

IMPORTANT:
Do not stop at only planning.
Actually generate the code and files progressively.

==================================================
17. IMPLEMENTATION PRIORITIES
==================================================

Prioritize:
1. correct architecture
2. security
3. data integrity
4. premium UI
5. maintainability
6. backups
7. good UX
8. reporting quality

==================================================
18. SPECIAL UI DIRECTION
==================================================

The visual style should feel like a mix of:
- premium finance software
- modern productivity desktop app
- executive dashboard tool

Use:
- refined dark theme
- elegant card surfaces
- muted premium accents
- modern typography hierarchy
- clean tables
- premium side navigation
- polished dashboard summaries

Avoid:
- childish gradients everywhere
- overly flashy colors
- cheap admin template feel
- outdated default Windows look
- cluttered dense forms
- amateur spacing

==================================================
19. UX DETAILS
==================================================

Include:
- empty states with guidance
- confirmation dialogs for destructive actions
- undo-friendly thinking where appropriate
- clear validation errors
- smart defaults
- recent data sections
- searchable lists where useful
- sortable tables where useful
- filters for reports and records
- polished onboarding for first run

==================================================
20. FINAL EXPECTATION
==================================================

Behave like an elite engineer and designer building a real premium application for a serious user.

Do not simplify this into a toy example.
Do not give only pseudo code.
Do not give only folder ideas.
Do not give only UI mock explanations.

Generate the actual implementation in a structured professional way.

Start now with:
- full solution architecture
- project structure
- all projects and references
- initial folder tree
- core design system setup
- domain model plan
- first implementation phase

Then continue phase by phase until the core system is fully scaffolded and implemented.

Additional strict instructions:

- Use professional naming everywhere.
- Prefer maintainable architecture over shortcuts.
- Make every screen visually polished.
- Build reusable components.
- Use a premium dark design system by default.
- Every financial write operation must be safe and validated.
- Avoid code duplication aggressively.
- Ensure all CRUD flows are production-minded.
- Seed realistic sample categories and starter data.
- Make the app feel like premium paid software.
- Keep the solution ready for future expansion.
- Show all created files and code progressively.
- If a decision is needed, choose the more professional long-term architecture.
- Never fallback to low-quality placeholder UI unless explicitly marked as temporary.
- Focus strongly on premium UX, security, and clean code.