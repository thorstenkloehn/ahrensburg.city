
sudo cp -u start.conf /etc/nginx/conf.d/start.conf
  sudo cp -u mediawiki.conf /etc/nginx/conf.d/mediawiki.conf
  sudo cp -u lernen.conf /etc/nginx/conf.d/lernen.conf
  sudo cp -u photon.service /etc/systemd/system/photon.service
  sudo  systemctl enable photon.service
  sudo  systemctl start photon.service

