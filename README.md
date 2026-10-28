# FinanceTracker

Сервис личного финансового учёта: REST API на .NET 10 (тестировался в Swagger) и SPA-клиент на React, который сейчас в разработке.

Проект позволяет вести доходы и расходы по категориям, фильтровать транзакции и смотреть аналитику за период: баланс, разбивку по категориям, дневной тренд и топ расходов.

> **Статус:** бэкенд готов и работает; фронтенд находится в разработке.

## Возможности

- Регистрация, вход и выход; JWT хранится в `HttpOnly` cookie.
- Транзакции: CRUD, фильтры по датам, категории и типу, серверная пагинация.
- Категории: системные (общие) и пользовательские.
- Аналитика поверх PostgreSQL/Dapper:
  - сводка доходов/расходов и баланса;
  - разбивка по категориям;
  - дневной тренд;
  - топ расходов за период.
- EF Core-миграции и автоматический сидинг системных категорий при старте.

## Стек

**Backend**

- .NET 10 / ASP.NET Core Web API
- Clean Architecture (Domain → Application → Infrastructure → Api)
- EF Core 10 + Npgsql (PostgreSQL 16)
- Dapper для агрегирующих запросов аналитики
- JWT (`Microsoft.AspNetCore.Authentication.JwtBearer`), BCrypt
- `CSharpFunctionalExtensions` (`Result<T, AppError>`)
- Swagger (только в Development)

**Frontend** (`client/`, в разработке)

- React 19 + TypeScript + Vite
- Tailwind CSS v4
- TanStack Query v5, Axios
- React Router, React Hook Form + Zod
- Recharts, Lucide, date-fns

## Архитектура

```
FinanceTracker.Domain          сущности и enum'ы, без внешних зависимостей
FinanceTracker.Application     сервисы, DTO, интерфейсы, Result/AppError
FinanceTracker.Infrastructure  EF Core, миграции, Dapper, JWT, хеширование
FinanceTracker.Api             контроллеры, middleware, DI, Swagger
```

Правило зависимостей: `Api → Infrastructure → Application → Domain`. Application не ссылается на EF Core.

## Структура репозитория

```
.
├─ src/
│  ├─ FinanceTracker.Api/
│  ├─ FinanceTracker.Application/
│  ├─ FinanceTracker.Domain/
│  └─ FinanceTracker.Infrastructure/
├─ client/                     # React + Vite (в разработке)
├─ docker-compose.yml          # PostgreSQL + Adminer
├─ FinanceTracker.slnx         # solution (XML-формат)
└─ AGENTS.md                   # заметки для разработки/агентов
```

## Быстрый старт

### Требования

- .NET 10 SDK
- Docker (для PostgreSQL)
- Node.js 20+ и npm (для клиента)

### 1. База данных

```bash
docker compose up -d
```

Поднимется PostgreSQL 16 на `localhost:5433` с базой `finance_db` и Adminer на <http://localhost:8080>.

### 2. API

```bash
dotnet run --project src/FinanceTracker.Api
```

При старте автоматически применяются миграции и создаются системные категории.

- HTTP: <http://localhost:5264>
- HTTPS: <https://localhost:7274>
- Swagger: <http://localhost:5264/swagger> (только Development)

### 3. Клиент

```bash
cd client
npm install
npm run dev
```

Приложение откроется на <http://localhost:5173>. В `client/.env.local` должен быть указан адрес API:

```
VITE_API_BASE_URL=http://localhost:5264
```

Доступные скрипты: `npm run dev`, `npm run build`, `npm run lint`, `npm run preview`.

## Конфигурация

Основные настройки — `src/FinanceTracker.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "ApplicationDbContext": "Host=localhost;Port=5433;Database=finance_db;Username=postgres;Password=superuser-psswd"
  },
  "JwtOptions": {
    "SecretKey": "...",
    "Issuer": "FinanceTrackerApi",
    "Audience": "FinanceTrackerClient",
    "ExpiryMinutes": 720
  }
}
```

Секреты в репозитории предназначены только для локальной разработки. Для деплоя используйте user-secrets или переменные окружения (`ConnectionStrings__ApplicationDbContext`, `JwtOptions__SecretKey`).

## API

Все маршруты `/api/users/*`, кроме `me`, публичные; транзакции, категории и аналитика требуют авторизации.

| Метод | Маршрут | Описание |
|---|---|---|
| `POST` | `/api/users/register` | Регистрация |
| `POST` | `/api/users/login` | Вход, устанавливает cookie |
| `POST` | `/api/users/logout` | Выход, удаляет cookie |
| `GET` | `/api/users/me` | Текущий пользователь |
| `GET` | `/api/transactions` | Список с фильтрами и пагинацией |
| `GET` | `/api/transactions/{id}` | Транзакция по id |
| `POST` | `/api/transactions` | Создать транзакцию |
| `PUT` | `/api/transactions/{id}` | Обновить транзакцию |
| `DELETE` | `/api/transactions/{id}` | Удалить транзакцию |
| `GET` | `/api/categories` | Системные и пользовательские категории |
| `GET` | `/api/categories/{id}` | Категория по id |
| `POST` | `/api/categories` | Создать категорию |
| `PUT` | `/api/categories/{id}` | Переименовать категорию |
| `DELETE` | `/api/categories/{id}` | Удалить категорию |
| `GET` | `/api/analytics/summary` | Сводка за период |
| `GET` | `/api/analytics/trend` | Дневной тренд |
| `GET` | `/api/analytics/topexpenses` | Топ расходов |

### Параметры

- `GET /api/transactions`: `fromDate`, `toDate`, `categoryId`, `type` (`1` — доход, `2` — расход), `page` (≥ 1), `pageSize` (1..100).
- Аналитика: обязательные `fromDate`, `toDate` (ISO-8601 UTC); для `/topexpenses` также `topCount` (1..100).
- `/trend` ограничен периодом в 366 дней.
- Ответы-списки транзакций возвращают `PagedResult` с `totalCount` и `totalPages`.

### Аутентификация

JWT передаётся не в заголовке `Authorization`, а в `HttpOnly` cookie `tasty-cookie` (`SameSite=Strict`). Frontend обязан отправлять запросы с `withCredentials: true`. CORS в development разрешает только `http://localhost:5173`.

Пример с curl:

```bash
curl -c cookies.txt -X POST http://localhost:5264/api/users/login \
  -H 'Content-Type: application/json' \
  -d '{"email":"user@example.com","password":"secret"}'

curl -b cookies.txt http://localhost:5264/api/users/me
```

### Формат ошибок

Доменные ошибки возвращают `{ "error": "..." }` с соответствующим статусом (`400`, `401`, `404`, `409`). Непредвиденные исключения — `ProblemDetails` через глобальный обработчик.

## EF Core миграции

Из корня репозитория:

```bash
dotnet ef migrations add <Name> \
  --project src/FinanceTracker.Infrastructure \
  --startup-project src/FinanceTracker.Api
```
