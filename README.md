## Ahrensburg.city Installieren
## Installieren von Ahrensburg.city
## Cerbort Konfigurieren
```
sudo certbot certonly --standalone -d ahrensburg.city
```
### Build von Ahrensburg.city
```
cd $HOME
gh repo clone thorstenkloehn/ahrensburg.city
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