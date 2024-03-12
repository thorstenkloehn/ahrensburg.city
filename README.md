# Ahrensburg.city Installieren
## Installieren von Ahrensburg.city
```bash
cd $HOME
curl -fsSL https://deb.nodesource.com/setup_20.x | sudo -E bash - &&\
sudo apt-get install -y nodejs
sudo apt install snapd
sudo apt install git
sudo apt-get install nginx
sudo rm /etc/nginx/sites-enabled/default
sudo systemctl stop nginx
sudo snap install --classic certbot
sudo ln -s /snap/bin/certbot /usr/bin/certbot
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

### Nginx Einrichten
```
sudo cp ahrensburg-city.conf /etc/nginx/conf.d/ahrensburg-city.conf
sudo nginx -t
sudo systemctl restart nginx
```
### config daten kopieren
```
cp env.local.example .env.local
```
