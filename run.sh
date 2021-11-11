# replace domain and nginx ssl with your own
# use: `openssl genrsa -out app.pem 2048` to create app.pem
offset=1
name=game-env-$offset
docker run -d \
    -p 7890:7890 \
    -v $PWD/1_example.com_bundle.crt:/app/domain.crt \
    -v $PWD/2_example.com.key:/app/domain.key \
    -v $PWD/app.pem:/app/app.pem \
    -e DOMAIN=example.com \
    -e OFFSET=$offset \
    --restart=always \
    --name $name \
    huangtianquan85/game-proxy
