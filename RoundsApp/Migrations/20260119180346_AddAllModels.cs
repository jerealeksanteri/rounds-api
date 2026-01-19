using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace RoundsApp.Migrations
{
    /// <inheritdoc />
    public partial class AddAllModels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkingSessions_Users_CreatedById",
                table: "DrinkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkingSessions_Users_UpdatedById",
                table: "DrinkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Drinks_Users_CreatedById",
                table: "Drinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Drinks_Users_UpdatedById",
                table: "Drinks");

            migrationBuilder.DropTable(
                name: "DrinkingSessionImages");

            migrationBuilder.DropTable(
                name: "DrinkingSessionParticipationDrinks");

            migrationBuilder.DropTable(
                name: "DrinkingSessionParticipations");

            migrationBuilder.DropColumn(
                name: "Producer",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "Title",
                table: "DrinkingSessions");

            migrationBuilder.RenameColumn(
                name: "ScheduledAt",
                table: "DrinkingSessions",
                newName: "StartsAt");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Drinks",
                type: "text",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Drinks",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(500)",
                oldMaxLength: 500,
                oldNullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DrinkTypeId",
                table: "Drinks",
                type: "uuid",
                nullable: false,
                defaultValue: Guid.Empty);

            migrationBuilder.AddColumn<decimal>(
                name: "VolumeLitres",
                table: "Drinks",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DrinkingSessions",
                type: "text",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(1000)",
                oldMaxLength: 1000,
                oldNullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                table: "DrinkingSessions",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "LocationId",
                table: "DrinkingSessions",
                type: "uuid",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Name",
                table: "DrinkingSessions",
                type: "text",
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateTable(
                name: "Achievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: true),
                    Criteria = table.Column<string>(type: "jsonb", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Achievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Achievements_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrinkImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrinkImages_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkImages_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrinkImages_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DrinkTypes",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkTypes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrinkTypes_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_DrinkTypes_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Friendships",
                columns: table => new
                {
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    FriendId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Friendships", x => new { x.UserId, x.FriendId });
                    table.ForeignKey(
                        name: "FK_Friendships_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_FriendId",
                        column: x => x.FriendId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Friendships_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Notifications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Type = table.Column<string>(type: "text", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Message = table.Column<string>(type: "text", nullable: false),
                    Metadata = table.Column<string>(type: "jsonb", nullable: true),
                    Read = table.Column<bool>(type: "boolean", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Notifications", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Notifications_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionComments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Content = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionComments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionComments_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionComments_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionComments_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionComments_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Url = table.Column<string>(type: "text", nullable: false),
                    Caption = table.Column<string>(type: "text", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionImages_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionImages_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionImages_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionInvites",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Status = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionInvites", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionInvites_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionInvites_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionInvites_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionInvites_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionLocations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Address = table.Column<string>(type: "text", nullable: true),
                    Latitude = table.Column<decimal>(type: "numeric", nullable: true),
                    Longitude = table.Column<decimal>(type: "numeric", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionLocations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionLocations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionLocations_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionParticipants",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionParticipants", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_SessionParticipants_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionTags",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    Tag = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionTags", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionTags_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionTags_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserDrinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    Quantity = table.Column<int>(type: "integer", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserDrinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserDrinks_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDrinks_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserDrinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDrinks_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserDrinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "UserFavouriteDrinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserFavouriteDrinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserFavouriteDrinks_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserFavouriteDrinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_UserFavouriteDrinks_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "SessionAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    SessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SessionAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SessionAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SessionAchievements_DrinkingSessions_SessionId",
                        column: x => x.SessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "UserAchievements",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    AchievementId = table.Column<Guid>(type: "uuid", nullable: false),
                    UnlockedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UserAchievements", x => x.Id);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Achievements_AchievementId",
                        column: x => x.AchievementId,
                        principalTable: "Achievements",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_UserAchievements_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Drinks_DrinkTypeId",
                table: "Drinks",
                column: "DrinkTypeId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessions_LocationId",
                table: "DrinkingSessions",
                column: "LocationId");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_CreatedById",
                table: "Achievements",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Achievements_UpdatedById",
                table: "Achievements",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkImages_CreatedById",
                table: "DrinkImages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkImages_DrinkId",
                table: "DrinkImages",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkImages_UpdatedById",
                table: "DrinkImages",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkTypes_CreatedById",
                table: "DrinkTypes",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkTypes_UpdatedById",
                table: "DrinkTypes",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_CreatedById",
                table: "Friendships",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_FriendId",
                table: "Friendships",
                column: "FriendId");

            migrationBuilder.CreateIndex(
                name: "IX_Friendships_UpdatedById",
                table: "Friendships",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_Notifications_UserId",
                table: "Notifications",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAchievements_AchievementId",
                table: "SessionAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionAchievements_SessionId",
                table: "SessionAchievements",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_CreatedById",
                table: "SessionComments",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_SessionId",
                table: "SessionComments",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_UpdatedById",
                table: "SessionComments",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionComments_UserId",
                table: "SessionComments",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionImages_CreatedById",
                table: "SessionImages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionImages_SessionId",
                table: "SessionImages",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionImages_UpdatedById",
                table: "SessionImages",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInvites_CreatedById",
                table: "SessionInvites",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInvites_SessionId",
                table: "SessionInvites",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInvites_UpdatedById",
                table: "SessionInvites",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionInvites_UserId",
                table: "SessionInvites",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionLocations_CreatedById",
                table: "SessionLocations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionLocations_UpdatedById",
                table: "SessionLocations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_CreatedById",
                table: "SessionParticipants",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_SessionId",
                table: "SessionParticipants",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_UpdatedById",
                table: "SessionParticipants",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionParticipants_UserId",
                table: "SessionParticipants",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTags_CreatedById",
                table: "SessionTags",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_SessionTags_SessionId",
                table: "SessionTags",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_AchievementId",
                table: "UserAchievements",
                column: "AchievementId");

            migrationBuilder.CreateIndex(
                name: "IX_UserAchievements_UserId",
                table: "UserAchievements",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrinks_CreatedById",
                table: "UserDrinks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrinks_DrinkId",
                table: "UserDrinks",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrinks_SessionId",
                table: "UserDrinks",
                column: "SessionId");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrinks_UpdatedById",
                table: "UserDrinks",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserDrinks_UserId",
                table: "UserDrinks",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteDrinks_CreatedById",
                table: "UserFavouriteDrinks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteDrinks_DrinkId",
                table: "UserFavouriteDrinks",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_UserFavouriteDrinks_UserId",
                table: "UserFavouriteDrinks",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkingSessions_SessionLocations_LocationId",
                table: "DrinkingSessions",
                column: "LocationId",
                principalTable: "SessionLocations",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkingSessions_Users_CreatedById",
                table: "DrinkingSessions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkingSessions_Users_UpdatedById",
                table: "DrinkingSessions",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Drinks_DrinkTypes_DrinkTypeId",
                table: "Drinks",
                column: "DrinkTypeId",
                principalTable: "DrinkTypes",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drinks_Users_CreatedById",
                table: "Drinks",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Drinks_Users_UpdatedById",
                table: "Drinks",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DrinkingSessions_SessionLocations_LocationId",
                table: "DrinkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkingSessions_Users_CreatedById",
                table: "DrinkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_DrinkingSessions_Users_UpdatedById",
                table: "DrinkingSessions");

            migrationBuilder.DropForeignKey(
                name: "FK_Drinks_DrinkTypes_DrinkTypeId",
                table: "Drinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Drinks_Users_CreatedById",
                table: "Drinks");

            migrationBuilder.DropForeignKey(
                name: "FK_Drinks_Users_UpdatedById",
                table: "Drinks");

            migrationBuilder.DropTable(
                name: "DrinkImages");

            migrationBuilder.DropTable(
                name: "DrinkTypes");

            migrationBuilder.DropTable(
                name: "Friendships");

            migrationBuilder.DropTable(
                name: "Notifications");

            migrationBuilder.DropTable(
                name: "SessionAchievements");

            migrationBuilder.DropTable(
                name: "SessionComments");

            migrationBuilder.DropTable(
                name: "SessionImages");

            migrationBuilder.DropTable(
                name: "SessionInvites");

            migrationBuilder.DropTable(
                name: "SessionLocations");

            migrationBuilder.DropTable(
                name: "SessionParticipants");

            migrationBuilder.DropTable(
                name: "SessionTags");

            migrationBuilder.DropTable(
                name: "UserAchievements");

            migrationBuilder.DropTable(
                name: "UserDrinks");

            migrationBuilder.DropTable(
                name: "UserFavouriteDrinks");

            migrationBuilder.DropTable(
                name: "Achievements");

            migrationBuilder.DropIndex(
                name: "IX_Drinks_DrinkTypeId",
                table: "Drinks");

            migrationBuilder.DropIndex(
                name: "IX_DrinkingSessions_LocationId",
                table: "DrinkingSessions");

            migrationBuilder.DropColumn(
                name: "DrinkTypeId",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "VolumeLitres",
                table: "Drinks");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "DrinkingSessions");

            migrationBuilder.DropColumn(
                name: "LocationId",
                table: "DrinkingSessions");

            migrationBuilder.DropColumn(
                name: "Name",
                table: "DrinkingSessions");

            migrationBuilder.RenameColumn(
                name: "StartsAt",
                table: "DrinkingSessions",
                newName: "ScheduledAt");

            migrationBuilder.AlterColumn<string>(
                name: "Name",
                table: "Drinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "text");

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "Drinks",
                type: "character varying(500)",
                maxLength: 500,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Producer",
                table: "Drinks",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Drinks",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AlterColumn<string>(
                name: "Description",
                table: "DrinkingSessions",
                type: "character varying(1000)",
                maxLength: 1000,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "text",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Title",
                table: "DrinkingSessions",
                type: "character varying(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: string.Empty);

            migrationBuilder.CreateTable(
                name: "DrinkingSessionImages",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ImageData = table.Column<byte[]>(type: "bytea", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkingSessionImages", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionImages_DrinkingSessions_DrinkingSessionId",
                        column: x => x.DrinkingSessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionImages_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionImages_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DrinkingSessionParticipations",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkingSessionId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    JoinedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkingSessionParticipations", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipations_DrinkingSessions_DrinkingSess~",
                        column: x => x.DrinkingSessionId,
                        principalTable: "DrinkingSessions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipations_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipations_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipations_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DrinkingSessionParticipationDrinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    CreatedById = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkId = table.Column<Guid>(type: "uuid", nullable: false),
                    DrinkingSessionParticipationId = table.Column<Guid>(type: "uuid", nullable: false),
                    UpdatedById = table.Column<Guid>(type: "uuid", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DrinkingSessionParticipationDrinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipationDrinks_DrinkingSessionParticipa~",
                        column: x => x.DrinkingSessionParticipationId,
                        principalTable: "DrinkingSessionParticipations",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipationDrinks_Drinks_DrinkId",
                        column: x => x.DrinkId,
                        principalTable: "Drinks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipationDrinks_Users_CreatedById",
                        column: x => x.CreatedById,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DrinkingSessionParticipationDrinks_Users_UpdatedById",
                        column: x => x.UpdatedById,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionImages_CreatedById",
                table: "DrinkingSessionImages",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionImages_DrinkingSessionId",
                table: "DrinkingSessionImages",
                column: "DrinkingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionImages_UpdatedById",
                table: "DrinkingSessionImages",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipationDrinks_CreatedById",
                table: "DrinkingSessionParticipationDrinks",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipationDrinks_DrinkId",
                table: "DrinkingSessionParticipationDrinks",
                column: "DrinkId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipationDrinks_DrinkingSessionParticipa~",
                table: "DrinkingSessionParticipationDrinks",
                column: "DrinkingSessionParticipationId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipationDrinks_UpdatedById",
                table: "DrinkingSessionParticipationDrinks",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipations_CreatedById",
                table: "DrinkingSessionParticipations",
                column: "CreatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipations_DrinkingSessionId",
                table: "DrinkingSessionParticipations",
                column: "DrinkingSessionId");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipations_UpdatedById",
                table: "DrinkingSessionParticipations",
                column: "UpdatedById");

            migrationBuilder.CreateIndex(
                name: "IX_DrinkingSessionParticipations_UserId",
                table: "DrinkingSessionParticipations",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkingSessions_Users_CreatedById",
                table: "DrinkingSessions",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DrinkingSessions_Users_UpdatedById",
                table: "DrinkingSessions",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Drinks_Users_CreatedById",
                table: "Drinks",
                column: "CreatedById",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Drinks_Users_UpdatedById",
                table: "Drinks",
                column: "UpdatedById",
                principalTable: "Users",
                principalColumn: "Id");
        }
    }
}
