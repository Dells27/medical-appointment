using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PatientService.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddDocumentUploader : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "appointment_id",
                table: "medical_documents",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "description",
                table: "medical_documents",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uploaded_by_name",
                table: "medical_documents",
                type: "character varying(255)",
                maxLength: 255,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "uploaded_by_role",
                table: "medical_documents",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "Patient");

            migrationBuilder.AddColumn<Guid>(
                name: "uploaded_by_user_id",
                table: "medical_documents",
                type: "uuid",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "appointment_id",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "description",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "uploaded_by_name",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "uploaded_by_role",
                table: "medical_documents");

            migrationBuilder.DropColumn(
                name: "uploaded_by_user_id",
                table: "medical_documents");
        }
    }
}
