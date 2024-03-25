---
layout: default
title: Wordprees
permalink: /Wordpress/
importance: 18
---
# Wordpress Installieren auf nginx Server auf Ubuntu 22.04
```bash
sudo apt update
sudo apt upgrade
sudo apt install nginx
sudo apt install mysql-server
sudo apt install php-curl php-gd php-intl php-mbstring php-soap php-xml php-xmlrpc php-zip
sudo mysql_secure_installation
sudo mysql
CREATE DATABASE wordpress;
CREATE USER 'wordpress'@'localhost'
IDENTIFIED BY 'password';
GRANT ALL PRIVILEGES ON wordpress.* TO 'wordpress'@'localhost';
FLUSH PRIVILEGES;
exit
```
## Wordpress herunterladen
```bash
cd /tmp
sudo mkdir /var/www/html
sudo cp -a /tmp/wordpress/. /var/www/html
sudo chown -R www-data:www-data /var/www/html
sudo find /var/www/html -type d -exec chmod 750 {} \;
sudo find /var/www/html -type f -exec chmod 640 {} \;
```
### Wordpress Konfigurieren nginx
```bash
sudo nano /etc/nginx/conf.d/ahrensburg-city.conf
```
```nginx
server {
    listen 80;
    server_name ahrensburg.city www.ahrensburg.city;
    root /var/www/html;
    index index.php index.html index.htm;
    location / {
        try_files $uri $uri/ /index.php?$args;
    }
    location ~ \.php$ {
        include snippets/fastcgi-php.conf;
        fastcgi_pass unix:/var/run/php/php7.4-fpm.sock;
        fastcgi_param SCRIPT_FILENAME $document_root$fastcgi_script_name;
        include fastcgi_params;
    }
}
```