Based on examples provided by Microsoft at https://github.com/dotnet/blazor-samples


To build and run the web & api projects in local docker containers, run the following command from the root of the project:
docker compose --env-file .env.development-laptop up --build -d



google chat regarding docker build/deploy and SSL
https://share.google/aimode/zJtSlAOFJpXtTmhxE

google chat regarding fixing the duplicate apple build bundle version
https://share.google/aimode/6mZZY0Yh1MERtRCkb


And maybe for getting the nginx endpoing to have it's own IP...something like this is suggested:  
    Might be able to use the already created network for the qnap_lan side (due to already having CADDY running this way)...investigate

<ins>docker-compose</ins>
services:
  nginx-proxy:
    image: nginx:latest
    container_name: nginx-proxy
    restart: always
    networks:
      qnap_lan:
        ipv4_address: 192.168.1.245 # The dedicated IP for your web services
      internal_net:
    ports:
      - "80:80"
      - "443:443"

  web-app:
    image: my-dotnet10-web-app
    container_name: dotnet-web
    networks:
      internal_net:

  web-api:
    image: my-dotnet10-web-api
    container_name: dotnet-api
    networks:
      internal_net:

networks:
  qnap_lan:
    external: true
  internal_net:
    driver: bridge


<ins>nginx.conf</ins>
server {
    listen 80;
    server_name myapp.local; # Or use the static IP 192.168.1.245

    location / {
        proxy_pass http://web-app:8080; # .NET 10 default HTTP port
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection keep-alive;
        proxy_set_header Host $host;
        proxy_cache_bypass $http_upgrade;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}

server {
    listen 80;
    server_name api.myapp.local;

    location / {
        proxy_pass http://web-api:8080; 
        # Same proxy headers as above
    }
}


