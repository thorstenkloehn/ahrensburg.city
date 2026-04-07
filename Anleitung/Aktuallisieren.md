```bash
# Zum Projektverzeichnis navigieren
cd /home/thorsten/ahrensburg.city

# Release erstellen und veröffentlichen
dotnet publish -c Release -r linux-x64 --self-contained false -o /home/thorsten/publis/ahrensburgcity/

# Veröffentlichte Dateien zum Remote-Server synchronisieren
rsync -avz --exclude 'bin' --exclude 'obj' --exclude 'config' /home/thorsten/publis/ahrensburgcity/ tt@ah.city:/var/www/ahrensburgcity/

dotnet mvc.dll --migrate
```