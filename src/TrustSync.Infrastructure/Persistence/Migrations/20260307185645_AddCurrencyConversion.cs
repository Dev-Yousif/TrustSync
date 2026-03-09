using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TrustSync.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class AddCurrencyConversion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ConvertedCurrencyCode",
                table: "SavingGoals",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedTargetAmount",
                table: "SavingGoals",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "SavingGoals",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedAmount",
                table: "SavingEntries",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConvertedCurrencyCode",
                table: "SavingEntries",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "SavingEntries",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedAmount",
                table: "Incomes",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConvertedCurrencyCode",
                table: "Incomes",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "Incomes",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedAmount",
                table: "Expenses",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConvertedCurrencyCode",
                table: "Expenses",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "Expenses",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.AddColumn<decimal>(
                name: "ConvertedAmount",
                table: "Deductions",
                type: "decimal(18,2)",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AddColumn<string>(
                name: "ConvertedCurrencyCode",
                table: "Deductions",
                type: "TEXT",
                maxLength: 3,
                nullable: false,
                defaultValue: "USD");

            migrationBuilder.AddColumn<decimal>(
                name: "ExchangeRateUsed",
                table: "Deductions",
                type: "decimal(18,6)",
                nullable: false,
                defaultValue: 1m);

            migrationBuilder.CreateTable(
                name: "ExchangeRates",
                columns: table => new
                {
                    CurrencyCode = table.Column<string>(type: "TEXT", maxLength: 3, nullable: false),
                    RateToUsd = table.Column<decimal>(type: "decimal(18,6)", nullable: false),
                    FetchedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ExchangeRates", x => x.CurrencyCode);
                });

            // Backfill existing records: set ConvertedAmount = Amount, ConvertedCurrencyCode = CurrencyCode
            migrationBuilder.Sql("UPDATE Incomes SET ConvertedAmount = Amount, ConvertedCurrencyCode = CurrencyCode, ExchangeRateUsed = 1.0;");
            migrationBuilder.Sql("UPDATE Expenses SET ConvertedAmount = Amount, ConvertedCurrencyCode = CurrencyCode, ExchangeRateUsed = 1.0;");
            migrationBuilder.Sql("UPDATE Deductions SET ConvertedAmount = Amount, ConvertedCurrencyCode = CurrencyCode, ExchangeRateUsed = 1.0;");
            migrationBuilder.Sql("UPDATE SavingGoals SET ConvertedTargetAmount = TargetAmount, ConvertedCurrencyCode = CurrencyCode, ExchangeRateUsed = 1.0;");
            migrationBuilder.Sql("UPDATE SavingEntries SET ConvertedAmount = Amount, ConvertedCurrencyCode = (SELECT sg.CurrencyCode FROM SavingGoals sg WHERE sg.Id = SavingEntries.SavingGoalId), ExchangeRateUsed = 1.0;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ExchangeRates");

            migrationBuilder.DropColumn(
                name: "ConvertedCurrencyCode",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "ConvertedTargetAmount",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "SavingGoals");

            migrationBuilder.DropColumn(
                name: "ConvertedAmount",
                table: "SavingEntries");

            migrationBuilder.DropColumn(
                name: "ConvertedCurrencyCode",
                table: "SavingEntries");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "SavingEntries");

            migrationBuilder.DropColumn(
                name: "ConvertedAmount",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "ConvertedCurrencyCode",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "Incomes");

            migrationBuilder.DropColumn(
                name: "ConvertedAmount",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ConvertedCurrencyCode",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "Expenses");

            migrationBuilder.DropColumn(
                name: "ConvertedAmount",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "ConvertedCurrencyCode",
                table: "Deductions");

            migrationBuilder.DropColumn(
                name: "ExchangeRateUsed",
                table: "Deductions");
        }
    }
}
