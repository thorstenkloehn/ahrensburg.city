## Ahrensburg.city Installieren
## Installieren von Ahrensburg.city
```bash
cd $HOME
sudo apt install snapd

## cerbort Installation
```bash
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
```
## Zertifikat erstellen
```bash
sudo rm /etc/nginx/sites-enabled/default
sudo systemctl stop nginx
```

## Cerbort Konfigurieren
```
sudo certbot certonly --standalone -d ahrensburg.city -d www.ahrensburg.city
```
### Build von Ahrensburg.city
```
cd $HOME
git clone https://github.com/thorstenkloehn/ahrensburg.city.git
cd ahrensburg.city
npm install
npm run build
```
### Systemctl Einrichten
```
sudo cp ahrensburg-city.service /etc/systemd/system/ahrensburg-city.service
sudo systemctl enable ahrensburg-city
sudo systemctl start ahrensburg-city
```
### Nginx Installieren
```
sudo apt update
sudo apt install nginx
```
### Nginx Einrichten
```
sudo cp ahrensburg-city.conf /etc/nginx/conf.d/ahrensburg-city.conf
sudo nginx -t
sudo systemctl restart nginx
```