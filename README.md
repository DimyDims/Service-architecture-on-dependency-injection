# Service-architecture-on-dependency-injection
Загрузка всех необходимых менеджеров в виде сервисов с помощью внедрения зависимостей


В качестве примера загрузчика сервисов предоставлен класс HUB. Который добавляет в коллекцию сервисов все необходимые ему сервисы.
И класс ResourcePanel который вызывает необходимый ему сервис с помощью Constructor Injection.
