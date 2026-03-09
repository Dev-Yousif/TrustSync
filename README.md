<p align="center">
  <img src="src/TrustSync.Desktop/Assets/logo-readme.svg" alt="TrustSync Logo" width="100" />
</p>

<h1 align="center">TrustSync</h1>

<p align="center">
  A premium, privacy-first desktop application for personal finance, freelance income, and project management.
</p>

<p align="center">
  <img src="https://img.shields.io/badge/.NET-8.0-512BD4?logo=dotnet" alt=".NET 8" />
  <img src="https://img.shields.io/badge/Avalonia-11.2-8B44AC?logo=data:image/png;base64," alt="Avalonia UI" />
  <img src="https://img.shields.io/badge/SQLite-Database-003B57?logo=sqlite" alt="SQLite" />
  <img src="https://img.shields.io/badge/License-MIT-green" alt="License" />
  <img src="https://img.shields.io/badge/Platform-Windows%20%7C%20Linux-blue" alt="Platform" />
</p>

---

## About

TrustSync is a cross-platform desktop accounting application built for freelancers, contractors, and professionals who work with multiple companies and income sources. All data is stored locally with strong security — no cloud, no subscriptions, no data sharing.

## Features

- **Dashboard** — Monthly KPIs, income vs expense charts, category breakdowns, recent transactions
- **Companies & Clients** — Track companies, clients, engagement types (full-time, freelance, contract)
- **Projects** — Link projects to clients, track status, agreed/received amounts, profitability
- **Income Tracking** — Salary, freelance, project payments, bonuses with multi-currency support
- **Expense Management** — Categorized expenses with custom categories, colors, and icons
- **Deductions** — Recurring obligations with start/end dates and recurrence schedules
- **Savings Goals** — Set targets, track progress, record contributions
- **Reminders** — Configurable notifications (daily, weekly, monthly, custom intervals)
- **Reports & PDF Export** — Monthly/yearly financial reports with PDF generation
- **Backup & Restore** — Manual backups with restore capability, configurable backup location
- **Currency Conversion** — Multi-currency support with automatic conversion
- **Security** — Password-protected with Argon2 hashing, auto-lock on inactivity, audit logging

## Tech Stack

| Layer | Technology |
|-------|-----------|
| **Language** | C# / .NET 8 |
| **UI Framework** | Avalonia UI 11.2 (cross-platform) |
| **Architecture** | Clean Architecture + MVVM |
| **Database** | SQLite via Entity Framework Core |
| **Charts** | ScottPlot |
| **Security** | Argon2id password hashing |
| **DI** | Microsoft.Extensions.DependencyInjection |
| **Testing** | xUnit + Moq + FluentAssertions |

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build & Run

```bash
# Clone the repository
git clone https://github.com/Dev-Yousif/TrustSync.git
cd TrustSync

# Build
dotnet build

# Run
dotnet run --project src/TrustSync.Desktop
```

### Run Tests

```bash
dotnet test
```

## Project Structure

```
src/
  TrustSync.Domain/          # Entities, enums, value objects (zero dependencies)
  TrustSync.Application/     # Service interfaces, DTOs, business rules
  TrustSync.Infrastructure/  # EF Core, database, service implementations
  TrustSync.Desktop/         # Avalonia UI, ViewModels, Views, themes

tests/
  TrustSync.Tests/           # Unit tests for core services
```

## Screenshots

*Coming soon*

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request. For major changes, please open an issue first to discuss what you would like to change.

1. Fork the repository
2. Create your feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## License

This project is licensed under the MIT License — see the [LICENSE](LICENSE) file for details.
