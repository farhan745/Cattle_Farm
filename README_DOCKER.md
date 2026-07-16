# 🐄 CattleFarm — Docker Setup & Deployment Guide

## ফাইল স্ট্রাকচার

```
CattleFarm/
├── Dockerfile                    ← Docker image build instructions
├── docker-compose.yml            ← Local development (app + SQL Server)
├── docker-compose.prod.yml       ← Production server deployment
├── .dockerignore                 ← Docker build exclusions
├── .env.example                  ← Environment variables template
├── nginx/
│   └── nginx.conf                ← Nginx reverse proxy config
└── .github/
    └── workflows/
        ├── dotnet-ci.yml         ← PR check (build + test)
        └── deploy.yml            ← Main branch: build → push → deploy
```

---

## 🖥️ Local Development (Docker দিয়ে চালানো)

### Step 1: Prerequisites
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) install করো

### Step 2: Environment setup
```bash
# .env.example কপি করো
cp .env.example .env

# .env file খুলে সব values fill করো
```

### Step 3: Start করো
```bash
# প্রথমবার build করতে হবে
docker compose up --build -d

# পরেরবার (code change হলে)
docker compose up --build -d app

# সব বন্ধ করতে
docker compose down
```

### Step 4: App দেখো
- 🌐 **App**: http://localhost:8080
- ❤️ **Health**: http://localhost:8080/Health
- 🗄️ **SQL Server**: localhost:1433 (SA password .env-এ দেওয়া)

### Useful Commands
```bash
# App logs দেখো
docker compose logs -f app

# DB logs দেখো
docker compose logs -f db

# Container-এ ঢুকো (debug করতে)
docker compose exec app bash

# Database reset (সব data মুছে যাবে!)
docker compose down -v
docker compose up -d
```

---

## 🚀 Auto-Deploy Setup (GitHub Actions)

`main` branch-এ code push করলে automatically deploy হবে।

### Step 1: GitHub Secrets সেট করো

**GitHub Repository → Settings → Secrets and variables → Actions → New repository secret**

| Secret Name | Value |
|---|---|
| `DB_SA_PASSWORD` | SQL Server SA password (strong password) |
| `JWT_KEY` | JWT secret key (64+ characters) |
| `EMAIL_USERNAME` | Gmail address |
| `EMAIL_PASSWORD` | Gmail App Password |
| `SSLCOMMERZ_STORE_ID` | SSLCommerz Store ID |
| `SSLCOMMERZ_STORE_PASSWORD` | SSLCommerz Store Password |
| `TWILIO_ACCOUNT_SID` | Twilio Account SID (optional) |
| `TWILIO_AUTH_TOKEN` | Twilio Auth Token (optional) |
| `TWILIO_FROM_PHONE` | Twilio phone number (optional) |

**VPS দিয়ে deploy করলে এগুলোও লাগবে:**

| Secret Name | Value |
|---|---|
| `SSH_HOST` | Server IP address |
| `SSH_USER` | SSH username (ubuntu, root, etc.) |
| `SSH_PRIVATE_KEY` | SSH private key (`cat ~/.ssh/id_rsa`) |
| `SSH_PORT` | SSH port (usually 22) |

### Step 2: GitHub Variables সেট করো

**Settings → Secrets and variables → Actions → Variables tab → New repository variable**

| Variable Name | Value |
|---|---|
| `DEPLOY_ENABLED` | `true` (deploy activate করতে) |
| `PRODUCTION_URL` | `https://your-domain.com` |

### Step 3: Server Setup (VPS)

Server-এ SSH করে এই কাজগুলো করো:

```bash
# Docker install (Ubuntu)
curl -fsSL https://get.docker.com | sh
sudo usermod -aG docker $USER

# App directory তৈরি করো
mkdir -p ~/cattlefarm/nginx/ssl

# Production files copy করো
# (GitHub Actions এটা করতে পারে, অথবা manually করো)
cd ~/cattlefarm

# .env file তৈরি করো server-এ
nano .env
# (সব values fill করো)

# docker-compose.prod.yml copy করো server-এ
# (সব values fill করো)
```

### Step 4: GHCR Package Permissions

GitHub-এ package access দিতে হবে:
- **GitHub → Profile → Packages → cattlefarm → Package Settings**
- Visibility: **Private** বা **Public**
- Repository access: তোমার repository add করো

---

## 🔄 Workflow কীভাবে কাজ করে

```
git push origin main
        ↓
GitHub Actions triggers 'deploy.yml'
        ↓
Job 1: Tests run
        ↓ (pass হলে)
Job 2: Docker image build
        ghcr.io/USERNAME/cattlefarm:latest
        ghcr.io/USERNAME/cattlefarm:sha-abc1234
        ↓
Job 3: SSH → Server
        docker compose pull app
        docker compose up -d --no-deps app
        ↓
✅ New version live! (zero downtime)
```

---

## 🌐 Nginx + HTTPS Setup

### Free SSL with Let's Encrypt

```bash
# Certbot install
sudo apt install certbot

# Certificate নাও
sudo certbot certonly --standalone -d your-domain.com -d www.your-domain.com

# Certificates copy করো
sudo cp /etc/letsencrypt/live/your-domain.com/fullchain.pem ~/cattlefarm/nginx/ssl/
sudo cp /etc/letsencrypt/live/your-domain.com/privkey.pem ~/cattlefarm/nginx/ssl/
```

### nginx.conf update করো
```nginx
server_name your-domain.com www.your-domain.com;
```

---

## ❓ Troubleshooting

### App start হচ্ছে না?
```bash
docker compose logs app
```

### Database connect হচ্ছে না?
```bash
# DB healthy কিনা দেখো
docker compose ps
docker compose logs db
```

### Image pull হচ্ছে না?
```bash
# GHCR-এ login করো
echo "YOUR_GITHUB_TOKEN" | docker login ghcr.io -u YOUR_USERNAME --password-stdin
```

### Port already in use?
```bash
# কোন process port use করছে
netstat -tlnp | grep 8080
# বা
docker compose down
```
