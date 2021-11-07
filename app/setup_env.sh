# nginx and server can read envionment variables, but clash cannot.
sed -i "s/<domain>/$DOMAIN/g" clash.conf