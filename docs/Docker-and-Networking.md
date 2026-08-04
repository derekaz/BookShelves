# Docker and Networking Notes

This document captures the solution's local Docker and reverse-proxy behavior.
Treat it as a working reference, not a design spec.

## Local Docker Workflow

From the repo root, build and run the web and API containers with:

```powershell
docker compose --env-file .env.development-laptop up --build -d
```

## Current Routing Model

The Docker setup uses nginx as a front door for the services.
It routes only application paths such as `/` for the web site and `/api` for the Web API.
It does not provide broader application-level proxying.

## Future Considerations

- Keep app containers on an internal bridge network
- Route only the documented application paths at the proxy layer
- Use a dedicated LAN-facing network only when a fixed host IP is required
- Avoid hard-coding environment-specific values in source-controlled files

## Related Files

- `src/docker-compose.yml`
- `src/BookShelves.Web/BookShelves.Web/Dockerfile`
- `src/BookShelves.WebApi/Dockerfile`
