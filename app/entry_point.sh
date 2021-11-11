#!/bin/sh

# nginx and server can read envionment variables, but clash cannot.
sed -i "s/<domain>/$DOMAIN/g" /app/clash.conf

# start supervisor
/usr/bin/supervisord