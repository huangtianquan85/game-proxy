# USAGE:
# docker run -d -p xx:7890 -v xx:/app/domain-crt -v xx:/app/domain-key -v xx:/app/server-key -e OFFSET=1 game-proxy

FROM dreamacro/clash

# source in China
RUN sed -i 's/dl-cdn.alpinelinux.org/mirrors.aliyun.com/g' /etc/apk/repositories

# install supervisor
RUN apk add --no-cache supervisor \
    && mkdir -p /var/log/supervisor \
    && sed -i 's/\[supervisord\]/\[supervisord\]\nnodaemon=true/' /etc/supervisord.conf \
    && sed -i 's/files = .*/files = \/app\/*.supervisor\n/' /etc/supervisord.conf

ENTRYPOINT ["/usr/bin/supervisord"]

# install nginx
RUN apk add --no-cache nginx

# install app
COPY app /app