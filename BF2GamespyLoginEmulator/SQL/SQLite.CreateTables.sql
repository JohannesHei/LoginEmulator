CREATE TABLE "main"."users" (
	"id"  INTEGER PRIMARY KEY AUTOINCREMENT NOT NULL,
	"name"  TEXT(20) NOT NULL,
	"password"  TEXT(30) NOT NULL,
	"email"  TEXT(50) NOT NULL,
	"country"  TEXT(4) NOT NULL,
	"session"  INTEGER DEFAULT 0
);