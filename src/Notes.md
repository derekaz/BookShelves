Based on examples provided by Microsoft at https://github.com/dotnet/blazor-samples


To build and run the web & api projects in local docker containers, run the following command from the root of the project:
docker compose --env-file .env.development-laptop up --build -d



google chat regarding docker build/deploy and SSL
https://share.google/aimode/zJtSlAOFJpXtTmhxE

google chat regarding fixing the duplicate apple build bundle version
https://share.google/aimode/6mZZY0Yh1MERtRCkb

continuation of the apple chat that takes into account feature branching and how/when to tag as well as GHA triggers for the feature branch as well as tags
https://share.google/aimode/fnQYTalX6Ffaiz5j4

more continuation of the chat regarding a comprehensive approach to the gha process...
https://share.google/aimode/rVfJ7ZMy1mcJnAeHf

DataSync is a Microsoft project that provides a set of libraries and tools for building data synchronization solutions. It allows developers to synchronize data between different platforms, such as mobile devices, web applications, and cloud services. DataSync can be used to keep data consistent across multiple devices and platforms, enabling offline access and real-time updates.
https://github.com/CommunityToolkit/Datasync


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


Branch/Release Management
		<Version>$(GitVersion_FullSemVer)</Version>
		<VersionPrefix>$(GitVersion_MajorMinorPatch)</VersionPrefix>
		<AssemblyVersion>$(GitVersion_MajorMinorPatch)</AssemblyVersion>
		<FileVersion>$(GitVersion_MajorMinorPatch)</FileVersion>
		<InformationalVersion>$(GitVersion_InformationalVersion)</InformationalVersion>
		<ApplicationVersion>$(GitVersion_CommitsSinceVersionSource)</ApplicationVersion>

Desired Outcome
Project|Maui - Windows|Maui - MacCatalyst|Maui - IOS|Web & WebApi - Docker
AssemblyVersion|0.1.2|0.1.2|0.1.2|0.1.2
FileVersion|0.1.2|0.1.2|0.1.2|0.1.2
InformationalVersion|0.1.2-dev.1+2|0.1.2-dev.1+2|0.1.2-dev.1+2|0.1.2-dev.1+2

Version|0.1.2-dev.1+2|0.1.2-dev.1+2|0.1.2-dev.1+2|0.1.2-dev.1+2
ApplicationDisplayVersion|0.1.2|0.1.2|0.1.2|0.1.2
ApplicationVersion|[1..n]|[1..n] (must be unique per ApplicationDisplayVersion)|[1..n] (must be unique per ApplicationDisplayVersion)|
PackageVersion|0.1.2|?|?|0.1.2

Docker Tags|NA|NA|NA|0.1.2-dev.1+2 (based source branch and/or tags present in that branch)


Build Triggers

Main - Commit|Build (Alpha + Increment/Last Tag In Branch + Increment)|Build (Alpha + Increment/Last Tag In Branch + Increment)|Build (Alpha + Increment/Last Tag In Branch + Increment)|Build (Alpha + Increment/Last Tag In Branch + Increment)
Main - Tag|Build & Deploy (Tag)|Build & Deploy (Tag)|Build & Deploy (Tag)|Build & Deploy (Tag)
Main - Release|Build & Deploy (Release)|Build & Deploy (Release)|Build & Deploy (Release)|Build & Deploy (Release)

Release/[major].[minor].[patch] - Commit|Build & Deploy (Dev/Tag + Increment)|Build & Deploy (Dev/Tag + Increment)|Build & Deploy (Dev/Tag + Increment)|Build & Deploy (Dev/Tag + Increment)
Release/[major].[minor].[patch] - Tag
Release/[major].[minor].[patch] -> PR Create -> Main
Release/[major].[minor].[patch] -> PR Merge -> Main

Feature/[feature-name] - Commit
Feature/[feature-name] -> PR Create -> Release/[major].[minor].[patch]
Feature/[feature-name] -> PR Merge -> Release/[major].[minor].[patch]



Maui - Windows|Yes|Yes|Yes
Maui - MacCatalyst|Yes|Yes|Yes
Maui - IOS|Yes|Yes|Yes

