ssh hhhh@wiki-ahrensburg.de "cd /var/www/wiki-ahrensburg-de && sudo ./backup export /root/meine_sicherung.xml --full"
scp jjjjj@wiki-ahrensburg.de:/root/meine_sicherung.xml /home/thorsten/Downloads/meine_sicherung.xml
