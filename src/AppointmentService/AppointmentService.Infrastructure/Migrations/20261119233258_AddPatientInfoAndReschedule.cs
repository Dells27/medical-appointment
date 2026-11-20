using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AppointmentService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddPatientInfoAndReschedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "cancelled_by_role",
                table: "appointments",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_email",
                table: "appointments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "patient_name",
                table: "appointments",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "rescheduled_at",
                table: "appointments",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_appointments_doctor_id_patient_id",
                table: "appointments",
                columns: new[] { "doctor_id", "patient_id" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_appointments_doctor_id_patient_id",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "cancelled_by_role",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "patient_email",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "patient_name",
                table: "appointments");

            migrationBuilder.DropColumn(
                name: "rescheduled_at",
                table: "appointments");
        }
    }
}
