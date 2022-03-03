using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Social_Media_API.Migrations
{
    public partial class CreateInitial : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Following",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    followingID = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    image = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Following", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Messages",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    fromID = table.Column<int>(type: "int", nullable: false),
                    toID = table.Column<int>(type: "int", nullable: false),
                    toName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    toImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fromName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    fromImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    content = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Messages", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Posts",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    userID = table.Column<int>(type: "int", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    tag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userImage = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageFile = table.Column<byte[]>(type: "varbinary(max)", nullable: false),
                    text = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    date = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Posts", x => x.id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    username = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    password = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    email = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    name = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    userTag = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageURL = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    imageURLBG = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    bio = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    livesIn = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    relationship = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    joined = table.Column<DateTime>(type: "datetime2", nullable: false),
                    privateAccount = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.id);
                });
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Following");

            migrationBuilder.DropTable(
                name: "Messages");

            migrationBuilder.DropTable(
                name: "Posts");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
