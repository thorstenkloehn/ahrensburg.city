SET standard_conforming_strings = OFF;
DROP TABLE IF EXISTS "public"."datenbank" CASCADE;
BEGIN;
CREATE TABLE "public"."datenbank" ( "ogc_fid" SERIAL, CONSTRAINT "datenbank_pk" PRIMARY KEY ("ogc_fid") );
SELECT AddGeometryColumn('public','datenbank','wkb_geometry',4326,'POINT',2);
CREATE INDEX "datenbank_wkb_geometry_geom_idx" ON "public"."datenbank" USING GIST ("wkb_geometry");
ALTER TABLE "public"."datenbank" ADD COLUMN "uuiid" VARCHAR(255);
ALTER TABLE "public"."datenbank" ADD COLUMN "name" VARCHAR(255);
ALTER TABLE "public"."datenbank" ADD COLUMN "popupcontent" VARCHAR;
ALTER TABLE "public"."datenbank" ADD COLUMN "amenity" VARCHAR;
INSERT INTO "public"."datenbank" ("wkb_geometry" , "uuiid", "name", "popupcontent", "amenity") VALUES ('0101000020E61000004BEA0E57127B244026487B1310D74A40', '{deb33c97-df51-435e-b15c-b8a4dd294194}', 'Schloss Ahrensburg', 'Schloss Ahrensburg', 'Schloss Ahrensburg');
COMMIT;
