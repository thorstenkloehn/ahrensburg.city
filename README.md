# Ahrensburg.city
## Installieren Ubunto 22.04
### Nutzer erstellen
Neuen Nutzer erstellen
```bash
sudo adduser --force-badname _renderd
sudo usermod -aG sudo _renderd
exit
```
### ssh _renderd
```bash
ssh _renderd@
```
### Vorbereitung
#### Nodejs
* [Anleitung](https://github.com/nodesource/distributions) Erste Eineichrichtung
#### Weitere Pakete
```bash
sudo apt update
sudo apt upgrade
sudo apt install screen locate libapache2-mod-tile renderd git tar unzip wget bzip2 apache2 lua5.1 mapnik-utils python3-mapnik python3-psycopg2 python3-yaml gdal-bin fonts-noto-cjk fonts-noto-hinted fonts-noto-unhinted fonts-unifont fonts-hanazono postgresql postgresql-contrib postgis postgresql-14-postgis-3 postgresql-14-postgis-3-scripts osm2pgsql net-tools curl osmosis
```
### Postgres
Posdtgres Datenbank erstellen
```bash
sudo -u postgres -i
createuser _renderd
createdb -E UTF8 -O _renderd gis
```
Postgres Datenbank konfigurieren
```bash
psql
\c gis
CREATE EXTENSION postgis;
CREATE EXTENSION hstore;
ALTER TABLE geometry_columns OWNER TO _renderd;
ALTER TABLE spatial_ref_sys OWNER TO _renderd;
\q
exit
```
### Mapnik Testen
```bash
python3
import mapnik
exit()
```
### Stylesheet configuration
```bash
mkdir ~/src
cd ~/src
git clone https://github.com/gravitystorm/openstreetmap-carto
cd openstreetmap-carto
sudo npm install -g carto
carto -v
carto project.mml > mapnik.xml
```
### Loading data

```bash
mkdir ~/data
cd ~/data
wget https://download.geofabrik.de/europe/germany/schleswig-holstein-latest.osm.pbf
osmosis --read-pbf file=schleswig-holstein-latest.osm.pbf --bounding-box left=10.1141 right=10.3716 top=53.7136 bottom=53.6249 --write-pbf file=ahrensburg.pbf

chmod o+rx ~
sudo -u _renderd osm2pgsql -d gis --create --slim  -G --hstore --tag-transform-script ~/src/openstreetmap-carto/openstreetmap-carto.lua -C 2500 --number-processes 1 -S ~/src/openstreetmap-carto/openstreetmap-carto.style ~/data/ahrensburg.pbf
```
### Index erstellen
```bash
cd ~/src/openstreetmap-carto/
sudo -u _renderd psql -d gis -f indexes.sql
```
### Shapefile download
```bash
cd ~/src/openstreetmap-carto/
mkdir data
sudo chown _renderd data
sudo -u _renderd scripts/get-external-data.py
```
### Fonts
```bash
cd ~/src/openstreetmap-carto/
scripts/get-fonts.sh
```
### Einstellung renderd
```bash
sudo nano /etc/renderd.conf
```
```bash
[s2o]
URI=/hot/
XML=/home/_renderd/src/openstreetmap-carto/mapnik.xml
HOST=localhost
TILESIZE=256
MAXZOOM=20
```
### Making sure that you can see debug messages
```bash
sudo nano /usr/lib/systemd/system/renderd.service
```
```bash
sudo systemctl daemon-reload
sudo systemctl restart renderd
sudo systemctl restart apache2
```
### Apache2
```bash
sudo /etc/init.d/apache2 restart
```
### Apache2 Port ändern
```bash
sudo nano /etc/apache2/ports.conf
```
```bash
Listen 8080
```
### Apache2 Reatart
```bash
sudo /etc/init.d/apache2 restart
```
### ahrensburg.city Installieren
```bash
sudo apt-get install nginx
sudo rm /etc/nginx/sites-enabled/default
sudo systemctl stop nginx
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
sudo certbot certonly --standalone -d ahrensburg.city -d www.ahrensburg.city
sudo certbot certonly --standalone -d karte.ahrensburg.city
sudo certbot certonly --standalone -d wiki.ahrensburg.city
sudo certbot certonly --standalone -d blog.ahrensburg.city
```
Neuen Nutzer erstellen
```bash
sudo adduser thorsten
sudo usermod -aG sudo thorsten
exit
```
### ssh thorsten
```bash
ssh thorsten@
cd $HOME
sudo -u postgres -i
createuser thorsten
createdb -E UTF8 -O thorsten thorsten
psql
\c thorsten
CREATE EXTENSION postgis;
CREATE EXTENSION hstore;
ALTER TABLE geometry_columns OWNER TO thorsten;
ALTER TABLE spatial_ref_sys OWNER TO thorsten;
## Passwort für den Benutzer postgres setzen
\password thorsten
\q
exit
```
### Herunterladen der Daten
```bash
cd $HOME
wget https://download.geofabrik.de/europe/germany/schleswig-holstein-latest.osm.pbf
osmosis --read-pbf file=schleswig-holstein-latest.osm.pbf --bounding-box left=10.1141 right=10.3716 top=53.7136 bottom=53.6249 --write-pbf file=ahrensburg.pbf

osm2pgsql  -d thorsten --create  -G --hstore  ahrensburg.pbf
```
### Build von Ahrensburg.city
```bash
cd $HOME
git clone https://github.com/thorstenkloehn/ahrensburg.city.git
cd ahrensburg.city
npm install
npm run build
```
## Systemctl Einrichten
```bash
sudo cp ahrensburg-city.service /etc/systemd/system/ahrensburg-city.service
sudo systemctl enable ahrensburg-city
sudo systemctl start ahrensburg-city
```
### config daten kopieren
```bash 
cp env.local.example .env.local
```

## Restart
```bash
sudo cp ahrensburg-city.conf /etc/nginx/conf.d/ahrensburg-city.conf
sudo systemctl restart ahrensburg-city
```

## Hallo