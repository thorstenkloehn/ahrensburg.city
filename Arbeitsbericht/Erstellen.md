## Vorausetzung
```
sudo snap install code --classic
sudo apt-get install postgresql-all
sudo apt install postgis postgresql-16-postgis-3 postgresql-16-postgis-3-scripts postgresql-16-pgvector
sudo apt-get install osm2pgsql osmosis
ssh thorsten@
cd $HOME
sudo -u postgres -i
createuser thorsten
createdb -E UTF8 -O thorsten thorsten
psql -d thorsten -c "CREATE EXTENSION postgis;" # Erweiterung hinzufügen
psql -d thorsten -c "CREATE EXTENSION hstore;" # Erweiterung hinzufügen
psql -d thorsten -c "CREATE EXTENSION vector;" # Erweiterung hinzufügen
psql -d thorsten -c "ALTER TABLE geometry_columns OWNER TO thorsten;" # Rechte setzen
psql -d thorsten -c "ALTER TABLE spatial_ref_sys OWNER TO thorsten;" # Rechte setzen
psql -d thorsten -c "\password thorsten"
exit # Ausloggen
cd $HOME
wget https://download.geofabrik.de/europe/germany/schleswig-holstein-latest.osm.pbf
osmosis --read-pbf file=schleswig-holstein-latest.osm.pbf --bounding-box left=10.1141 right=10.3716 top=53.7136 bottom=53.6249 --write-pbf file=ahrensburg.pbf

osm2pgsql  -d thorsten --create  -G --hstore  ahrensburg.pbf

```
## Genau Beschreibung

Die folgenden Befehle werden in einer Linux-Umgebung ausgeführt, um verschiedene Softwarepakete zu installieren, einen PostgreSQL-Benutzer und eine Datenbank zu erstellen, PostGIS-, hstore- und vector-Erweiterungen zu aktivieren, OpenStreetMap-Daten herunterzuladen und in die PostgreSQL-Datenbank zu importieren.

**1. Installation von Software:**

* `sudo snap install code --classic`: Installiert Visual Studio Code (eine Code-Editor-Anwendung) über Snap. Das Flag `--classic` erlaubt der Anwendung erweiterten Zugriff auf das System.
* `sudo apt-get install postgresql-all`: Installiert alle verfügbaren PostgreSQL-Serverpakete.
* `sudo apt install postgis postgresql-16-postgis-3 postgresql-16-postgis-3-scripts postgresql-16-pgvector`: Installiert PostGIS (räumliche Datenbankerweiterung für PostgreSQL), spezifische PostGIS-Versionen für PostgreSQL 16 und die pgvector-Erweiterung (für die Speicherung und Suche von Vektoreinbettungen).
* `sudo apt-get install osm2pgsql osmosis`: Installiert `osm2pgsql` (ein Werkzeug zum Importieren von OpenStreetMap-Daten in eine PostgreSQL/PostGIS-Datenbank) und `osmosis` (ein Befehlszeilenwerkzeug zur Verarbeitung von OpenStreetMap-Daten).

**2. PostgreSQL-Konfiguration:**

* `ssh thorsten@`: Versucht, sich per SSH als Benutzer `thorsten` auf dem lokalen oder einem entfernten Server einzuloggen. Die nachfolgenden Befehle werden nach erfolgreicher SSH-Verbindung ausgeführt.
* `cd $HOME`: Wechselt in das Home-Verzeichnis des Benutzers `thorsten`.
* `sudo -u postgres -i`: Wechselt zum PostgreSQL-Systembenutzer (`postgres`) in einer interaktiven Shell. Dies ist notwendig, um administrative Aufgaben in PostgreSQL auszuführen.
* `createuser thorsten`: Erstellt einen neuen PostgreSQL-Benutzer namens `thorsten`.
* `createdb -E UTF8 -O thorsten thorsten`: Erstellt eine neue PostgreSQL-Datenbank namens `thorsten`. `-E UTF8` legt die Zeichenkodierung auf UTF-8 fest, und `-O thorsten` setzt `thorsten` als Eigentümer der Datenbank.
* `psql -d thorsten -c "CREATE EXTENSION postgis;"`: Stellt eine Verbindung zur Datenbank `thorsten` her und führt den SQL-Befehl aus, um die PostGIS-Erweiterung zu aktivieren.
* `psql -d thorsten -c "CREATE EXTENSION hstore;"`: Stellt eine Verbindung zur Datenbank `thorsten` her und führt den SQL-Befehl aus, um die `hstore`-Erweiterung zu aktivieren (ermöglicht die Speicherung von Schlüssel-Wert-Paaren in einer einzelnen Spalte).
* `psql -d thorsten -c "CREATE EXTENSION vector;"`: Stellt eine Verbindung zur Datenbank `thorsten` her und führt den SQL-Befehl aus, um die `vector`-Erweiterung zu aktivieren (ermöglicht die Speicherung und Suche von Vektoreinbettungen).
* `psql -d thorsten -c "ALTER TABLE geometry_columns OWNER TO thorsten;"`: Ändert den Eigentümer der Tabelle `geometry_columns` (die von PostGIS verwendet wird) zu `thorsten`.
* `psql -d thorsten -c "ALTER TABLE spatial_ref_sys OWNER TO thorsten;"`: Ändert den Eigentümer der Tabelle `spatial_ref_sys` (die von PostGIS für räumliche Referenzsysteme verwendet wird) zu `thorsten`.
* `psql -d thorsten -c "\password thorsten"`: Ändert das Passwort des PostgreSQL-Benutzers `thorsten`. Die Eingabe des neuen Passworts wird interaktiv abgefragt.
* `exit`: Verlässt die PostgreSQL-Shell und kehrt zum vorherigen Benutzer zurück.
* `cd $HOME`: Wechselt zurück in das Home-Verzeichnis des Benutzers `thorsten`.

**3. OpenStreetMap-Datenverarbeitung:**

* `wget https://download.geofabrik.de/europe/germany/schleswig-holstein-latest.osm.pbf`: Lädt die aktuellsten OpenStreetMap-Daten für Schleswig-Holstein im `.osm.pbf`-Format herunter und speichert sie als `schleswig-holstein-latest.osm.pbf` im aktuellen Verzeichnis.
* `osmosis --read-pbf file=schleswig-holstein-latest.osm.pbf --bounding-box left=10.1141 right=10.3716 top=53.7136 bottom=53.6249 --write-pbf file=ahrensburg.pbf`: Verwendet `osmosis`, um einen geografischen Ausschnitt aus der heruntergeladenen OSM-Datei zu extrahieren. Die Parameter `--bounding-box` definieren die Koordinaten des gewünschten Rechtecks (linke, rechte, obere und untere Koordinate). Die gefilterten Daten werden in der Datei `ahrensburg.pbf` gespeichert. Die angegebenen Koordinaten umfassen wahrscheinlich das Gebiet um Ahrensburg.

**4. OpenStreetMap-Datenimport:**

* `osm2pgsql -d thorsten --create -G --hstore ahrensburg.pbf`: Importiert die OpenStreetMap-Daten aus der Datei `ahrensburg.pbf` in die PostgreSQL-Datenbank `thorsten`.
    * `-d thorsten`: Gibt die Zieldatenbank an (`thorsten`).
    * `--create`: Erstellt die notwendigen Tabellen in der Datenbank, falls sie noch nicht existieren.
    * `-G`: Fügt eine Spalte für die geometrischen Daten (Punkte, Linien, Polygone) hinzu, die von PostGIS verwaltet wird.
    * `--hstore`: Aktiviert die Speicherung von zusätzlichen OpenStreetMap-Tags in einer `hstore`-Spalte in den Tabellen.

Zusammenfassend bereiten diese Befehle das System vor, laden OpenStreetMap-Daten für Ahrensburg herunter und importieren diese Daten in eine PostgreSQL-Datenbank mit räumlicher Unterstützung durch PostGIS und der Möglichkeit, zusätzliche Daten als Schlüssel-Wert-Paare mit `hstore` und Vektoreinbettungen mit `vector` zu speichern. Der Benutzer `thorsten` wird als Eigentümer der Datenbank und relevanter Tabellen festgelegt.
## dotnet Installieren
```bash
sudo apt-get update # Aktualisiert die Paketliste
sudo apt-get install -y dotnet-sdk-9.0 # Installation des .NET SDK
sudo apt-get install -y dotnet-sdk-8.0 # Installation des .NET SDK
echo 'export PATH=$HOME/.dotnet/tools:$PATH' >> ~/.bashrc # Hinzufügen des Pfads zum .bashrc
source ~/.bashrc  # Aktualisierung der .bashrc

```
### dotnet Installieren - Detaillierte Beschreibung der Arbeitsschritte

Dieser Abschnitt beschreibt detailliert die einzelnen Befehle, die zur Installation des .NET SDK unter Linux verwendet werden.

#### 1. `sudo apt-get update`

* **Arbeitsschritt:** Dieser Befehl aktualisiert die lokale Paketliste Ihres Debian-basierten Systems (wie Ubuntu oder das System, auf dem Sie arbeiten).
* **Beschreibung:**
    * `sudo`: Dieser Befehl führt den nachfolgenden Befehl mit Superuser-Rechten aus. Dies ist notwendig, da das Aktualisieren der Paketliste Systemdateien betrifft. Sie werden möglicherweise nach Ihrem Passwort gefragt.
    * `apt-get`: Dies ist ein Befehlszeilen-Tool, das zur Verwaltung von Paketen auf Debian-basierten Systemen verwendet wird.
    * `update`: Diese Option von `apt-get` lädt Informationen über die neuesten verfügbaren Versionen von Softwarepaketen aus den konfigurierten Paketquellen (Repositories) herunter. Es werden *keine* Software-Updates oder -Installationen durchgeführt. Der Befehl aktualisiert lediglich die Datenbank der verfügbaren Pakete und deren Abhängigkeiten.
* **Ergebnis:** Nach erfolgreicher Ausführung dieses Befehls verfügt Ihr System über die aktuellsten Informationen über die verfügbaren Softwarepakete. Dies ist ein wichtiger erster Schritt, um sicherzustellen, dass Sie die neuesten Versionen des .NET SDK installieren können.

#### 2. `sudo apt-get install -y dotnet-sdk-9.0`

* **Arbeitsschritt:** Dieser Befehl installiert das .NET SDK in der Version 9.0 auf Ihrem System.
* **Beschreibung:**
    * `sudo`: Wie bereits erwähnt, führt dieser Befehl den nachfolgenden Befehl mit Superuser-Rechten aus, da die Installation von Software systemweite Änderungen erfordert.
    * `apt-get`: Das Paketverwaltungstool.
    * `install`: Diese Option von `apt-get` wird verwendet, um neue Pakete zu installieren.
    * `-y`: Diese Option weist `apt-get` an, alle Fragen mit "yes" zu beantworten. Das bedeutet, dass die Installation ohne weitere Benutzerinteraktion fortgesetzt wird. Seien Sie vorsichtig bei der Verwendung dieser Option, da sie automatische Entscheidungen treffen kann.
    * `dotnet-sdk-9.0`: Dies ist der Name des Pakets, das das .NET SDK in der Version 9.0 enthält. `apt-get` sucht in den zuvor aktualisierten Paketquellen nach diesem Paket und seinen Abhängigkeiten und installiert diese.
* **Ergebnis:** Nach erfolgreicher Ausführung dieses Befehls ist das .NET SDK in der Version 9.0 auf Ihrem System installiert und einsatzbereit. Sie können nun .NET-Anwendungen entwickeln, erstellen und ausführen, die auf dieser Version abzielen.

#### 3. `sudo apt-get install -y dotnet-sdk-8.0`

* **Arbeitsschritt:** Dieser Befehl installiert das .NET SDK in der Version 8.0 auf Ihrem System.
* **Beschreibung:**
    * Dieser Befehl funktioniert analog zu dem vorherigen Befehl (`sudo apt-get install -y dotnet-sdk-9.0`), installiert jedoch das .NET SDK in der Version 8.0.
    * Es ist möglich, mehrere Versionen des .NET SDK parallel auf einem System zu installieren. Dies kann nützlich sein, wenn Sie an Projekten arbeiten, die unterschiedliche .NET-Versionen erfordern.
* **Ergebnis:** Nach erfolgreicher Ausführung dieses Befehls ist auch das .NET SDK in der Version 8.0 auf Ihrem System installiert. Sie können nun zwischen den Versionen 8.0 und 9.0 wählen, wenn Sie neue .NET-Projekte erstellen oder bestehende ausführen.

#### 4. `echo 'export PATH=$HOME/.dotnet/tools:$PATH' >> ~/.bashrc`

* **Arbeitsschritt:** Dieser Befehl fügt eine Zeile hinzu, die den Pfad zu den global installierten .NET-Tools zur `PATH`-Umgebungsvariable Ihrer Bash-Shell hinzufügt. Diese Änderung wird in der Konfigurationsdatei `.bashrc` gespeichert.
* **Beschreibung:**
    * `echo 'export PATH=$HOME/.dotnet/tools:$PATH'`: Dieser Teil des Befehls erzeugt eine Textausgabe.
        * `export PATH=...`: Dies ist ein Befehl, der die `PATH`-Umgebungsvariable setzt oder erweitert. Die `PATH`-Variable enthält eine Liste von Verzeichnissen, in denen das System nach ausführbaren Dateien sucht, wenn Sie einen Befehl im Terminal eingeben.
        * `$HOME/.dotnet/tools`: Dies ist der Standardpfad, in dem global installierte .NET-Tools (z.B. über den Befehl `dotnet tool install --global`) gespeichert werden. `$HOME` ist eine Umgebungsvariable, die Ihr Home-Verzeichnis repräsentiert.
        * `:$PATH`: Dies hängt den aktuellen Wert der `PATH`-Variablen an den neuen Pfad an, sodass bereits vorhandene Einträge in der `PATH`-Variablen erhalten bleiben.
    * `>> ~/.bashrc`: Dieser Teil des Befehls leitet die Ausgabe des `echo`-Befehls (die `export`-Zeile) an das Ende der Datei `.bashrc` um. Die Datei `.bashrc` ist eine Shell-Konfigurationsdatei, die ausgeführt wird, wenn Sie ein neues interaktives Bash-Terminal starten.
* **Ergebnis:** Nach Ausführung dieses Befehls wird bei jedem neuen Start eines Bash-Terminals der Pfad `$HOME/.dotnet/tools` zur `PATH`-Umgebungsvariable hinzugefügt. Dadurch können Sie global installierte .NET-Tools direkt über den Terminal aufrufen, ohne den vollständigen Pfad angeben zu müssen.

#### 5. `source ~/.bashrc`

* **Arbeitsschritt:** Dieser Befehl liest und führt den Inhalt der Datei `.bashrc` im aktuellen Terminalfenster aus.
* **Beschreibung:**
    * `source`: Dieser Befehl (manchmal auch als `.` abgekürzt) wird verwendet, um ein Skript oder eine Konfigurationsdatei im aktuellen Shell-Prozess auszuführen. Im Gegensatz zur direkten Ausführung eines Skripts mit `sh` oder `bash` wird hier kein neuer Subprozess gestartet. Stattdessen werden die in der Datei definierten Variablen und Funktionen direkt in die aktuelle Shell-Umgebung übernommen.
    * `~/.bashrc`: Dies ist der Pfad zur Bash-Konfigurationsdatei in Ihrem Home-Verzeichnis.
* **Ergebnis:** Durch die Ausführung dieses Befehls werden die Änderungen, die Sie gerade mit dem `echo`-Befehl in die `.bashrc`-Datei geschrieben haben (das Hinzufügen des .NET-Tools-Pfads zur `PATH`-Variablen), sofort in der aktuellen Terminal-Sitzung wirksam. Ohne diesen Befehl müssten Sie das Terminalfenster schließen und neu öffnen, damit die Änderungen in `.bashrc` geladen werden.

Nachdem Sie diese Schritte ausgeführt haben, sollten das .NET SDK in den Versionen 9.0 und 8.0 erfolgreich auf Ihrem System installiert sein und Sie können global installierte .NET-Tools bequem über die Befehlszeile verwenden. Sie können die Installation überprüfen, indem Sie die folgenden Befehle in Ihrem Terminal ausführen:

## Start des Projekts
```bash
dotnet new mvc --auth individual -o ahrensburg.city
```
Der Befehl `dotnet new mvc --auth individual -o ahrensburg.city`, der im Abschnitt "Start des Projekt" verwendet wird, dient dazu, ein neues ASP.NET Core MVC-Projekt zu erstellen. Hierbei wird die MVC-Architektur (Model-View-Controller) genutzt, um eine klare Trennung zwischen Daten, Benutzeroberfläche und Steuerungslogik zu gewährleisten.

Die Option `--auth individual` konfiguriert das Projekt mit individueller Benutzer-Authentifizierung. Dies bedeutet, dass Funktionen wie Benutzerregistrierung, Anmeldung und Profilverwaltung bereits integriert sind. ASP.NET Core Identity wird hierbei als Standardlösung verwendet.

Mit der Option `-o ahrensburg.city` wird das Projekt in einem neuen Verzeichnis namens `ahrensburg.city` erstellt. Dieses Verzeichnis dient als Arbeitsbereich für die Webanwendung.

Zusammengefasst legt dieser Befehl die Grundlage für eine Webanwendung mit integrierter Benutzerverwaltung und einer klar strukturierten Architektur.



## Visual Studio Code Erweiterung Installieren
```
code --install-extension ms-dotnettools.csdevkit # Installiert die C# Dev Kit Erweiterung
```
## Entity Framework Installieren
```
dotnet tool install --global dotnet-ef
```
## Nuget Pakete installieren
* [Npgsql](https://www.nuget.org/packages/Npgsql)
* [Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL.NetTopologySuite)
* [Npgsql.EntityFrameworkCore.PostgreSQL](https://www.nuget.org/packages/Npgsql.EntityFrameworkCore.PostgreSQL)

## Erstelle eine Datei appsettings.json
```
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=test;Username=test;Password=Test"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}

```
## Dotnet 
```
dotnet ef dbcontext scaffold "Host=localhost;Database=Test;Username=Test;Password=Test" Npgsql.EntityFrameworkCore.PostgreSQL -o Models -c MapDbContext - -t planet_osm_line -t planet_osm_point -t planet_osm_point -t planet_osm_polygon -t planet_osm_roads --data-annotations 
```
Verwendung: dotnet ef dbcontext scaffold [Argumente] [Optionen]

Argumente:
  `<CONNECTION>`          Die Verbindungszeichenfolge zur Datenbank.
  `<PROVIDER>`            Der zu verwendende Provider. (z.B. Microsoft.EntityFrameworkCore.SqlServer)

Optionen:
* -d|--data-annotations   Verwendet Attribute zur Konfiguration des Modells (wo möglich). Wenn weggelassen, wird nur die Fluent API verwendet.
* -c|--context <NAME>     Der Name des DbContext. Standardmäßig der Datenbankname.
* --context-dir <PATH>    Das Verzeichnis, in dem die DbContext-Datei abgelegt werden soll. Pfade sind relativ zum Projektverzeichnis.
* -f|--force              Überschreibt vorhandene Dateien.
* -o|--output-dir <PATH>  Das Verzeichnis, in dem Dateien abgelegt werden sollen. Pfade sind relativ zum Projektverzeichnis.
*  --schema <SCHEMA_NAME>... Die Schemas von Tabellen und Ansichten, für die Entitätstypen generiert werden sollen. Alle Tabellen und Ansichten in den Schemas werden in das Modell aufgenommen, auch wenn sie nicht explizit mit dem Parameter --table angegeben werden.
*  -t|--table <TABLE_NAME>... Die Tabellen und Ansichten, für die Entitätstypen generiert werden sollen. Tabellen oder Ansichten in einem bestimmten Schema können im Format 'schema.tabelle' oder 'schema.ansicht' angegeben werden.
* --use-database-names    Verwendet Tabellen-, Ansichts-, Sequenz- und Spaltennamen direkt aus der Datenbank.
* --json                  Zeigt JSON-Ausgabe an. Zur programmatischen Analyse mit --prefix-output verwenden.
* -n|--namespace <NAMESPACE> Der zu verwendende Namespace. Standardmäßig das Verzeichnis.
* --context-namespace <NAMESPACE> Der Namespace der DbContext-Klasse. Standardmäßig das Verzeichnis.
* --no-onconfiguring      Generiert kein DbContext.OnConfiguring.
*  --no-pluralize          Verwendet keine Pluralisierung.
* -p|--project <PROJECT>    Das zu verwendende Projekt. Standardmäßig das aktuelle Arbeitsverzeichnis.
*  -s|--startup-project <PROJECT> Das zu verwendende Startup-Projekt. Standardmäßig das aktuelle Arbeitsverzeichnis.
*  --framework <FRAMEWORK>   Das Zielframework. Standardmäßig das erste im Projekt.
*  --configuration <CONFIGURATION> Die zu verwendende Konfiguration.
* --runtime <RUNTIME_IDENTIFIER> Die zu verwendende Runtime.
* --msbuildprojectextensionspath <PATH> Der Pfad zu den MSBuild-Projekterweiterungen. Standardmäßig "obj".
* --no-build              Erstellt das Projekt nicht. Soll verwendet werden, wenn der Build aktuell ist.
* -h|--help               Zeigt Hilfestellungen an.
* -v|--verbose            Zeigt ausführliche Ausgaben an.
* --no-color              Verwendet keine farbige Ausgabe.
* --prefix-output         Präfixiert die Ausgabe mit der Ebene.

## Migrations
```
dotnet ef migrations add InitialMigration --context ApplicationDbContext
dotnet ef migrations add MapMigration --context MapDbContext
dotnet ef database update --context MapDbContext
dotnet ef database update --context ApplicationDbContext

```



