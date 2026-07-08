PR130/2022 Darko Bozic
# PUGS — Planer Putovanja

Web aplikacija za planiranje putovanja, razvijena kao projektni zadatak iz predmeta *Primena veb programiranja u infrastrukturnim sistemima*.

## Tehnologije

**Backend**
- Microsoft Service Fabric (mikroservisna arhitektura)
- ASP.NET Core Web API (.NET)
- Entity Framework Core + Microsoft SQL Server
- Service Fabric Remoting (međuservisna komunikacija)
- JWT autentikacija, BCrypt heširanje lozinki
- LDAP (System.DirectoryServices.Protocols) — SSO nadogradnja
- YARP — API Gateway / reverse proxy
- QuestPDF — generisanje PDF izveštaja
- QRCoder — generisanje QR kodova za deljenje

**Frontend**
- React (Vite)
- Material UI (MUI)
- Context API (upravljanje stanjem)
- React Router
- Axios

**Infrastruktura**
- Docker (OpenLDAP simulacija Active Directory)

---

## Arhitektura sistema

Sistem se sastoji od 5 Service Fabric servisa:

| Servis | Tip | Odgovornost |
|---|---|---|
| **ApiGateway** | Stateless | Jedinstvena ulazna tačka, rutiranje ka ostalim servisima |
| **AuthService** | Stateless | Registracija, prijava (obična i LDAP), JWT, admin upravljanje nalozima |
| **TravelPlanningService** | Stateful | Planovi putovanja, destinacije, aktivnosti, checklist |
| **BudgetService** | Stateful | Evidencija troškova, automatski obračun budžeta |
| **SharingService** | Stateless | QR kod / linkovi za deljenje planova (VIEW/EDIT pristup) |

Svaki servis ima svoju odvojenu SQL Server bazu podataka (1 servis = 1 baza). Servisi komuniciraju međusobno preko Service Fabric Remoting-a.

Detaljan dijagram arhitekture i Use Case dijagram nalaze se u `docs/` folderu.

---

## Preduslovi

Pre pokretanja, potrebno je instalirati:

1. **Visual Studio 2022** sa workload-om `Azure development` (donosi Service Fabric SDK)
2. **Microsoft SQL Server Express** (ili slična edicija) — [preuzimanje](https://www.microsoft.com/en-us/sql-server/sql-server-downloads)
3. **SQL Server Management Studio (SSMS)** — za podešavanje baze
4. **Node.js** (v18+) i npm — za frontend
5. **Docker Desktop** — za LDAP server (SSO nadogradnja)

---

## 1. Podešavanje SQL Servera

Aplikacija koristi **SQL Server Authentication** (ne Windows Authentication), zbog kompatibilnosti sa Service Fabric procesima.

1. U SSMS-u, poveži se na svoju SQL Server Express instancu (Windows Authentication)
2. Omogući Mixed Mode Authentication: desni klik na server → **Properties → Security** → **SQL Server and Windows Authentication mode** → restartuj SQL Server servis
3. Kreiraj login preko New Query:

```sql
CREATE LOGIN pugs_app WITH PASSWORD = 'Pugs2026!', CHECK_POLICY = OFF;
GO
ALTER SERVER ROLE sysadmin ADD MEMBER pugs_app;
GO
```

> Baze podataka (`PUGS_AuthDb`, `PUGS_TravelPlanningDb`, `PUGS_BudgetDb`, `PUGS_SharingDb`) kreiraju se automatski prilikom pokretanja EF Core migracija (korak 3).

---

## 2. Podešavanje LDAP servera (za SSO nadogradnju)

U `backend/ldap-docker/` folderu:

```powershell
docker compose up -d
```

Ovo pokreće OpenLDAP server (port 389) sa unapred definisanim test nalozima:

| Korisničko ime | Lozinka | Napomena |
|---|---|---|
| `marko.markovic` | `test123` | Standardni korisnik |
| `ana.admin` | `admin123` | Korisnik koji se može promovisati u Admin rolu preko admin panela |

Grafički pregled LDAP strukture dostupan je na `http://localhost:8081` (phpLDAPadmin), login: `cn=admin,dc=pugs,dc=local` / `AdminPass123`.

---

## 3. Pokretanje backend-a

1. Otvori `backend/PUGS/PUGS.sln` u Visual Studio-u
2. Proveri da svaki servis ima ispravan connection string u svom `appsettings.json` (koriste zajednički `pugs_app` login kreiran u koraku 1)
3. EF Core migracije se automatski primenjuju na prvo pokretanje ako je `Update-Database` prethodno izvršen za svaki servis (`AuthService`, `TravelPlanningService`, `BudgetService`, `SharingService`) — ako baze ne postoje, pokreni u Package Manager Console za svaki servis:
   ```powershell
   Update-Database -Context <ImeDbContext-a>
   ```
4. Postavi **`PUGS.ServiceFabricApp`** kao Startup Project
5. Pokreni sa **F5**

Prvo pokretanje traje malo duže (Service Fabric lokalni klaster se podiže). Status servisa možeš pratiti u **Service Fabric Explorer**: `http://localhost:19080/Explorer`.

API Gateway je dostupan na fiksnom portu (`http://localhost:8790`) — svi ostali servisi imaju dinamički dodeljene portove koje Gateway interno rešava.

---

## 4. Pokretanje frontenda

```powershell
cd frontend
npm install
npm run dev
```

Frontend je dostupan na `http://localhost:5173`.

Proveri da `frontend/.env` sadrži:
```
VITE_API_BASE_URL=http://localhost:8790/api
```

---

## Redosled pokretanja (sažetak)

1. SQL Server Express servis pokrenut (obično automatski)
2. `docker compose up -d` u `ldap-docker/` (za LDAP prijavu)
3. F5 u Visual Studio-u (backend, `PUGS.ServiceFabricApp`)
4. `npm run dev` u `frontend/` folderu

---

## Test nalozi

| Tip | Email / Korisničko ime | Lozinka |
|---|---|---|
| Standardna registracija | (registruj sopstveni preko `/register`) | — |
| LDAP korisnik | `marko.markovic` | `test123` |
| LDAP korisnik (potencijalni admin) | `ana.admin` | `admin123` |

> Napomena: prvi LDAP login automatski kreira lokalni nalog sa `User` rolom. Da bi neki nalog postao Admin, potrebno je da postojeći Admin promeni njegovu rolu preko Admin panela (`/admin`), ili se rola ručno postavi u bazi za prvog admina:
> ```sql
> UPDATE Users SET Role = 1 WHERE Email = 'email@primer.com';
> ```
> (`Role = 1` odgovara vrednosti `Admin`)

---

## Struktura repozitorijuma

```
PUGS/
├── backend/
│   ├── PUGS.sln
│   ├── PUGS.ServiceFabricApp/
│   ├── ApiGateway/
│   ├── AuthService/
│   ├── TravelPlanningService/
│   ├── BudgetService/
│   ├── SharingService/
│   ├── PUGS.Common/
│   └── ldap-docker/
├── frontend/
└── docs/
    ├── use-case-diagram.png
    ├── architecture-diagram.png
    └── README.md (ovaj fajl)
```

---

## Dodatna nadogradnja

Projekat implementira **SSO prijavu putem LDAP-a** kao izabranu dodatnu nadogradnju — vidi `AuthService/Ldap/` i `backend/ldap-docker/`.