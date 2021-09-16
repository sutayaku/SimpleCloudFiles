using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace SimpleCloudFiles.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CfFiles",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: true),
                    DirId = table.Column<string>(type: "TEXT", nullable: true),
                    SourceMd5 = table.Column<string>(type: "TEXT", nullable: true),
                    Md5 = table.Column<string>(type: "TEXT", nullable: true),
                    Size = table.Column<long>(type: "INTEGER", nullable: false),
                    Ext = table.Column<string>(type: "TEXT", nullable: true),
                    FileName = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CfFiles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Dirs",
                columns: table => new
                {
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    AccountId = table.Column<string>(type: "TEXT", nullable: true),
                    DirId = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreateTime = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Dirs", x => x.Id);
                });

            migrationBuilder.InsertData(
                table: "Accounts",
                columns: new[] { "Id", "CreateTime", "Password", "UserName" },
                values: new object[] { "4548c99fa90b41e19283b046fff14ec9", new DateTime(2021, 9, 14, 23, 34, 49, 892, DateTimeKind.Local).AddTicks(7935), "E10ADC3949BA59ABBE56E057F20F883E", "admin" });

            migrationBuilder.InsertData(
                table: "Dirs",
                columns: new[] { "Id", "AccountId", "CreateTime", "DirId", "Name" },
                values: new object[] { "4329f1a476204af68493bba52d022728", "4548c99fa90b41e19283b046fff14ec9", new DateTime(2021, 9, 14, 23, 34, 49, 895, DateTimeKind.Local).AddTicks(2221), "", "" });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "CfFiles");

            migrationBuilder.DropTable(
                name: "Dirs");
        }
    }
}
