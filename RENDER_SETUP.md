# CattleFarm -- Render.com Deployment Guide

Render-e **CattleFarm** deploy করা খুবই সহজ কারণ প্রজেক্টটিতে **Dockerfile** এবং **render.yaml Blueprint** ইতোমধ্যে যুক্ত করা আছে।

---

## 🚀 Step-by-Step Deployment Guide

### Step 1: GitHub-e Code Push

প্রথমে তোমার পরিবর্তনগুলো GitHub repository (`https://github.com/farhan745/Cattle_Farm`)-এ push করো:

```bash
git add .
git commit -m "feat: add render.yaml blueprint for Render deployment"
git push origin main
```

---

### Step 2: Render.com Account & New Blueprint

1. **[render.com](https://render.com)**-এ গিয়ে Log in / Sign up করো।
2. Dashboard-এর উপরে ডানে **New +** বাটন ক্লিক করো।
3. **Blueprint** সিলেক্ট করো।
4. তোমার GitHub Account connect করে `farhan745/Cattle_Farm` repository-টি বেছে নাও।
5. Render স্বয়ংক্রিয়ভাবে repository-র `render.yaml` ফাইলটি ডিটেক্ট করবে।

---

### Step 3: Environment Variables Set

Render Blueprint পেজে নিচের Secret Key-গুলোর মান চাইবে:

| Variable | Recommendation / Value |
|---|---|
| `ConnectionStrings__DefaultConnection` | তোমার SQL Server Connection String (নিচে দেখুন) |
| `Email__Password` | Gmail App Password (যদি ইমেইল এলার্ট দরকার হয়) |
| `Jwt__Key` | Render স্বয়ংক্রিয়ভাবে 64-character random key তৈরি করে নেবে |

---

### Step 4: Database Connection String Setup

Render-এ standard PostgreSQL সার্ভিস থাকে, কিন্তু **CattleFarm** সি-শার্প প্রজেক্টটি SQL Server (EF Core) ব্যবহার করে। Database-এর জন্য নিচের যে কোনো একটি ব্যবহার করতে পারো:

#### Option A: Azure SQL Server (Recommended)
Free / Basic Tier Database ($5/month):
```text
Server=tcp:your-sql-server.database.windows.net,1433;Initial Catalog=CattleFarmDB;Persist Security Info=False;User ID=cattleadmin;Password=YourPassword123!;MultipleActiveResultSets=True;Encrypt=True;TrustServerCertificate=False;Connection Timeout=60;
```

#### Option B: Aiven / SmarterASP.NET / External SQL Host
```text
Server=YOUR_HOST,1433;Database=CattleFarmDB;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;
```

---

### Step 5: Apply & Deploy

1. **Apply** বাটনে ক্লিক করো।
2. Render তোমার Docker Container build করবে এবং **Singapore** Region-এ অ্যাপটি Deploy করে দেবে।
3. Build সফল হলে তোমার Web App URL তৈরি হয়ে যাবে:
   `https://cattlefarm-web.onrender.com`

---

## 🔍 Key Configuration Details

- **Docker File**: `./Dockerfile` (Multi-stage .NET 10 Container)
- **Port**: `8080` (Internal Docker ASPNETCORE URL)
- **Health Check Endpoint**: `/Health`
- **Region**: Singapore (Bangladesh-এর সবচেয়ে কাছে)
