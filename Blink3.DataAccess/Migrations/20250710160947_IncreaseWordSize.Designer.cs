﻿// <auto-generated />
using System;
using Blink3.DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Blink3.DataAccess.Migrations
{
    [DbContext(typeof(BlinkDbContext))]
    [Migration("20250710160947_IncreaseWordSize")]
    partial class IncreaseWordSize
    {
        /// <inheritdoc />
        protected override void BuildTargetModel(ModelBuilder modelBuilder)
        {
#pragma warning disable 612, 618
            modelBuilder
                .HasAnnotation("ProductVersion", "9.0.6")
                .HasAnnotation("Relational:MaxIdentifierLength", 63);

            NpgsqlModelBuilderExtensions.UseIdentityByDefaultColumns(modelBuilder);

            modelBuilder.Entity("Blink3.Core.Entities.BlinkGuild", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("BackgroundColour")
                        .HasColumnType("text");

                    b.Property<string>("CorrectTileColour")
                        .HasColumnType("text");

                    b.Property<string>("IncorrectTileColour")
                        .HasColumnType("text");

                    b.Property<decimal?>("LoggingChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("MisplacedTileColour")
                        .HasColumnType("text");

                    b.Property<decimal?>("TemporaryVcCategoryId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("TextColour")
                        .HasColumnType("text");

                    b.HasKey("Id");

                    b.ToTable("BlinkGuilds");
                });

            modelBuilder.Entity("Blink3.Core.Entities.BlinkUser", b =>
                {
                    b.Property<decimal>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("BlinkUsers");
                });

            modelBuilder.Entity("Blink3.Core.Entities.GameStatistics", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("BlinkUserId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("CurrentStreak")
                        .HasColumnType("integer");

                    b.Property<int>("GamesPlayed")
                        .HasColumnType("integer");

                    b.Property<int>("GamesWon")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("LastActivity")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("MaxStreak")
                        .HasColumnType("integer");

                    b.Property<int>("Points")
                        .HasColumnType("integer");

                    b.Property<DateTime?>("ReminderSentAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<int>("Type")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("BlinkUserId");

                    b.ToTable("GameStatistics");
                });

            modelBuilder.Entity("Blink3.Core.Entities.TempVc", b =>
                {
                    b.Property<decimal>("GuildId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.PrimitiveCollection<decimal[]>("BannedUsers")
                        .IsRequired()
                        .HasColumnType("numeric(20,0)[]");

                    b.Property<bool>("CamOnly")
                        .HasColumnType("boolean");

                    b.Property<DateTime>("CreatedAt")
                        .HasColumnType("timestamp with time zone");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("GuildId", "ChannelId");

                    b.ToTable("TempVcs");
                });

            modelBuilder.Entity("Blink3.Core.Entities.UserTodo", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("Complete")
                        .HasColumnType("boolean");

                    b.Property<string>("Description")
                        .HasMaxLength(50)
                        .HasColumnType("character varying(50)");

                    b.Property<string>("Label")
                        .IsRequired()
                        .HasMaxLength(25)
                        .HasColumnType("character varying(25)");

                    b.Property<decimal>("UserId")
                        .HasColumnType("numeric(20,0)");

                    b.HasKey("Id");

                    b.ToTable("UserTodos");
                });

            modelBuilder.Entity("Blink3.Core.Entities.Word", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<bool>("IsSolution")
                        .HasColumnType("boolean");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasMaxLength(10)
                        .HasColumnType("character varying(10)");

                    b.Property<int>("Length")
                        .HasColumnType("integer");

                    b.Property<string>("Text")
                        .IsRequired()
                        .HasMaxLength(100)
                        .HasColumnType("character varying(100)");

                    b.HasKey("Id");

                    b.HasIndex("Text", "Language");

                    b.HasIndex("Language", "IsSolution", "Length");

                    b.ToTable("Words");
                });

            modelBuilder.Entity("Blink3.Core.Entities.Wordle", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("ChannelId")
                        .HasColumnType("numeric(20,0)");

                    b.Property<string>("Language")
                        .IsRequired()
                        .HasColumnType("text");

                    b.PrimitiveCollection<decimal[]>("Players")
                        .IsRequired()
                        .HasColumnType("numeric(20,0)[]");

                    b.Property<string>("WordToGuess")
                        .IsRequired()
                        .HasMaxLength(8)
                        .HasColumnType("character varying(8)");

                    b.HasKey("Id");

                    b.ToTable("Wordles");
                });

            modelBuilder.Entity("Blink3.Core.Entities.WordleGuess", b =>
                {
                    b.Property<int>("Id")
                        .ValueGeneratedOnAdd()
                        .HasColumnType("integer");

                    NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b.Property<int>("Id"));

                    b.Property<decimal>("GuessedById")
                        .HasColumnType("numeric(20,0)");

                    b.Property<int>("WordleId")
                        .HasColumnType("integer");

                    b.HasKey("Id");

                    b.HasIndex("WordleId");

                    b.ToTable("WordleGuesses");
                });

            modelBuilder.Entity("Blink3.Core.Entities.GameStatistics", b =>
                {
                    b.HasOne("Blink3.Core.Entities.BlinkUser", "BlinkUser")
                        .WithMany("GameStatistics")
                        .HasForeignKey("BlinkUserId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.Navigation("BlinkUser");
                });

            modelBuilder.Entity("Blink3.Core.Entities.WordleGuess", b =>
                {
                    b.HasOne("Blink3.Core.Entities.Wordle", "Wordle")
                        .WithMany("Guesses")
                        .HasForeignKey("WordleId")
                        .OnDelete(DeleteBehavior.Cascade)
                        .IsRequired();

                    b.OwnsMany("Blink3.Core.Entities.WordleLetter", "Letters", b1 =>
                        {
                            b1.Property<int>("WordleGuessId")
                                .HasColumnType("integer");

                            b1.Property<int>("Id")
                                .ValueGeneratedOnAdd()
                                .HasColumnType("integer");

                            NpgsqlPropertyBuilderExtensions.UseIdentityByDefaultColumn(b1.Property<int>("Id"));

                            b1.Property<char>("Letter")
                                .HasColumnType("character(1)");

                            b1.Property<int>("Position")
                                .HasColumnType("integer");

                            b1.Property<int>("State")
                                .HasColumnType("integer");

                            b1.HasKey("WordleGuessId", "Id");

                            b1.ToTable("WordleLetter");

                            b1.WithOwner()
                                .HasForeignKey("WordleGuessId");
                        });

                    b.Navigation("Letters");

                    b.Navigation("Wordle");
                });

            modelBuilder.Entity("Blink3.Core.Entities.BlinkUser", b =>
                {
                    b.Navigation("GameStatistics");
                });

            modelBuilder.Entity("Blink3.Core.Entities.Wordle", b =>
                {
                    b.Navigation("Guesses");
                });
#pragma warning restore 612, 618
        }
    }
}
