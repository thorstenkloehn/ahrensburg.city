```bash
# Zum Projektverzeichnis navigieren
cd /home/thorsten/wiki-ahrensburg.de

# Release erstellen und veröffentlichen
dotnet publish -c Release -r linux-x64 --self-contained false -o /home/thorsten/publis/wiki-ahrensburg-de/

# Veröffentlichte Dateien zum Remote-Server synchronisieren
rsync -avz --exclude 'bin' --exclude 'obj' --exclude 'config' /home/thorsten/publis/wiki-ahrensburg-de/ tt@ah.city:/var/www/wiki-ahrensburg-de/

dotnet mvc.dll --migrate
## Import
sudo ./backup import thomas.xml
oder sudo ./backup import thorsten/thomas.xml
## Export
sudo ./backup export meine_sicherung.xml --full

```

