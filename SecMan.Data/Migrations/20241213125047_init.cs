using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SecMan.Data.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Applications",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    InstalledDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Applications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DevDefs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    App = table.Column<bool>(type: "INTEGER", nullable: false),
                    Vers = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevDefs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "InitFileTypes",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Type = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Vers = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InitFileTypes", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    IsLoggedOutType = table.Column<bool>(type: "INTEGER", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "RSAKeys",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PrivateKey = table.Column<string>(type: "TEXT", nullable: true),
                    PublicKey = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RSAKeys", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SuperUsers",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SuperUsers", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SysFeats",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Common = table.Column<bool>(type: "INTEGER", nullable: false),
                    TestConnection = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysFeats", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Domain = table.Column<string>(type: "TEXT", nullable: true),
                    UserName = table.Column<string>(type: "TEXT", nullable: true),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    PasswordDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ChangePassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordExpiryEnable = table.Column<bool>(type: "INTEGER", nullable: false),
                    PasswordExpiryDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LastLoginDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RFI = table.Column<string>(type: "TEXT", nullable: true),
                    RFIDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Biometric = table.Column<string>(type: "TEXT", nullable: true),
                    BiometricDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FirstName = table.Column<string>(type: "TEXT", nullable: true),
                    LastName = table.Column<string>(type: "TEXT", nullable: true),
                    Description = table.Column<string>(type: "TEXT", nullable: true),
                    Language = table.Column<string>(type: "TEXT", nullable: true),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Enabled = table.Column<bool>(type: "INTEGER", nullable: false),
                    EnabledDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Retired = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetiredDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Locked = table.Column<bool>(type: "INTEGER", nullable: false),
                    LockedReason = table.Column<string>(type: "TEXT", nullable: true),
                    LockedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    LegacySupport = table.Column<bool>(type: "INTEGER", nullable: false),
                    LastLogoutDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    IsLegacy = table.Column<bool>(type: "INTEGER", nullable: false),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    InActiveDate = table.Column<DateTime>(type: "TEXT", nullable: true),
                    ResetPassword = table.Column<bool>(type: "INTEGER", nullable: false),
                    FirstLogin = table.Column<bool>(type: "INTEGER", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    SessionId = table.Column<string>(type: "TEXT", nullable: true),
                    SessionExpiry = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ResetPasswordToken = table.Column<string>(type: "TEXT", nullable: true),
                    ResetPasswordTokenExpiry = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Zones",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Zones", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "DevDefLangs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevDefLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevDefLangs_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevPermDefs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vers = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Posn = table.Column<int>(type: "INTEGER", nullable: false),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPermDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPermDefs_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevPolDefs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vers = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Posn = table.Column<int>(type: "INTEGER", nullable: false),
                    ValType = table.Column<string>(type: "TEXT", nullable: true),
                    ValMin = table.Column<double>(type: "REAL", nullable: false),
                    ValMax = table.Column<double>(type: "REAL", nullable: false),
                    ValDflt = table.Column<string>(type: "TEXT", nullable: true),
                    Units = table.Column<string>(type: "TEXT", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPolDefs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPolDefs_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SysFeatLangs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    SysFeatId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysFeatLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysFeatLangs_SysFeats_SysFeatId",
                        column: x => x.SysFeatId,
                        principalTable: "SysFeats",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SysFeatProps",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Vers = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Units = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Posn = table.Column<int>(type: "INTEGER", nullable: false),
                    ValType = table.Column<string>(type: "TEXT", nullable: true),
                    ValMin = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ValMax = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Val = table.Column<string>(type: "TEXT", nullable: true),
                    SysFeatId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysFeatProps", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysFeatProps_SysFeats_SysFeatId",
                        column: x => x.SysFeatId,
                        principalTable: "SysFeats",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "EventLogs",
                columns: table => new
                {
                    EventLogId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EventType = table.Column<string>(type: "TEXT", nullable: true),
                    EventSubType = table.Column<string>(type: "TEXT", nullable: true),
                    Severity = table.Column<int>(type: "INTEGER", nullable: false),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    SigningUserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    AuthorizingUserId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Message = table.Column<string>(type: "TEXT", nullable: true),
                    Source = table.Column<string>(type: "TEXT", nullable: true),
                    Date = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EventLogs", x => x.EventLogId);
                    table.ForeignKey(
                        name: "FK_EventLogs_Users_AuthorizingUserId",
                        column: x => x.AuthorizingUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventLogs_Users_SigningUserId",
                        column: x => x.SigningUserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_EventLogs_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "PasswordHistories",
                columns: table => new
                {
                    EntryId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    UserId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    Password = table.Column<string>(type: "TEXT", nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PasswordHistories", x => x.EntryId);
                    table.ForeignKey(
                        name: "FK_PasswordHistories_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RoleUser",
                columns: table => new
                {
                    RolesId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UsersId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleUser", x => new { x.RolesId, x.UsersId });
                    table.ForeignKey(
                        name: "FK_RoleUser_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleUser_Users_UsersId",
                        column: x => x.UsersId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Devs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    ZoneId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Vers = table.Column<string>(type: "TEXT", nullable: true),
                    Legacy = table.Column<bool>(type: "INTEGER", nullable: false),
                    SysPolVer = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DevPolVer = table.Column<ulong>(type: "INTEGER", nullable: false),
                    DevPermVer = table.Column<ulong>(type: "INTEGER", nullable: false),
                    UserVer = table.Column<ulong>(type: "INTEGER", nullable: false),
                    RoleVer = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ConnRate = table.Column<int>(type: "INTEGER", nullable: false),
                    ConnState = table.Column<int>(type: "INTEGER", nullable: false),
                    LastConnDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Ip = table.Column<string>(type: "TEXT", nullable: true),
                    DeploymentStatus = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Devs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Devs_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Devs_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RoleZone",
                columns: table => new
                {
                    RolesId = table.Column<ulong>(type: "INTEGER", nullable: false),
                    ZonesId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RoleZone", x => new { x.RolesId, x.ZonesId });
                    table.ForeignKey(
                        name: "FK_RoleZone_Roles_RolesId",
                        column: x => x.RolesId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RoleZone_Zones_ZonesId",
                        column: x => x.ZonesId,
                        principalTable: "Zones",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DevPermDefLangs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    DevPermDefId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPermDefLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPermDefLangs_DevPermDefs_DevPermDefId",
                        column: x => x.DevPermDefId,
                        principalTable: "DevPermDefs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevPermVals",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZoneId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevPermDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Val = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPermVals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPermVals_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevPermVals_DevPermDefs_DevPermDefId",
                        column: x => x.DevPermDefId,
                        principalTable: "DevPermDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevPermVals_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevPermVals_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevSigVals",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZoneId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    RoleId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevPermDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Sign = table.Column<bool>(type: "INTEGER", nullable: false),
                    Auth = table.Column<bool>(type: "INTEGER", nullable: false),
                    Note = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevSigVals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevSigVals_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevSigVals_DevPermDefs_DevPermDefId",
                        column: x => x.DevPermDefId,
                        principalTable: "DevPermDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevSigVals_Roles_RoleId",
                        column: x => x.RoleId,
                        principalTable: "Roles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevSigVals_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevPolDefLangs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Units = table.Column<string>(type: "TEXT", nullable: true),
                    DevPolDefId = table.Column<ulong>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPolDefLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPolDefLangs_DevPolDefs_DevPolDefId",
                        column: x => x.DevPolDefId,
                        principalTable: "DevPolDefs",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "DevPolVals",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    ZoneId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    DevPolDefId = table.Column<ulong>(type: "INTEGER", nullable: true),
                    Val = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DevPolVals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DevPolVals_DevDefs_DevDefId",
                        column: x => x.DevDefId,
                        principalTable: "DevDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevPolVals_DevPolDefs_DevPolDefId",
                        column: x => x.DevPolDefId,
                        principalTable: "DevPolDefs",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DevPolVals_Zones_ZoneId",
                        column: x => x.ZoneId,
                        principalTable: "Zones",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "SysFeatPropLangs",
                columns: table => new
                {
                    Id = table.Column<ulong>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Code = table.Column<string>(type: "TEXT", nullable: true),
                    Name = table.Column<string>(type: "TEXT", nullable: true),
                    Desc = table.Column<string>(type: "TEXT", nullable: true),
                    Cat = table.Column<string>(type: "TEXT", nullable: true),
                    Units = table.Column<string>(type: "TEXT", nullable: true),
                    SysFeatPropId = table.Column<ulong>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SysFeatPropLangs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SysFeatPropLangs_SysFeatProps_SysFeatPropId",
                        column: x => x.SysFeatPropId,
                        principalTable: "SysFeatProps",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_DevDefLangs_DevDefId",
                table: "DevDefLangs",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermDefLangs_DevPermDefId",
                table: "DevPermDefLangs",
                column: "DevPermDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermDefs_DevDefId",
                table: "DevPermDefs",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermVals_DevDefId",
                table: "DevPermVals",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermVals_DevPermDefId",
                table: "DevPermVals",
                column: "DevPermDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermVals_RoleId",
                table: "DevPermVals",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPermVals_ZoneId",
                table: "DevPermVals",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPolDefLangs_DevPolDefId",
                table: "DevPolDefLangs",
                column: "DevPolDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPolDefs_DevDefId",
                table: "DevPolDefs",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPolVals_DevDefId",
                table: "DevPolVals",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPolVals_DevPolDefId",
                table: "DevPolVals",
                column: "DevPolDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevPolVals_ZoneId",
                table: "DevPolVals",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_Devs_DevDefId",
                table: "Devs",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_Devs_ZoneId",
                table: "Devs",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_DevSigVals_DevDefId",
                table: "DevSigVals",
                column: "DevDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevSigVals_DevPermDefId",
                table: "DevSigVals",
                column: "DevPermDefId");

            migrationBuilder.CreateIndex(
                name: "IX_DevSigVals_RoleId",
                table: "DevSigVals",
                column: "RoleId");

            migrationBuilder.CreateIndex(
                name: "IX_DevSigVals_ZoneId",
                table: "DevSigVals",
                column: "ZoneId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_AuthorizingUserId",
                table: "EventLogs",
                column: "AuthorizingUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_SigningUserId",
                table: "EventLogs",
                column: "SigningUserId");

            migrationBuilder.CreateIndex(
                name: "IX_EventLogs_UserId",
                table: "EventLogs",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_PasswordHistories_UserId",
                table: "PasswordHistories",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleUser_UsersId",
                table: "RoleUser",
                column: "UsersId");

            migrationBuilder.CreateIndex(
                name: "IX_RoleZone_ZonesId",
                table: "RoleZone",
                column: "ZonesId");

            migrationBuilder.CreateIndex(
                name: "IX_SysFeatLangs_SysFeatId",
                table: "SysFeatLangs",
                column: "SysFeatId");

            migrationBuilder.CreateIndex(
                name: "IX_SysFeatPropLangs_SysFeatPropId",
                table: "SysFeatPropLangs",
                column: "SysFeatPropId");

            migrationBuilder.CreateIndex(
                name: "IX_SysFeatProps_SysFeatId",
                table: "SysFeatProps",
                column: "SysFeatId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Applications");

            migrationBuilder.DropTable(
                name: "DevDefLangs");

            migrationBuilder.DropTable(
                name: "DevPermDefLangs");

            migrationBuilder.DropTable(
                name: "DevPermVals");

            migrationBuilder.DropTable(
                name: "DevPolDefLangs");

            migrationBuilder.DropTable(
                name: "DevPolVals");

            migrationBuilder.DropTable(
                name: "Devs");

            migrationBuilder.DropTable(
                name: "DevSigVals");

            migrationBuilder.DropTable(
                name: "EventLogs");

            migrationBuilder.DropTable(
                name: "InitFileTypes");

            migrationBuilder.DropTable(
                name: "PasswordHistories");

            migrationBuilder.DropTable(
                name: "RoleUser");

            migrationBuilder.DropTable(
                name: "RoleZone");

            migrationBuilder.DropTable(
                name: "RSAKeys");

            migrationBuilder.DropTable(
                name: "SuperUsers");

            migrationBuilder.DropTable(
                name: "SysFeatLangs");

            migrationBuilder.DropTable(
                name: "SysFeatPropLangs");

            migrationBuilder.DropTable(
                name: "DevPolDefs");

            migrationBuilder.DropTable(
                name: "DevPermDefs");

            migrationBuilder.DropTable(
                name: "Users");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Zones");

            migrationBuilder.DropTable(
                name: "SysFeatProps");

            migrationBuilder.DropTable(
                name: "DevDefs");

            migrationBuilder.DropTable(
                name: "SysFeats");
        }
    }
}
