using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MedicalRecordService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMedicalBackground : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "medical_backgrounds",
                columns: table => new
                {
                    id = table.Column<Guid>(type: "uuid", nullable: false),
                    patient_id = table.Column<Guid>(type: "uuid", nullable: false),
                    chronic_conditions = table.Column<string>(type: "text", nullable: true),
                    surgeries = table.Column<string>(type: "text", nullable: true),
                    current_medications = table.Column<string>(type: "text", nullable: true),
                    family_history = table.Column<string>(type: "text", nullable: true),
                    allergies = table.Column<string>(type: "text", nullable: true),
                    habits = table.Column<string>(type: "text", nullable: true),
                    notes = table.Column<string>(type: "text", nullable: true),
                    updated_by_doctor_id = table.Column<Guid>(type: "uuid", nullable: false),
                    created_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    updated_at = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_medical_backgrounds", x => x.id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_medical_records_patient_id",
                table: "medical_records",
                column: "patient_id");

            migrationBuilder.CreateIndex(
                name: "IX_medical_backgrounds_patient_id",
                table: "medical_backgrounds",
                column: "patient_id",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "medical_backgrounds");

            migrationBuilder.DropIndex(
                name: "IX_medical_records_patient_id",
                table: "medical_records");
        }
    }
}
