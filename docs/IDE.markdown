---
layout: default
title: IDE
permalink: /IDE/
importance: 10
---
## Voraussetzungen

- [x] Ubuntu 18.04 LTS oder höher
- [x] Root Zugriff
- [x] SSH Zugriff
- [x] Internet Verbindung

## Ubuntu 18.04 LTS oder höher
Browser öffnen und auf [Ubuntu](https://ubuntu.com/download/desktop) gehen und die gewünschte Version herunterladen.
### usb-stick erstellen
#### Windows
- [x] [Rufus](https://rufus.ie/) herunterladen
- [x] Rufus starten
- [x] USB-Stick auswählen
- [x] ISO-Image auswählen
- [x] Starten
#### Ubuntu Deskop
-  USB-Stick einstecken
- Programm Startmedienerstellen öffnen
- ISO-Image auswählen
- USB-Stick auswählen
- Starten

### Ubuntu installieren

- USB-Stick einstecken
- PC neustarten
- Bootmenü öffnen
- USB-Stick auswählen
- Ubuntu installieren
- Sprache auswählen
- Tastatur auswählen
- Installationstyp auswählen
- Benutzerdaten eingeben
- Installation starten
-  Neustarten

## ubunru

## Root Zugriff
### Root aktivieren
- Terminal öffnen
- sudo passwd root 
- Passwort eingeben
- Passwort wiederholen
- Neustarten
### Root anmelden
- Terminal öffnen
- `su` eingeben
- Passwort eingeben
## Vorbereitung

### Aktualisieren

- Öffne Sie das Terminal und auf ihrem Ubuntu-Desktop
- Führen Sie folgenden Befehl aus,um System zu aktualisieren.
```bash
sudo apt-get update 
sudo apt-get upgrade
```
### Google Chrome Installieren
```bash
wget https://dl.google.com/linux/direct/google-chrome-stable_current_amd64.deb
 sudo dpkg -i google-chrome-stable_current_amd64.deb
 ```
#### Ubuntu 20.04 Lts gh installieren
```bash
type -p curl >/dev/null || (sudo apt update && sudo apt install curl -y)
curl -fsSL https://cli.github.com/packages/githubcli-archive-keyring.gpg | sudo dd of=/usr/share/keyrings/githubcli-archive-keyring.gpg \
&& sudo chmod go+r /usr/share/keyrings/githubcli-archive-keyring.gpg \
&& echo "deb [arch=$(dpkg --print-architecture) signed-by=/usr/share/keyrings/githubcli-archive-keyring.gpg] https://cli.github.com/packages stable main" | sudo tee /etc/apt/sources.list.d/github-cli.list > /dev/null \
&& sudo apt update \
&& sudo apt install gh -y
```
##### Webadresse ist
- [Download gh](https://cli.github.com/)

### Git Installieren
```bash
sudo apt-get install git  gh
git config --global user.email "you@example.com"
git config --global user.name "Your Name"

```
#### gh Konfigurieren

```bash
gh auth login
```
### Programmiersprachen Installieren
https://github.com/thorstenkloehn/Allgemein.git
#### Rust Installieren
```bash
sudo apt  install curl 
sudo apt install build-essential
curl --proto '=https' --tlsv1.2 -sSf https://sh.rustup.rs | sh
```
#### Nodejs Installieren
```bash
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash - &&\
sudo apt-get install -y nodejs
```
## Visual Studio Code 
### Installieren
```bash
sudo snap install code --classic
```
### Pflugin
- Starten Sie VS Code Quick Open ( Ctrl+P), fügen Sie den folgenden Befehl ein und drücken Sie die Eingabetaste.
```bash
code --install-extension GitHub.copilot
code --install-extension rust-lang.rust-analyzer
code --install-extension ckolkman.vscode-postgres
code --install-extension  ms-ossdata.vscode-postgresql
```
## jekyll
### Installieren Sie Jekyll mit folgendem Befehl.
```bash
sudo apt-get install ruby-full build-essential zlib1g-dev
echo '# Install Ruby Gems to ~/gems' >> ~/.bashrc
echo 'export GEM_HOME="$HOME/gems"' >> ~/.bashrc
echo 'export PATH="$HOME/gems/bin:$PATH"' >> ~/.bashrc
source ~/.bashrc
gem install jekyll bundler
```
## Gatsby
### Installieren Sie Gatsby.
```bash
npm init gatsby
```
## Hugo
### Installieren Sie Hugo.
```bash
sudo snap install hugo
```
 ## Datenbank
### PostgreSQL
#### Installation

```bash
sudo apt-get install postgresql-all
```
### PosgreSQL Version zeigen
```bash
psql --version
```

### Postgis Installation in Ubuntu 23.04
```bash
sudo apt install postgis postgresql-16-postgis-3 postgresql-16-postgis-3-scripts
```
### Installation von osm2pgsql und osmosis
```bash
sudo apt-get install osm2pgsql osmosis
```

### Datenbank erstellen
```bash
sudo adduser thorsten
sudo usermod -aG sudo thorsten
```

## Benutzer thorsten zu Gruppe postgres hinzufügen
```bash

sudo -u postgres -i
createuser thorsten
createdb -E UTF8 -O thorsten thorsten
psql
\c thorsten
CREATE EXTENSION postgis;
CREATE EXTENSION hstore;
ALTER TABLE geometry_columns OWNER TO thorsten;
ALTER TABLE spatial_ref_sys OWNER TO thorsten;
```
## Passwort für den Benutzer postgres setzen
```bash
\password thorsten
\q
exit

```
### Datenbank mit osm2pgsql befüllen
```bash
wget https://download.geofabrik.de/europe/germany/schleswig-holstein-latest.osm.pbf
osmosis --read-pbf file=schleswig-holstein-latest.osm.pbf --bounding-box left=10.23 right=10.33 top=53.71 bottom=53.61 --write-pbf file=ahrensburg.pbf
osm2pgsql  -d thorsten --create  -G --hstore  ahrensburg.pbf
```


### Datenbank Löschen
```bash
sudo -u postgres -i
psql
GRANT ALL PRIVILEGES ON DATABASE thorsten TO postgres;
drop database thorsten;
\q
```
## Anki
Anki ist ein Programm zum Lernen von Vokabeln und anderen Inhalten. Es ist für Windows, Linux und Mac OS X verfügbar. Anki ist Open Source und kostenlos. Es ist auch für Android und iOS verfügbar, aber diese Versionen sind nicht kostenlos.

### Anforderungen
```bash
sudo apt install libxcb-xinerama0 libxcb-cursor0

```
### Anki herunterladen
```bash
wget https://github.com/ankitects/anki/releases/download/23.12.1/anki-23.12.1-linux-qt6.tar.zst
```
### Anki installieren
```bash
tar xaf anki-23.12.1-linux-qt6.tar.zst
cd  anki-23.12.1-linux-qt6
sudo ./install.sh
QT_DEBUG_PLUGINS=1 anki
```

### Erweiterungen

#### Erweiterungen installieren
```bash
1436550454 1933645497 1463041493 1190756458
```

## Golang Installieren
```bash
wget https://go.dev/dl/go1.22.0.linux-amd64.tar.gz
sudo tar -C /usr/local -xzf go1.22.0.linux-amd64.tar.gz
```
### Golang Konfigurieren
```bash
nano ~/.bashrc
```
### Golang Konfigurieren
```bash
export PATH=$PATH:/usr/local/go/bin
export GOPATH=$HOME/go 
export PATH=$PATH:$GOPATH/bin
```
### Golang Konfigurieren
```bash
source ~/.bashrc
```
## Python Installieren
```bash
sudo apt install python3 python3-pip python3-venv
```
## Python vscode Erweiterungen
```bash
code --install-extension ms-python.python
code --install-extension ms-toolsai.jupyter
code --install-extension ms-vscode-remote.vscode-remote-extensionpack
code --install-extension ms-toolsai.jupyter-keymap
code --install-extension ms-toolsai.vscode-jupyter-cell-tags
code --install-extension ms-toolsai.vscode-jupyter-slideshow
code --install ms-dotnettools.dotnet-interactive-vscode
```
## Python Virtual Environment
```bash
python3 -m venv .venv
source .venv/bin/activate
```
## nginx Installieren
```bash
sudo apt install nginx
code --install-extension ahmadalli.vscode-nginx-conf
code --install-extension  shanoor.vscode-nginx
```
## Python Vorasusetzungen
### Ubuntu
```bash
sudo apt-get install build-essential libssl-dev zlib1g-dev libncurses5-dev libncursesw5-dev libreadline-dev libsqlite3-dev  libgdbm-dev libdb5.3-dev libbz2-dev libexpat1-dev liblzma-dev tk-dev libffi-dev uuid-dev#
```
#### hinunterladen von Python
```bash
wget https://www.python.org/ftp/python/3.12.2/Python-3.12.2.tgz
```
#### entpacken von Python
```bash
tar -xvf Python-3.12.2.tgz
```
#### wechseln in das Python Verzeichnis
```bash
cd Python-3.12.2
```
#### Installieren von Python
```bash
./configure
make
sudo make altinstall 
```
#### Quellangabe
* Quelle: [Download](https://www.python.org/downloads)
* Quelle: [Installieren Anleitung](https://wiki.ubuntuusers.de/Python/)

## Lumi Installieren
```bash
wget https://github.com/Lumieducation/Lumi/releases/download/v0.10.0/lumi_0.10.0_amd64.deb
sudo dpkg -i lumi_0.10.0_amd64.deb
```






