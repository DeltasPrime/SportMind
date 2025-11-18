## SportMind Viewer (Flask + S3)

Mini app Flask para visualizar las fechas disponibles y las sesiones almacenadas en S3 por la API de SportMind. Incluye una página web estática (HTML/CSS/JS) que consume endpoints locales del propio Flask.

### Contenidos
- `app.py`: servidor Flask con endpoints REST y página web.
- `requirements.txt`: dependencias de Python.
- `.env.example`: variables de entorno de ejemplo.
- `static/index.html`, `static/styles.css`, `static/app.js`: UI que lista fechas y sesiones.

---

## 1) Preparación de IAM (solo una vez)

Crea un usuario IAM (o rol) con permisos de solo lectura al bucket.

1. AWS Console → IAM → Users → Create user
2. Nombre: `sportmind-viewer`
3. Permissions → Attach policies directly → Create policy (JSON) con este contenido (ajusta el nombre del bucket):

```json
{
  "Version": "2012-10-17",
  "Statement": [
    {
      "Sid": "AllowListBucket",
      "Effect": "Allow",
      "Action": ["s3:ListBucket"],
      "Resource": "arn:aws:s3:::userdata-sportmind"
    },
    {
      "Sid": "AllowGetObjects",
      "Effect": "Allow",
      "Action": ["s3:GetObject"],
      "Resource": "arn:aws:s3:::userdata-sportmind/*"
    }
  ]
}
```

4. Adjunta la policy al usuario.
5. Crear Access Key/Secret (Programmatic access):
   - IAM → Users → `sportmind-viewer` → pestaña "Security credentials"
   - Sección "Access keys" → "Create access key"
   - Use case: "Application running outside AWS" → Next
   - Copia y guarda: `AWS_ACCESS_KEY_ID` y `AWS_SECRET_ACCESS_KEY`

> Producción: si alojas en EC2/ECS/Lambda, usa un Role con la misma policy en vez de Access Keys.

---

## 2) Clonar requisitos y crear venv

En Windows PowerShell (recomendado):

```powershell
cd webapps/sportmind_viewer
python -m venv .venv
.\.venv\Scripts\Activate.ps1
pip install -r requirements.txt
```

---

## 3) Configurar variables de entorno

Copia el archivo `.env.example` a `.env` y edítalo:

```powershell
Copy-Item env.example .env
```

Contenido a ajustar en `.env`:
- `S3_BUCKET_NAME`: nombre de tu bucket (ej: `userdata-sportmind`)
- `AWS_DEFAULT_REGION`: región (ej: `sa-east-1`)
- `AWS_ACCESS_KEY_ID` y `AWS_SECRET_ACCESS_KEY`: si corres local con usuario IAM

> Alternativa: en lugar de `.env`, puedes exportar variables en la terminal.

Opcional: usar API Gateway en vez de leer directo S3. Añade al `.env`:
```
USE_API_GATEWAY=true
API_GATEWAY_BASE_URL=https://xc06hens94.execute-api.sa-east-1.amazonaws.com/prod
API_GATEWAY_API_KEY=fyrKCyfgZ98gk6Y53tmBi1Z9fCsXG1U37FUDrCIv
```
Si `USE_API_GATEWAY=true`, la app llamará a `/data/dates` y `/data/{date}` de tu API usando esa API Key.

---

## 4) Ejecutar la app

```powershell
.\.venv\Scripts\Activate.ps1
python app.py
```

Abrir en navegador: `http://localhost:5000/`

Endpoints API locales:
- `GET /api/dates` → fechas disponibles
- `GET /api/sessions/<YYYY-MM-DD>` → sesiones de la fecha

---

## 5) Estructura de S3 esperada

```
data/
  YYYY-MM-DD/
    sportmind_data_YYYYMMDD_HHMMSS_<uuid>.json
```

Cada JSON contiene al menos:
```json
{
  "timestamp": "2025-01-15T10:30:00.000000",
  "session_id": "uuid-v4",
  "data": { ... }  // Payload enviado desde Unity
}
```

---

## 6) Notas y seguridad
- Nunca subas tus claves en git. `.env` está pensado para uso local.
- En producción usa Roles (IAM) o Secrets Manager/SSM Parameter Store.
- Los endpoints de Flask exponen solo lectura.
- La API Key de API Gateway NO es lo mismo que `AWS_ACCESS_KEY_ID/SECRET`. 
  - API Key (x-api-key): protege tu API Gateway.
  - Access Key/Secret: autentican a tu app contra AWS (S3, etc.).

---

## 7) Comandos útiles

Reinstalar dependencias:
```powershell
pip install --upgrade -r requirements.txt
```

Salir del venv:
```powershell
deactivate
```


