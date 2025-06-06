 # JobSearchApp - Платформа для Поиска Работы

## Описание

JobSearchApp - это веб-приложение, предназначенное для упрощения процесса поиска работы для соискателей и публикации вакансий для работодателей. Платформа предоставляет удобный интерфейс для просмотра вакансий, отклика на них, а также для управления профилями пользователей и компаний.

## Функционал

*   **Для Соискателей:**
    *   Регистрация и аутентификация (JWT).
    *   Создание и редактирование профиля (резюме, навыки, опыт).
    *   Поиск вакансий по ключевым словам, категориям, местоположению.
    *   Просмотр деталей вакансии.
    *   Отклик на вакансии с приложением резюме (если функционал реализован).
    *   Просмотр статуса откликов.
*   **Для Работодателей:**
    *   Регистрация и аутентификация.
    *   Создание и редактирование профиля компании.
    *   Публикация и управление вакансиями (создание, редактирование, архивирование).
    *   Просмотр откликов на вакансии.
    *   (Возможно) Поиск кандидатов по базе резюме.
*   **API:**
    *   RESTful API для взаимодействия между фронтендом и бэкендом.
    *   Документация API доступна через Swagger UI.

## Технологический Стек

*   **Бэкенд:** ASP.NET Core 7 Web API (C#)
*   **База данных:** PostgreSQL
*   **ORM:** Entity Framework Core 7
*   **Аутентификация:** JWT (JSON Web Tokens)
*   **Фронтенд:** (React)
*   **Управление БД:** DBeaver

## Предварительные требования

Перед началом установки убедитесь, что у вас установлены:

1.  **[.NET 7 SDK](https://dotnet.microsoft.com/download/dotnet/7.0)**
2.  **[Node.js и npm](https://nodejs.org/)** (если используется JavaScript-фреймворк для фронтенда)
3.  **[PostgreSQL](https://www.postgresql.org/download/)**
4.  **[DBeaver](https://dbeaver.io/download/)** (или другой SQL-клиент для PostgreSQL, например, pgAdmin)
5.  **[Git](https://git-scm.com/downloads)**

## Установка и Запуск

### 1. Клонирование репозитория

```bash
git clone <URL_вашего_репозитория>
cd <название_папки_проекта>
```

### 2. Настройка базы данных PostgreSQL (с использованием DBeaver)

1.  **Запустите DBeaver** и создайте новое подключение к вашему серверу PostgreSQL.
    *   Нажмите `Database` -> `New Database Connection`.
    *   Выберите `PostgreSQL` и нажмите `Next`.
    *   Введите данные вашего сервера PostgreSQL (хост, порт, имя пользователя администратора (обычно `postgres`), пароль) и нажмите `Finish`.
2.  **Создайте базу данных:**
    *   В панели `Database Navigator` щелкните правой кнопкой мыши по вашему подключению PostgreSQL.
    *   Выберите `Create` -> `Database`.
    *   Введите имя для вашей базы данных (например, `jobsearchapp_db`) и нажмите `OK`.
3.  **(Рекомендуется) Создайте отдельного пользователя для приложения:**
    *   В панели `Database Navigator` разверните ваше подключение, затем разверните `Roles`.
    *   Щелкните правой кнопкой мыши по `Roles` и выберите `Create` -> `Role`.
    *   Введите имя пользователя (например, `jobsearch_user`).
    *   Перейдите на вкладку `Definition` и задайте пароль для пользователя.
    *   Перейдите на вкладку `Privileges` и убедитесь, что у пользователя есть права `LOGIN`. Нажмите `OK`.
    *   Теперь предоставьте этому пользователю права на созданную базу данных:
        *   Откройте SQL Editor для вашей основной базы данных (`postgres` или другой административной).
        *   Выполните SQL-команду (замените имена при необходимости):
            ```sql
            GRANT ALL PRIVILEGES ON DATABASE jobsearchapp_db TO jobsearch_user;
            -- Если вы хотите сделать пользователя владельцем БД (опционально):
            -- ALTER DATABASE jobsearchapp_db OWNER TO jobsearch_user;
            ```
4.  **Сформируйте строку подключения:**
    Ваша строка подключения будет выглядеть примерно так (замените значения вашими):
    `Host=localhost;Port=5432;Database=jobsearchapp_db;Username=jobsearch_user;Password=Ваш_пароль;`

### 3. Настройка и запуск Бэкенда (API)

1.  **Перейдите в папку API:**
    ```bash
    cd JobSearchApp.API
    ```
2.  **Настройте секреты:**
    Используйте .NET User Secrets для хранения конфиденциальных данных в локальной разработке.
    *   Инициализируйте user secrets:
        ```bash
        dotnet user-secrets init
        ```
    *   Задайте строку подключения (используйте строку, полученную на шаге 2.4):
        ```bash
        dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=jobsearchapp_db;Username=jobsearch_user;Password=Ваш_пароль;"
        ```
    *   Задайте параметры JWT (замените значения на ваши **надежные** секреты):
        ```bash
        dotnet user-secrets set "Jwt:Key" "ВАШ_СУПЕР_СЕКРЕТНЫЙ_И_ДЛИННЫЙ_КЛЮЧ_ДЛЯ_ПОДПИСИ_JWT"
        dotnet user-secrets set "Jwt:Issuer" "JobSearchAppIssuer"  # Например
        dotnet user-secrets set "Jwt:Audience" "JobSearchAppClient" # Например
        ```
    *   **Важно:** Для продакшен-среды используйте переменные окружения или другие безопасные конфигурационные провайдеры вместо User Secrets.
3.  **Запустите API:**
    ```bash
    dotnet run
    ```
    При первом запуске Entity Framework Core автоматически создаст таблицы в базе данных на основе миграций. Если миграций нет или они не актуальны, возможно, потребуется их создать (`dotnet ef migrations add InitialCreate`) и применить (`dotnet ef database update`).

### 4. Настройка и запуск Фронтенда

**(Предполагается, что папка фронтенда называется `JobSearchApp.UI` и используется npm)**

1.  **Перейдите в папку фронтенда:**
    ```bash
    cd ../JobSearchApp.UI # Или другое имя вашей папки фронтенда
    ```
2.  **Установите зависимости:**
    ```bash
    npm install
    ```
3.  **Настройте адрес API:**
    *   Найдите файл конфигурации фронтенда (например, `.env`, `environment.ts`, `config.js`).
    *   Укажите URL вашего запущенного API (обычно `https://localhost:7XXX` или `http://localhost:5XXX`, точный порт смотрите в выводе команды `dotnet run`). Например:
        ```
        # В файле .env (для React/Vue)
        REACT_APP_API_BASE_URL=https://localhost:7123
        # или VUE_APP_API_BASE_URL=https://localhost:7123

        // В файле environment.ts (для Angular)
        export const environment = {
          production: false,
          apiBaseUrl: 'https://localhost:7123'
        };
        ```
    *   **Замените `7123` на фактический порт вашего API.**
4.  **Запустите фронтенд:**
    ```bash
    npm start
    ```
    Приложение должно открыться в вашем браузере.

## Документация API (Swagger)

После запуска бэкенда интерактивная документация API доступна по адресу:
`https://localhost:<порт_API>/swagger`
(Замените `<порт_API>` на фактический порт вашего API).

Через Swagger UI вы можете просматривать все доступные эндпоинты, их параметры, а также отправлять тестовые запросы (после аутентификации, если эндпоинт защищен).

## Структура Проекта

*   `JobSearchApp.API/`: Проект ASP.NET Core Web API (бэкенд, контроллеры, настройка).
*   `JobSearchApp.Core/`: Основная бизнес-логика, сущности (Entities), интерфейсы репозиториев, сервисов.
*   `JobSearchApp.Infrastructure/`: Реализация доступа к данным (EF Core DbContext, репозитории), реализация внешних сервисов (например, работа с файлами).
*   `JobSearchApp.UI/` (или другое имя): Проект фронтенда (React/Vue/Angular/etc.).
*   `.gitignore`: Файл для исключения ненужных файлов из Git.
*   `README.md`: Этот файл.
*   `JobSearchApp.sln`: Файл решения Visual Studio.

---
