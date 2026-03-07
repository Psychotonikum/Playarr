# Reverse Proxy

Configure a reverse proxy to access Playarr through a domain or URL path.

## URL Base

If you want to serve Playarr under a path (e.g., `https://example.com/playarr`):

1. Edit `config.xml`:
   ```xml
   <UrlBase>/playarr</UrlBase>
   ```
2. Restart Playarr
3. Access at `http://localhost:9797/playarr`

## Nginx

### Subdomain

```nginx
server {
    listen 80;
    server_name playarr.example.com;

    location / {
        proxy_pass http://127.0.0.1:9797;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $http_connection;
    }
}
```

### Subpath

```nginx
location /playarr {
    proxy_pass http://127.0.0.1:9797/playarr;
    proxy_set_header Host $host;
    proxy_set_header X-Real-IP $remote_addr;
    proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
    proxy_set_header X-Forwarded-Proto $scheme;
    proxy_http_version 1.1;
    proxy_set_header Upgrade $http_upgrade;
    proxy_set_header Connection $http_connection;
}
```

### With SSL (Let's Encrypt)

```nginx
server {
    listen 443 ssl http2;
    server_name playarr.example.com;

    ssl_certificate /etc/letsencrypt/live/playarr.example.com/fullchain.pem;
    ssl_certificate_key /etc/letsencrypt/live/playarr.example.com/privkey.pem;

    location / {
        proxy_pass http://127.0.0.1:9797;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
        proxy_http_version 1.1;
        proxy_set_header Upgrade $http_upgrade;
        proxy_set_header Connection $http_connection;
    }
}

server {
    listen 80;
    server_name playarr.example.com;
    return 301 https://$host$request_uri;
}
```

## Apache

### Subdomain

```apache
<VirtualHost *:80>
    ServerName playarr.example.com

    ProxyPreserveHost On
    ProxyPass / http://127.0.0.1:9797/
    ProxyPassReverse / http://127.0.0.1:9797/

    RewriteEngine on
    RewriteCond %{HTTP:Upgrade} websocket [NC]
    RewriteCond %{HTTP:Connection} upgrade [NC]
    RewriteRule ^/?(.*) "ws://127.0.0.1:9797/$1" [P,L]
</VirtualHost>
```

### Subpath

```apache
<Location /playarr>
    ProxyPass http://127.0.0.1:9797/playarr
    ProxyPassReverse http://127.0.0.1:9797/playarr
</Location>
```

Enable required modules:
```bash
sudo a2enmod proxy proxy_http proxy_wstunnel rewrite
sudo systemctl restart apache2
```

## Caddy

### Subdomain
```
playarr.example.com {
    reverse_proxy localhost:9797
}
```

### Subpath
```
example.com {
    handle_path /playarr/* {
        reverse_proxy localhost:9797
    }
}
```

## Traefik

### Docker Labels

```yaml
services:
  playarr:
    image: playarr/playarr:latest
    labels:
      - "traefik.enable=true"
      - "traefik.http.routers.playarr.rule=Host(`playarr.example.com`)"
      - "traefik.http.services.playarr.loadbalancer.server.port=9797"
```

## Important Notes

- **WebSocket support** is required for SignalR real-time updates. Ensure your proxy passes `Upgrade` and `Connection` headers.
- If using a subpath, you **must** set `UrlBase` in `config.xml` to match.
- CORS is handled by Playarr — no additional proxy headers needed.
