# Ski-Service Management Backend

## Projektbeschreibung
Das Ski-Service Management Backend dient zur Verwaltung von Serviceaufträgen und ermöglicht die Bearbeitung und Speicherung von Aufträgen in einer NoSQL-Datenbank (MongoDB). Die Web-API bietet CRUD-Funktionalitäten für die Auftragsverwaltung und ist in ASP.NET Core implementiert.

---

## Features
- **Benutzer-Authentifizierung** via JWT
- **CRUD-Operationen** für Serviceaufträge
- **MongoDB Integration** für Skalierbarkeit
- **REST API Endpunkte** für einfache Nutzung
- **Postman Collection** für Tests

---

## API Endpunkte

### Authentifizierung
- `POST /api/auth/register` → Benutzer registrieren
- `POST /api/auth/login` → Benutzer einloggen

### Auftragsmanagement
- `GET /api/auftrag` → Alle Aufträge abrufen
- `GET /api/auftrag/{id}` → Spezifischen Auftrag abrufen
- `POST /api/auftrag` → Neuen Auftrag erstellen
- `PUT /api/auftrag/{id}` → Auftrag aktualisieren
- `DELETE /api/auftrag/{id}` → Auftrag löschen

---

## Technologie-Stack
- **Programmiersprache:** C#
- **Framework:** ASP.NET Core 6.0
- **Datenbank:** MongoDB
- **API-Dokumentation:** Swagger (OpenAPI)
- **Test-Tool:** Postman
- **Versionierung:** GitHub Repository

---

## Installation & Setup

### Voraussetzungen
- .NET SDK 6.0 oder höher
- MongoDB
- Postman
- Git

### Schritte

1. **Umgebungsvariablen setzen**  (z. B. `MongoDBContext.cs`):
   ```json
        public MangoDBContext()
        {
            var connectionString = "Connection String";
            if (string.IsNullOrWhiteSpace(connectionString))
   ```
vergessin sie nicht zu eine Benutzer und eine Serviceauftraege collections zu ertellen


2. **Projekt starten**:
   ```bash
   dotnet run
   ```


---

## Tests
- **Postman Collection** zur API-Validierung nutzen
- **Unit Tests** mit xUnit implementiert

---

## Autor
**Projektteam:  Tunahan und Felipe**

