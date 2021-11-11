cd server
go build -o ../app/server main.go
cd ..
docker build -t game-proxy .