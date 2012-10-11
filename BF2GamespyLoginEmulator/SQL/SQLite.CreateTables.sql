CREATE TABLE "main"."accounts" (
	"id"  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name"  TEXT(20) NOT NULL UNIQUE,
	"password"  TEXT(30) NOT NULL,
	"email"  TEXT(50) NOT NULL,
	"country"  TEXT(4) NOT NULL,
	"session"  INTEGER DEFAULT 0
);

CREATE TABLE "main"."bf2pids" (
	"pid"  INTEGER PRIMARY KEY NOT NULL,
	"nick"  TEXT(20) NOT NULL UNIQUE
);