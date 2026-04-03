# Railway Deployment Guide — Social Marketplace API

## Prerequisites

- [Railway account](https://railway.app) (free tier works for testing)
- [Railway CLI](https://docs.railway.app/develop/cli) (optional but useful): `npm i -g @railway/cli`
- Your PostgreSQL database (Supabase or Railway Postgres plugin)
- Your Redis instance (Upstash or Railway Redis plugin)

---

## Step 1 — Create a Railway Project

1. Go to [railway.app/new](https://railway.app/new)
2. Click **"Deploy from GitHub repo"**
3. Authorize Railway to access your GitHub account
4. Select the `socialcrooseapp` repo
5. Railway will detect the `Dockerfile` inside `backend/` automatically

> If your `backend/` folder is a subdirectory of the repo root, set the **Root Directory** to `backend` in:
> **Project → Service → Settings → Source → Root Directory** = `backend`

---

## Step 2 — Add Services (Database & Cache)

### Option A — Use Railway's built-in plugins (easiest)

In your Railway project dashboard:

1. Click **"+ New"** → **"Database"** → **"PostgreSQL"**
   - Railway provisions a Postgres instance and injects `DATABASE_URL` automatically
2. Click **"+ New"** → **"Database"** → **"Redis"**
   - Railway injects `REDIS_URL` automatically

### Option B — Use external services (Supabase + Upstash)

You already have a Supabase database. Just supply the connection strings as env vars (Step 3).

---

## Step 3 — Set Environment Variables

Go to your Railway service → **Variables** tab and add every variable below.  
**Never put secrets in `appsettings.json`** — use env vars in Railway.

### Required

| Variable | Example value | Notes |
|---|---|---|
| `ConnectionStrings__DefaultConnection` | `Host=...;Port=5432;Database=postgres;Username=...;Password=...;SSL Mode=Require;Trust Server Certificate=true` | Your Supabase / Postgres connection string |
| `ConnectionStrings__Redis` | `redis://default:password@host:6379` | Redis connection string |
| `Jwt__Key` | `a-random-256-bit-secret-min-32-chars!!` | Generate with: `openssl rand -base64 32` |
| `Jwt__Issuer` | `SocialMarketplace` | Must match what your frontend expects |
| `Jwt__Audience` | `SocialMarketplace` | Must match what your frontend expects |
| `Cors__AllowedOrigins__0` | `https://your-frontend.vercel.app` | Your deployed frontend URL |

### Optional but recommended

| Variable | Value | Notes |
|---|---|---|
| `Jwt__ExpiryMinutes` | `60` | Token lifetime in minutes |
| `Redis__Enabled` | `true` | Set `false` to disable Redis entirely |
| `SignalR__UseRedisBackplane` | `true` | Set `false` if no Redis (single-instance only) |
| `ASPNETCORE_ENVIRONMENT` | `Production` | Already set in Dockerfile; can override here |
| `Cache__DefaultExpirationMinutes` | `30` | L2 Redis cache TTL |
| `Cache__L1ExpirationSeconds` | `30` | In-memory L1 cache TTL |

### OpenTelemetry (optional)

| Variable | Value |
|---|---|
| `OTEL_EXPORTER_OTLP_ENDPOINT` | `https://your-collector:4317` |
| `OTEL_SERVICE_NAME` | `SocialMarketplace.Api` |

---

## Step 4 — Generate a Strong JWT Key

Run this locally (PowerShell):

```powershell
[Convert]::ToBase64String((1..32 | ForEach-Object { [byte](Get-Random -Max 256) }))
```

Or in bash/WSL:

```bash
openssl rand -base64 32
```

Paste the output as `Jwt__Key` in Railway Variables. It must be at least 32 characters.

---

## Step 5 — Set the Port

Railway automatically injects a `PORT` environment variable at runtime. The Dockerfile already handles this:

```dockerfile
ENV ASPNETCORE_URLS=http://+:${PORT:-8080}
```

No action needed — Railway wires `PORT` up automatically.

---

## Step 6 — Configure CORS for Your Frontend

Add all frontend URLs as separate indexed variables:

```
Cors__AllowedOrigins__0 = https://your-app.vercel.app
Cors__AllowedOrigins__1 = https://your-custom-domain.com
```

The `__0`, `__1` syntax is the ASP.NET Core array binding convention for environment variables.

---

## Step 7 — Deploy

### Via GitHub (recommended)

Every `git push` to your configured branch triggers an automatic deploy.

```bash
git add .
git commit -m "deploy: railway config"
git push origin main
```

### Via Railway CLI

```bash
# From the backend/ folder
railway login
railway link          # link to your Railway project
railway up            # deploy current directory
```

---

## Step 8 — Run Database Migrations

After first deploy, run EF Core migrations. Options:

### Option A — Using Railway CLI (easiest)

```bash
# From backend/ folder locally, pointed at production DB
railway run dotnet ef database update --project Marketplace.Database --startup-project Marketplace.Api
```

### Option B — Seed endpoint (already built in)

Your app exposes:
- `POST /api/admin/seed` — seeds demo data + super admin
- `POST /api/admin/seed-superadmin` — seeds super admin only

After deploy, call these once via curl or Postman:

```bash
curl -X POST https://your-app.up.railway.app/api/admin/seed
```

> Remove or protect these endpoints before going to production.

### Option C — Auto-migrate on startup (add to Program.cs)

```csharp
// After var app = builder.Build();
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MarketplaceDbContext>();
    await db.Database.MigrateAsync();
}
```

---

## Step 9 — Verify Deployment

Once Railway shows **"Deploy Success"**:

```bash
# Health check
curl https://your-app.up.railway.app/health

# API info
curl https://your-app.up.railway.app/

# Swagger UI (only in Development env — disable in Production or protect it)
open https://your-app.up.railway.app/swagger
```

---

## Step 10 — Set a Custom Domain (optional)

1. Railway dashboard → Service → **Settings** → **Domains**
2. Click **"+ Custom Domain"**
3. Add a CNAME record at your DNS provider pointing to the Railway-provided value

---

## Enabling Swagger in Production (optional)

By default Swagger only mounts in Development. To enable it in Production, edit `Program.cs`:

```csharp
// Change this:
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwaggerUI(...);
}

// To this (protect with a header or basic auth in real production):
app.MapOpenApi();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/openapi/v1.json", "Social Marketplace API v1");
    c.RoutePrefix = "swagger";
});
```

---

## Environment Variable Reference (Complete)

All ASP.NET Core configuration keys can be overridden with environment variables by replacing `:` with `__`:

| `appsettings.json` key | Environment variable |
|---|---|
| `ConnectionStrings:DefaultConnection` | `ConnectionStrings__DefaultConnection` |
| `ConnectionStrings:Redis` | `ConnectionStrings__Redis` |
| `Jwt:Key` | `Jwt__Key` |
| `Jwt:Issuer` | `Jwt__Issuer` |
| `Jwt:Audience` | `Jwt__Audience` |
| `Jwt:ExpiryMinutes` | `Jwt__ExpiryMinutes` |
| `Cors:AllowedOrigins:0` | `Cors__AllowedOrigins__0` |
| `Redis:Enabled` | `Redis__Enabled` |
| `SignalR:UseRedisBackplane` | `SignalR__UseRedisBackplane` |
| `Cache:DefaultExpirationMinutes` | `Cache__DefaultExpirationMinutes` |
| `Cache:L1ExpirationSeconds` | `Cache__L1ExpirationSeconds` |
| `Router:LoadFactorThreshold` | `Router__LoadFactorThreshold` |

---

## Security Checklist Before Going Live

- [ ] Remove hardcoded credentials from `appsettings.json` and rotate them
- [ ] Rotate the Supabase database password (it is currently in the repo)
- [ ] Use a strong, randomly-generated `Jwt__Key` (not the placeholder)
- [ ] Remove or protect `/api/admin/seed` and `/api/admin/seed-superadmin`
- [ ] Restrict Swagger to non-production environments
- [ ] Set `ASPNETCORE_ENVIRONMENT=Production` in Railway Variables

---

## Troubleshooting

| Problem | Fix |
|---|---|
| Build fails — SDK not found | Ensure `Dockerfile` is in `backend/` and Railway Root Directory = `backend` |
| `JWT key not configured` crash | Set `Jwt__Key` in Railway Variables |
| Database connection refused | Check `ConnectionStrings__DefaultConnection` variable; ensure Supabase allows Railway's egress IPs |
| Redis errors but Redis is optional | Set `Redis__Enabled=false` and `SignalR__UseRedisBackplane=false` to run without Redis |
| `PORT` binding errors | Do not hardcode port in variables; Railway manages `PORT` automatically |
| Health check fails | Check `/health` responds 200; ensure DB migration has run |
